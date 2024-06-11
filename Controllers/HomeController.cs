using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using TradeShop.Models;
using System.Web.Security;
using TradeShop.common;
using TradeShop.ViewModels;

using System.IO;
using PagedList;
using PagedList.Mvc;

namespace TradeShop.Controllers
{
    public class HomeController : Controller
    {
        TradeShopDataContext data = new TradeShopDataContext();

        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public ActionResult Index()
        {
            var danhMucList = data.DanhMucs.ToList();
            ViewBag.DanhMucList = danhMucList;

            var random = new Random();
            var suggestedProducts = data.SanPhams.Where(m => m.status == 1)
                                                 .ToList()
                                                 .OrderBy(x => random.Next())
                                                 .Take(18)
                                                 .ToList();

            if (Session["Taikhoan"] != null)
            {
                TaiKhoan tk = (TaiKhoan)Session["Taikhoan"];
                suggestedProducts = data.SanPhams.Where(m => m.status == 1 && m.id_user != tk.id_user)
                                                  .ToList()
                                                  .OrderBy(x => random.Next())
                                                  .Take(18)
                                                  .ToList();
            }

            return View(suggestedProducts);
        }


        private void SetDanhMucList()
        {
            ViewBag.DanhMucList = data.DanhMucs.ToList();
        }

        public ActionResult Login()
        {
            if (Session["Taikhoan"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                if (TempData["statusSignin"] != null)
                {
                    ViewBag.SigninSuccess = TempData["statusSignin"];
                    TempData.Remove("statusSignin");
                }
                return View();
            }
        }

        public ActionResult Signin()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(FormCollection collection, string returnUrl)
        {
            password EncryptedData = new password();
            var email = collection["email"];
            var matkhau = collection["password"];

            if (String.IsNullOrEmpty(email))
            {
                ViewData["Loi1"] = "Email không được bỏ trống";
            }
            else if (String.IsNullOrEmpty(matkhau))
            {
                ViewData["Loi2"] = "Mật khẩu không được bỏ trống";
            }
            else
            {
                TaiKhoan tk = data.TaiKhoans.SingleOrDefault(model => model.email == email && model.password == EncryptedData.Encode(matkhau));
                if (tk != null && tk.status == 0)
                {
                    FormsAuthentication.SetAuthCookie(tk.fullname, false);
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/") && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        Session["Taikhoan"] = tk;
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        Session["Taikhoan"] = tk;
                        TempData["LoginSuccess"] = "Đăng nhập thành công";
                        return RedirectToAction("Index", "Home");
                    }
                }
                else if (tk != null && tk.status == 1)
                {
                    ViewData["LoiDN"] = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ với bộ phận chăm sóc khách hàng";
                }
                else
                {
                    ViewData["LoiDN"] = "Tên đăng nhập hoặc mật khẩu không đúng";
                }
            }
            return View();
        }

        [HttpPost]
        public ActionResult Signin(FormCollection collection, TaiKhoan tk)
        {
            var time_register = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
            var format_time_register = DateTimeOffset.FromUnixTimeSeconds(time_register).LocalDateTime;

            var hoten = collection["fullname"];
            var ngaysinh = String.Format("{0:dd/MM/yyyy}", "2000/01/01");
            var sdt = collection["phone"];
            var diachi = collection["address"];
            var email = collection["email"];
            var matkhau = collection["password"];
            var matkhaunhaplai = collection["retypepass"];
            password EncryptedData = new password();

            if (String.IsNullOrEmpty(hoten))
            {
                ViewData["Loi1"] = "Tên không được để trống";
            }
            if (String.IsNullOrEmpty(ngaysinh))
            {
                ngaysinh = "01/01/2000";
            }
            if (String.IsNullOrEmpty(sdt))
            {
                ViewData["Loi2"] = "Số điện thoại không được để trống";
            }
            if (String.IsNullOrEmpty(email))
            {
                ViewData["Loi3"] = "Email không được để trống";
            }
            if (String.IsNullOrEmpty(matkhau))
            {
                ViewData["Loi4"] = "Mật khẩu không được để trống";
            }
            if (String.IsNullOrEmpty(matkhaunhaplai))
            {
                ViewData["Loi5"] = "Mật khẩu không được để trống";
            }
            else if (matkhaunhaplai != matkhau)
            {
                ViewData["Loi6"] = "Mật khẩu không khớp";
            }
            else
            {
                tk.fullname = hoten;
                tk.birthday = DateTime.Parse(ngaysinh);
                tk.phone = sdt;
                tk.email = email;
                tk.address = diachi;
                tk.avatar = "";
                tk.time_register = (int)time_register;
                tk.time_update = 0;
                tk.status = 0;
                tk.password = EncryptedData.Encode(matkhau);
                if (data.TaiKhoans.Any(x => x.email == tk.email))
                {
                    ViewBag.LoiDK = "Email đã được đăng ký";
                }
                else
                {
                    data.TaiKhoans.InsertOnSubmit(tk);
                    data.SubmitChanges();
                    TempData["statusSignin"] = "Đăng Ký Thành công";
                    return RedirectToAction("Login");
                }
            }
            return this.Signin();
        }

        public ActionResult Logout()
        {
            Session["Taikhoan"] = null;
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Information()
        {
            if (Session["Taikhoan"] == null)
            {
                return RedirectToAction("Login", new { returnUrl = Request.RawUrl.ToString() });
            }
            else
            {
                TaiKhoan tk = (TaiKhoan)Session["Taikhoan"];
                if (TempData["ChangeSuccess"] != null)
                {
                    ViewBag.ChangeSuccess = TempData["ChangeSuccess"];
                    TempData.Remove("ChangeSuccess");
                }
                return View(data.TaiKhoans.Single(n => n.email == tk.email));
            }
        }

        [HttpPost]
        public ActionResult Information(FormCollection collection)
        {
            TaiKhoan sstk = (TaiKhoan)Session["Taikhoan"];
            TaiKhoan tk = data.TaiKhoans.Single(n => n.email == sstk.email);

            var fullname = collection["fullname"];
            var address = collection["address"];
            var phone = collection["phone"];
            var birthday = collection["birthday"];

            if (ModelState.IsValid)
            {
                tk.fullname = fullname;
                tk.address = address;
                tk.phone = phone;
                tk.birthday = Convert.ToDateTime(birthday);

                data.SubmitChanges();
            }
            TempData["ChangeSuccess"] = "Cập nhật thông tin thành công";
            return RedirectToAction("Information");
        }

        public ActionResult UpdateInfor(int makh, string hoten, string ngaysinh)
        {
            TaiKhoan kh = data.TaiKhoans.SingleOrDefault(n => n.id_user == makh);

            ngaysinh = String.Format("{0:dd/MM/yyyy}", ngaysinh);
            bool isSuccess = false;

            if (ModelState.IsValid)
            {
                var time_update = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();

                kh.fullname = hoten;
                kh.birthday = Convert.ToDateTime(ngaysinh);
                kh.time_update = (int)time_update;

                data.SubmitChanges();
                isSuccess = true;
            }
            return Json(new { Success = isSuccess });
        }

        public ActionResult ChangePass()
        {
            if (Session["Taikhoan"] == null)
            {
                return RedirectToAction("Login", new { returnUrl = Request.RawUrl.ToString() });
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult ChangePass(FormCollection collection)
        {
            TaiKhoan tk = (TaiKhoan)Session["Taikhoan"];
            TaiKhoan kh = data.TaiKhoans.Single(n => n.email == tk.email);

            var oldpassword = collection["oldpassword"];
            var newpassword = collection["newpassword"];
            var retypepass = collection["retypepass"];

            password EncryptedData = new password();

            if (String.IsNullOrEmpty(oldpassword) || String.IsNullOrEmpty(newpassword) || String.IsNullOrEmpty(retypepass))
            {
                ViewBag.LoiNhap = "Mật khẩu không được để trống!";
            }
            else if (EncryptedData.Encode(oldpassword) != kh.password)
            {
                ViewData["Loi1"] = "Mật khẩu cũ không đúng!";
            }
            else if (retypepass != newpassword)
            {
                ViewData["Loi2"] = "Mật khẩu nhập lại không khớp!";
            }
            else if (EncryptedData.Encode(newpassword) == kh.password)
            {
                ViewData["Loi3"] = "Mật khẩu mới không được trùng với mật khẩu cũ!";
            }
            else
            {
                kh.password = EncryptedData.Encode(newpassword);
                data.SubmitChanges();

                TempData["ChangeSuccess"] = "Đổi mật khẩu thành công";
                return RedirectToAction("Information", "Home");
            }
            return this.Information();
        }

        public ActionResult ManagerProduct(int? id)
        {
            if (Session["Taikhoan"] == null)
            {
                return RedirectToAction("Login", new { returnUrl = Request.RawUrl.ToString() });
            }
            else
            {
                int proid = (id ?? 0);
                SanPham sp = data.SanPhams.SingleOrDefault(n => n.id == proid);
                if (sp == null)
                {
                    ViewBag.id_cat = new SelectList(data.DanhMucs.ToList().OrderBy(n => n.name_cat), "id_cat", "name_cat");
                    return View();
                }
                else
                {
                    ViewBag.id_cat = new SelectList(data.DanhMucs.ToList().OrderBy(n => n.name_cat), "id_cat", "name_cat", sp.id_cat);
                    return View(sp);
                }
            }
        }

        [HttpPost]
        public ActionResult ManagerProduct(int? id, HttpPostedFileBase ImageUpload, FormCollection collection)
        {
            int proid = (id ?? 0);
            TaiKhoan tk = (TaiKhoan)Session["Taikhoan"];
            SanPham sanpham = new SanPham();

            var tensp = collection["product_name"];
            var gia = collection["product_price"];
            var mota = collection["descrip"];
            var soluong = collection["quantity"];
            var madm = collection["id_cat"];
            var time_add = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();

            if (proid != 0)
            {
                SanPham sp = data.SanPhams.SingleOrDefault(n => n.id == proid);
                if (ImageUpload == null)
                {
                    sp.product_name = tensp;
                    sp.product_price = Convert.ToInt32(gia);
                    sp.descrip = mota;
                    sp.quantity = Convert.ToInt32(soluong);
                    sp.time_edit = (int)time_add;
                    sp.id_cat = Convert.ToInt32((madm == "") ? null : madm);
                    if (sp.id_cat == 0)
                    {
                        sp.id_cat = null;
                    }
                    data.SubmitChanges();
                }
                else
                {
                    if (ModelState.IsValid)
                    {
                        var fileName = Path.GetFileName(ImageUpload.FileName);
                        var path = Path.Combine(Server.MapPath("~/Assets/Images/Products"), fileName);
                        ImageUpload.SaveAs(path);

                        //Lưu vào CSDL
                        sp.product_name = tensp;
                        sp.product_price = Convert.ToInt32(gia);
                        sp.descrip = mota;
                        sp.quantity = Convert.ToInt32(soluong);
                        sp.product_image = fileName;
                        sp.time_edit = Convert.ToInt32(time_add);
                        sp.id_cat = Convert.ToInt32((madm == "") ? null : madm);

                        if (sp.id_cat == 0)
                        {
                            sp.id_cat = null;
                        }
                        data.SubmitChanges();
                    }
                }
                return RedirectToAction("ListPro");
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var fileName = Path.GetFileName(ImageUpload.FileName);
                    var path = Path.Combine(Server.MapPath("~/Assets/Images/Products"), fileName);
                    ImageUpload.SaveAs(path);

                    sanpham.product_name = tensp;
                    sanpham.product_price = Convert.ToInt32(gia);
                    sanpham.product_image = fileName;
                    sanpham.quantity = Convert.ToInt32(soluong);
                    sanpham.descrip = mota;
                    sanpham.time_add = (int)time_add;
                    sanpham.time_edit = 0;
                    sanpham.status = 0;
                    sanpham.id_cat = Convert.ToInt32((madm == "") ? null : madm);

                    if (sanpham.id_cat == 0)
                    {
                        sanpham.id_cat = null;
                    }
                    sanpham.id_user = tk.id_user;

                    data.SanPhams.InsertOnSubmit(sanpham);
                    data.SubmitChanges();
                    return RedirectToAction("ListPro");
                }
            }
            return RedirectToAction("ManagerProduct", "Home");
        }

        public ActionResult cc(int id)
        {
            SanPham sp = data.SanPhams.SingleOrDefault(n => n.id == id);
            return View(sp);
        }

        public ActionResult ListPro()
        {
            if (Session["Taikhoan"] == null)
            {
                return RedirectToAction("Login", new { returnUrl = Request.RawUrl.ToString() });
            }
            else
            {
                TaiKhoan tk = (TaiKhoan)Session["Taikhoan"];
                return View(data.SanPhams.Where(n => n.id_user == tk.id_user).Where(m => m.status == 1).OrderByDescending(t => t.time_add).ToList());
            }
        }

        public ActionResult InforShop(int shop_id, string orderby)
        {
            var listProduct = data.SanPhams.Where(x => x.id_user == shop_id);
            if (orderby == "ascending")
            {
                listProduct = listProduct.OrderBy(a => a.product_price);
            }
            else
            {
                listProduct = listProduct.OrderByDescending(a => a.product_price);
            }
            var model = new InforShopViewModel()
            {
                inforShop = data.TaiKhoans.Where(m => m.id_user == shop_id).ToList(),
                listProduct = listProduct.ToList(),
            };
            var rating_shop = data.DonDatHangs.Where(n => n.id_seller == shop_id).Average(m => (double?)m.rating);
            ViewBag.Rating = ((rating_shop != null) ? rating_shop : 0);
            ViewBag.CountPro = data.SanPhams.Where(n => n.id_user == shop_id).Count();
            ViewBag.id_shop = shop_id;
            return View(model);
        }

        public ActionResult ListCategory()
        {
            var listcat = (from cat in data.DanhMucs select cat).ToList();
            return View(listcat);
        }

        public ActionResult ListOrder()
        {
            if (Session["Taikhoan"] == null)
            {
                return RedirectToAction("Login", new { returnUrl = Request.RawUrl.ToString() });
            }
            else
            {
                TaiKhoan tk = (TaiKhoan)Session["Taikhoan"];
                var model = new OrderBuyViewModel()
                {
                    listorder = data.DonDatHangs.Where(m => m.id_seller == tk.id_user).OrderByDescending(t => t.time_ord).ToList(),
                    listdetail = data.ChiTietDonHangs.ToList(),
                };
                return View(model);
            }
        }

        public ActionResult OrderDetail(int id_ord)
        {
            if (Session["Taikhoan"] == null)
            {
                return RedirectToAction("Login", new { returnUrl = Request.RawUrl.ToString() });
            }
            else
            {
                TaiKhoan tk = (TaiKhoan)Session["Taikhoan"];
                DonDatHang ddh = data.DonDatHangs.Single(i => i.id == id_ord);
                ViewBag.total_payment = ddh.total_payment;
                ViewBag.id_order = id_ord;

                var model = new OrderBuyViewModel()
                {
                    listorder = data.DonDatHangs.Where(n => n.id == id_ord).ToList(),
                    listdetail = data.ChiTietDonHangs.Where(n => n.id_ord == id_ord).ToList(),
                };
                return View(model);
            }
        }

        public ActionResult ListBuy()
        {
            if (Session["Taikhoan"] == null)
            {
                return RedirectToAction("Login", new { returnUrl = Request.RawUrl.ToString() });
            }
            else
            {
                TaiKhoan tk = (TaiKhoan)Session["Taikhoan"];
                var model = new OrderBuyViewModel()
                {
                    listorder = data.DonDatHangs.Where(m => m.id_buyer == tk.id_user).OrderByDescending(t => t.time_ord).ToList(),
                    listdetail = data.ChiTietDonHangs.ToList(),
                };
                var listrating = new Dictionary<int, string>() { { 5, "rate-5" }, { 4, "rate-4" }, { 3, "rate-3" }, { 2, "rate-2" }, { 1, "rate-1" } };
                foreach (var item in listrating)
                {
                    var key = item.Key;
                    var value = item.Value;
                }
                return View(model);
            }
        }

        public ActionResult BuyDetail(int id_ord)
        {
            if (Session["Taikhoan"] == null)
            {
                return RedirectToAction("Login", new { returnUrl = Request.RawUrl.ToString() });
            }
            else
            {
                TaiKhoan tk = (TaiKhoan)Session["Taikhoan"];
                DonDatHang ddh = data.DonDatHangs.Single(i => i.id == id_ord);
                ViewBag.total_payment = ddh.total_payment;
                ViewBag.id_order = id_ord;
                ViewBag.status = ddh.status;

                var model = new OrderBuyViewModel()
                {
                    listorder = data.DonDatHangs.Where(n => n.id == id_ord).ToList(),
                    listdetail = data.ChiTietDonHangs.Where(n => n.id_ord == id_ord).ToList(),
                };
                return View(model);
            }
        }

        public ActionResult ProductCategory(int id_cat, string orderby, int? page)
        {
            var danhMucList = data.DanhMucs.ToList();
            ViewBag.DanhMucList = danhMucList;

            int pageSize = 20;
            int pageNum = (page ?? 1);

            var product = from p in data.SanPhams where p.id_cat == id_cat select p;
            if (Session["Taikhoan"] != null)
            {
                TaiKhoan tk = (TaiKhoan)Session["Taikhoan"];
                product = product.Where(m => m.id_user != tk.id_user);
            }
            if (orderby == "ascending")
            {
                product = product.OrderBy(a => a.product_price);
            }
            else
            {
                product = product.OrderByDescending(a => a.product_price);
            }
            ViewBag.Page = pageNum;
            ViewBag.id_cat = id_cat;
            ViewBag.orderby = orderby;
            return View(product.ToPagedList(pageNum, pageSize));
        }

        public ActionResult Search(string searchString, string orderby, int? page)
        {
            int pageSize = 20;
            int pageNum = (page ?? 1);

            var product = from p in data.SanPhams select p;
            if (Session["Taikhoan"] != null)
            {
                TaiKhoan tk = (TaiKhoan)Session["Taikhoan"];
                product = product.Where(m => m.id_user != tk.id_user);
            }
            if (!String.IsNullOrEmpty(searchString))
            {
                product = product.Where(s => s.product_name.Contains(searchString));
            }
            if (orderby == "ascending")
            {
                product = product.OrderBy(a => a.product_price);
            }
            else
            {
                product = product.OrderByDescending(a => a.product_price);
            }
            ViewBag.Page = pageNum;
            ViewBag.searchString = searchString;
            ViewBag.orderby = orderby;
            return View(product.ToPagedList(pageNum, pageSize));
        }

        public ActionResult SearchShop(string searchString)
        {
            var listShop = data.TaiKhoans.Where(y => y.fullname.Contains(searchString)).ToList();
            ViewBag.searchString = searchString;
            return View(listShop);
        }

        public ActionResult ProductDetail(int product_id)
        {
            var danhMucList = data.DanhMucs.ToList();
            ViewBag.DanhMucList = danhMucList;
            var detailPro = data.SanPhams.Single(m => m.id == product_id);
            ViewBag.CountPro = data.SanPhams.Where(n => n.id_user == detailPro.TaiKhoan.id_user).Count();

            if (Session["Taikhoan"] != null)
            {
                TaiKhoan tk = (TaiKhoan)Session["Taikhoan"];
                ViewBag.iduser = tk.id_user;
            }

            var rating_shop = data.DonDatHangs.Where(n => n.id_seller == detailPro.TaiKhoan.id_user).Average(m => (double?)m.rating);
            ViewBag.Rating = ((rating_shop != null) ? rating_shop : 0);

            return View(detailPro);
        }

     

        public ActionResult ratingOrder(int iMaDH, int rating)
        {
            DonDatHang dh = data.DonDatHangs.SingleOrDefault(n => n.id == iMaDH);
            bool isSuccess = false;

            if (ModelState.IsValid)
            {
                var time_rating = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();

                dh.rating = rating;
                dh.time_rating = (int)time_rating;
                data.SubmitChanges();
                isSuccess = true;
            }
            return Json(new { Success = isSuccess });
        }

        [HttpPost]
        public JsonResult CheckReceived(int iMaDH)
        {
            DonDatHang dh = data.DonDatHangs.SingleOrDefault(n => n.id == iMaDH);
            bool isSuccess = false;

            if (dh != null)
            {
                dh.status = 3; // Trạng thái "Đã giao thành công"
                var chiTietDonHangs = data.ChiTietDonHangs.Where(c => c.id_ord == iMaDH).ToList();
                foreach (var item in chiTietDonHangs)
                {
                    // Lấy sản phẩm theo id_product
                    var sanPham = data.SanPhams.SingleOrDefault(p => p.id == item.id_product);
                    if (sanPham != null)
                    {
                        sanPham.quantity -= item.quantity; // Trừ số lượng sản phẩm đã mua
                        if (sanPham.quantity < 0) sanPham.quantity = 0; // Đảm bảo số lượng không âm
                    }
                }
                data.SubmitChanges();
                isSuccess = true;
            }
            return Json(new { Success = isSuccess });
        }

        [HttpPost]
        public JsonResult ConfirmOrder(int iMaDH)
        {
            DonDatHang dh = data.DonDatHangs.SingleOrDefault(n => n.id == iMaDH);
            bool isSuccess = false;

            if (dh != null)
            {
                dh.status = 1; // Trạng thái "Đang giao hàng"
                data.SubmitChanges();
                isSuccess = true;
            }
            return Json(new { Success = isSuccess });
        }

        [HttpPost]
        public JsonResult CancelOrder(int iMaDH)
        {
            DonDatHang dh = data.DonDatHangs.SingleOrDefault(n => n.id == iMaDH);
            bool isSuccess = false;

            if (dh != null)
            {
                dh.status = 2; // Trạng thái "Bị hủy"
                data.SubmitChanges();
                isSuccess = true;
            }
            return Json(new { Success = isSuccess });
        }
        public ActionResult About()
        {
            return View();
        }
    }
}
