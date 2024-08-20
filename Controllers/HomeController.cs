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

            ViewBag.ActiveTab = "suggest_today";
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
                Giohang giohang = new Giohang();

                if (id != null)
                {
                    SanPham sp = data.SanPhams.SingleOrDefault(n => n.id == id);
                    if (sp != null)
                    {
                        giohang = new Giohang(sp.id);
                    }
                }

                ViewBag.id_cat = new SelectList(data.DanhMucs.ToList().OrderBy(n => n.name_cat), "id_cat", "name_cat", giohang != null ? giohang.id_seller : (int?)null);
                return View(giohang);
            }
        }

        [HttpPost]
        public ActionResult ManagerProduct(Giohang model, HttpPostedFileBase imageUpload, FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                if (imageUpload == null && model.iMasp == 0)
                {
                    ViewBag.ImageUploadError = "Vui lòng chọn ảnh cho sản phẩm";
                    ViewBag.id_cat = new SelectList(data.DanhMucs.ToList().OrderBy(n => n.name_cat), "id_cat", "name_cat", model.id_seller);
                    return View(model);
                }

                TaiKhoan tk = (TaiKhoan)Session["Taikhoan"];
                SanPham sanpham;

                if (model.iMasp != 0)
                {
                    sanpham = data.SanPhams.SingleOrDefault(n => n.id == model.iMasp);
                }
                else
                {
                    sanpham = new SanPham();
                    data.SanPhams.InsertOnSubmit(sanpham);
                }

                sanpham.product_name = model.sTensp;
                sanpham.product_price = model.dDongia;
                sanpham.descrip = model.descrip;
                sanpham.quantity = model.iSoluong;
                sanpham.id_cat = collection["id_cat"] != "" ? int.Parse(collection["id_cat"]) : (int?)null;
                sanpham.id_user = tk.id_user;
                sanpham.status = 0;

                if (imageUpload != null)
                {
                    var fileName = Path.GetFileName(imageUpload.FileName);
                    var path = Path.Combine(Server.MapPath("~/Assets/Images/Products"), fileName);
                    imageUpload.SaveAs(path);
                    sanpham.product_image = fileName;
                }

                data.SubmitChanges();
                return RedirectToAction("ListPro");
            }

            ViewBag.id_cat = new SelectList(data.DanhMucs.ToList().OrderBy(n => n.name_cat), "id_cat", "name_cat", model.id_seller);
            return View(model);
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

        public ActionResult InforShop(int shop_id, string orderby = null, string priceFilter = null)
        {
            var listProduct = data.SanPhams.Where(x => x.id_user == shop_id);

            // Logic sắp xếp theo giá
            if (priceFilter == "zero")
            {
                listProduct = listProduct.Where(a => a.product_price == 0);
            }
            else if (orderby == "ascending")
            {
                listProduct = listProduct.OrderBy(a => a.product_price);
            }
            else if (orderby == "descending")
            {
                listProduct = listProduct.OrderByDescending(a => a.product_price);
            }

            var model = new InforShopViewModel()
            {
                inforShop = data.TaiKhoans.Where(m => m.id_user == shop_id).ToList(),
                listProduct = listProduct.ToList(),
            };

            var rating_shop = data.DonDatHangs.Where(n => n.id_seller == shop_id).Average(m => (double?)m.rating);
            ViewBag.Rating = rating_shop ?? 0;
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

        public ActionResult ProductCategory(int id_cat, string orderby = null, string priceFilter = null, int? page = 1)
        {
            var danhMucList = data.DanhMucs.ToList();
            ViewBag.DanhMucList = danhMucList;

            int pageSize = 20;
            int pageNum = (page ?? 1);

            var product = data.SanPhams.Where(p => p.id_cat == id_cat);

            if (Session["Taikhoan"] != null)
            {
                TaiKhoan tk = (TaiKhoan)Session["Taikhoan"];
                product = product.Where(m => m.id_user != tk.id_user);
            }

            if (!string.IsNullOrEmpty(priceFilter) && priceFilter == "zero")
            {
                product = product.Where(a => a.product_price == 0);
            }
            
            else if (!string.IsNullOrEmpty(orderby) && orderby == "ascending")
            {
                product = product.OrderBy(a => a.product_price);
            }
            else if (!string.IsNullOrEmpty(orderby) && orderby == "descending")
            {
                product = product.OrderByDescending(a => a.product_price);
            }

            ViewBag.Page = pageNum;
            ViewBag.id_cat = id_cat;
            ViewBag.orderby = orderby;
            ViewBag.priceFilter = priceFilter;

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
        public ActionResult FreeProducts()
        {
            var freeProducts = data.SanPhams.Where(p => p.product_price == 0).ToList();
            ViewBag.ActiveTab = "free_products";
            return View("Index", freeProducts);
        }
        public ActionResult CustomerManager(string searchString, int? page)
        {
            int pageSize = 20;
            int pageNumber = (page ?? 1);

            var customers = from c in data.TaiKhoans select c;

            if (!String.IsNullOrEmpty(searchString))
            {
                customers = customers.Where(s => s.fullname.Contains(searchString));
            }
            
            ViewBag.searchString = searchString;
            return View(customers.OrderBy(c => c.id_user).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult ProductManager(string tukhoa, int? page)
        {
            int pageSize = 20;
            int pageNumber = (page ?? 1);

            var products = from p in data.SanPhams select p;

            if (!String.IsNullOrEmpty(tukhoa))
            {
                products = products.Where(s => s.product_name.Contains(tukhoa));
            }

            ViewBag.tukhoa = tukhoa;
            return View(products.OrderBy(p => p.id).ToPagedList(pageNumber, pageSize));
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
