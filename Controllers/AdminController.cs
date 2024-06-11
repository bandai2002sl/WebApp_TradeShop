using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TradeShop.Models;
using System.IO;
using System.Web.Security;

using PagedList;
using PagedList.Mvc;

namespace TradeShop.Controllers
{
    public class AdminController : Controller
    {
        TradeShopDataContext data = new TradeShopDataContext();

        // GET: Admin
        public ActionResult Login()
        {
            Admin ad = (Admin)Session["Taikhoanadmin"];
            if (Session["Taikhoanadmin"] != null)
            {
                return RedirectToAction("Index", "Admin", ad);
            }
            else
            {
                return View();
            }
        }
        [HttpPost]
        public ActionResult Login(FormCollection collection, string returnUrl)
        {
            //Gán các giá trị người dùng nhập liệu cho các biến
            var tendn = collection["username"];
            var matkhau = collection["password"];
            if (String.IsNullOrEmpty(tendn))
            {
                ViewData["Loi1"] = "Tên đăng nhập không được để trống";
            }
            else if (String.IsNullOrEmpty(matkhau))
            {
                ViewData["Loi2"] = "Mật khẩu chưa được nhập";
            }
            else
            {
                //Gán giá trị cho đối tượng được tạo mới (ad)
                Admin ad = data.Admins.SingleOrDefault(n => n.email == tendn && n.password == matkhau);
                if (ad != null)
                {
                    FormsAuthentication.SetAuthCookie(ad.fullname, false);
                    Session["Taikhoanadmin"] = ad;
                    return RedirectToAction("Index", "Admin");
                }
                else
                    ViewBag.ThongBao = "Tên đăng nhập hoặc mật khẩu không đúng";
            }
            return View();
        }
        public ActionResult SignOut()
        {
            Session["Taikhoanadmin"] = null;
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Admin");
        }
        [Authorize]
        public ActionResult Index()
        {
            if (Session["Taikhoanadmin"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Admin");
            }
        }
        public ActionResult Map()
        {
            if (Session["Taikhoanadmin"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Admin");
            }
        }
        public ActionResult ProductManager(int? page)
        {
            int pageNumber = (page ?? 1);
            int pageSize = 7;

            if (Session["Taikhoanadmin"] != null)
            {
                return View(data.SanPhams.ToList().OrderByDescending(n => n.time_add).OrderBy(m => m.status).ToPagedList(pageNumber, pageSize));
            }
            else
            {
                return RedirectToAction("Login", "Admin");
            }
        }
        public ActionResult Delete(int id)
        {
            //Lấy đối tượng sản phẩm cần xóa theo mã
            SanPham sp = data.SanPhams.SingleOrDefault(n => n.id == id);
            bool isSuccess = false;
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            else
            {
                data.SanPhams.DeleteOnSubmit(sp);
                data.SubmitChanges();
                isSuccess = true;
            }
            return Json( new { Success = isSuccess });
        }
        public ActionResult ConfirmPro(int id)
        {
            //Lấy đối tượng sản phẩm cần xóa theo mã
            SanPham sp = data.SanPhams.SingleOrDefault(n => n.id == id);
            bool isSuccess = false;
            if (sp != null)
            {
                sp.status = 1;
                data.SubmitChanges();
                isSuccess = true;
            }
            return Json(new { Success = isSuccess });
        }
        public ActionResult CustomerManager(int? page)
        {
            int pageNumber = (page ?? 1);
            int pageSize = 7;

            if (Session["Taikhoanadmin"] != null)
            {
                return View(data.TaiKhoans.ToList().OrderByDescending(n => n.time_register).OrderBy(m => m.status).ToPagedList(pageNumber, pageSize));
            }
            else
            {
                return RedirectToAction("Login", "Admin");
            }
        }
        public ActionResult ChangeStatusAccount(int id_user, int status)
        {
            //Lấy đối tượng sản phẩm cần xóa theo mã
            TaiKhoan sp = data.TaiKhoans.SingleOrDefault(n => n.id_user == id_user);
            bool isSuccess = false;
            if (sp != null)
            {
                sp.status = (byte)status;
                data.SubmitChanges();
                isSuccess = true;
            }
            return Json(new { Success = isSuccess });
        }
    }
}