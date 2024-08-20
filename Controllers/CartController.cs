using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TradeShop.Models;
namespace TradeShop.Controllers
{
    public class CartController : Controller
    {
        TradeShopDataContext data = new TradeShopDataContext();
        // GET: Cart
        public ActionResult CartDefault()
        {
            return View();
        }
        public List<Giohang> Laygiohang()
        {
            List<Giohang> lstGiohang = Session["Giohang"] as List<Giohang>;
            if (lstGiohang == null)
            {
                lstGiohang = new List<Giohang>();
                Session["Giohang"] = lstGiohang;
                Session.Timeout = 60;
            }
            return lstGiohang;
        }
        public ActionResult ThemGiohangChiTiet(int iMaSP, string strURL, int quantity)
        {
            if (Session["Taikhoan"] == null)
            {
                return RedirectToAction("Login", "Home", new { returnUrl = Request.RawUrl.ToString() });
            }
            else
            {
                List<Giohang> lstGiohang = Laygiohang(); // Lấy ra Session["Giohang"]
                Giohang sanpham = lstGiohang.Find(n => n.iMasp == iMaSP);// Kiểm tra sp này có tồn tại trong Session["Giohang"] chưa?
                if (sanpham == null)
                {
                    sanpham = new Giohang(iMaSP);
                    lstGiohang.Add(sanpham);
                    sanpham.iSoluong = quantity;
                    return Redirect(strURL);
                }
                else
                {
                    sanpham.iSoluong += quantity;
                    return Redirect(strURL);
                }
            }
        }
        public ActionResult ThemGiohang(int iMaSP, string strURL)
        {
            bool isSuccess = false;
            if (Session["Taikhoan"] == null)
            {
                return RedirectToAction("Login", "Home", new { returnUrl = Request.RawUrl.ToString() });
            }
            else
            {
                List<Giohang> lstGiohang = Laygiohang(); // Lấy ra Session["Giohang"]
                Giohang sanpham = lstGiohang.Find(n => n.iMasp == iMaSP);// Kiểm tra sách này có tồn tại trong Session["Giohang"] chưa?
                if (sanpham == null)
                {
                    sanpham = new Giohang(iMaSP);
                    lstGiohang.Add(sanpham);
                    isSuccess = true;
                }
                else
                {
                    sanpham.iSoluong++;
                    isSuccess = true;
                }
            }
            return Json(new { Success = isSuccess });
        }
        private double TongSoLuong()// Method tính tổng số lượng
        {
            int iTongSoLuong = 0;
            List<Giohang> lstGiohang = Session["GioHang"] as List<Giohang>;
            if (lstGiohang != null)
            {
                iTongSoLuong = lstGiohang.Sum(n => n.iSoluong);
            }
            return iTongSoLuong;
        }
        private double TongTien()// Method tính tổng tiền
        {
            int iTongTien = 0;
            List<Giohang> lstGiohang = Session["GioHang"] as List<Giohang>;
            if (lstGiohang != null)
            {
                iTongTien = lstGiohang.Sum(n => n.dThanhtien);
            }
            return iTongTien;
        }
        [HttpGet]
        // GET: Giohang
        public ActionResult GioHang()
        {
            List<Giohang> lstGiohang = Laygiohang();

            if (lstGiohang.Count == 0)
            {
                return RedirectToAction("CartDefault", "Cart");
            }
            ViewBag.Tongsoluong = TongSoLuong();
            ViewBag.Tongtien = TongTien();
            return View(lstGiohang);
        }
        public PartialViewResult GiohangPartial()
        {
            ViewBag.Tongsoluong = TongSoLuong();
            ViewBag.Tongtien = TongTien();
            return PartialView();
        }
        public ActionResult XoaGiohang(int iMaSP)
        {
            List<Giohang> lstGiohang = Laygiohang();// Lấy giỏ hàng từ session
            Giohang sanpham = lstGiohang.SingleOrDefault(n => n.iMasp == iMaSP); //  kiểm tra sách đã có trong session chưa?
            bool isSuccess = false;

            if (sanpham != null)
            {
                lstGiohang.RemoveAll(n => n.iMasp == iMaSP);
                isSuccess = true;
            }
            if (lstGiohang.Count == 0)
            {
                return RedirectToAction("Index", "Home");
            }
            return Json(new { Success = isSuccess});
        }
        public ActionResult CapnhapGiohang(int iMaSP, int soluong)
        {
            List<Giohang> lstGiohang = Laygiohang();// Lấy giỏ hàng từ session
            Giohang sanpham = lstGiohang.SingleOrDefault(n => n.iMasp == iMaSP);

            SanPham check_quantity = data.SanPhams.Single(model => model.id == iMaSP);
            bool isSuccess = false;

            if (sanpham != null)
            {
                if(soluong <= check_quantity.quantity)
                {
                    sanpham.iSoluong = soluong;
                    isSuccess = true;
                }
            }
            return Json(new { Success = isSuccess, max_quantity = check_quantity.quantity });
        }
        public ActionResult XoaTatCaGioHang()
        {
            List<Giohang> lstGiohang = Laygiohang();
            lstGiohang.Clear();
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public ActionResult Dathang()
        {
            List<Giohang> lstGiohang = Laygiohang();
            ViewBag.Tongsoluong = TongSoLuong();
            ViewBag.Tongtien = TongTien();
            return View(lstGiohang);
        }
        [HttpPost]
        public ActionResult Dathang(FormCollection collection)
        {
            TaiKhoan kh = (TaiKhoan)Session["Taikhoan"];
            List<Giohang> gh = Laygiohang();

            var time_order = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
            var time_deliver = new DateTimeOffset(DateTime.Now.AddDays(5)).ToUnixTimeSeconds();

            var groupby_order = gh.GroupBy(m => m.id_seller).ToList();            

            foreach (var seller in groupby_order)
            {
                DonDatHang ddh = new DonDatHang();
                var orderdetail = gh.Where(m => m.id_seller == seller.Key).ToList();
                int total_payment = orderdetail.Sum(s => s.dThanhtien);

                ddh.id_buyer = kh.id_user;
                ddh.id_seller = seller.Key;
                ddh.fullname = collection["fullname"];
                ddh.phone = collection["phone"];
                ddh.address = collection["address"];
                ddh.time_ord = (int)time_order;
                ddh.time_deliver = (int)time_deliver;
                ddh.total_payment = total_payment;
                ddh.rating = 0;
                ddh.time_rating = 0;
                ddh.status = 0;
                data.DonDatHangs.InsertOnSubmit(ddh);
                data.SubmitChanges();

                foreach (var i in orderdetail)
                {
                    ChiTietDonHang ctdh = new ChiTietDonHang();
                    ctdh.id_ord = ddh.id;
                    ctdh.id_product = i.iMasp;
                    ctdh.quantity = i.iSoluong;
                    ctdh.price = (int)i.dDongia;
                    ctdh.total = i.iSoluong * (int)i.dDongia;
                    data.ChiTietDonHangs.InsertOnSubmit(ctdh);
                }
            }
            data.SubmitChanges();

            Session["Giohang"] = null;
            return RedirectToAction("ListBuy", "Home");
        }
        public ActionResult OrderConfirmation()
        {
            return View();
        }
    }
}