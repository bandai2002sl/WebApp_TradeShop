using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TradeShop.Models
{
    public class Giohang
    {
        public int iMasp { set; get; }
        public int id_seller { set; get; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        public String sTensp { set; get; }

        public String sAnhbia { set; get; }

        [Required(ErrorMessage = "Giá bán không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải là số dương")]
        public int dDongia { get; set; }

        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải là số dương")]
        public int iSoluong { set; get; }

        [Required(ErrorMessage = "Mô tả không được để trống")]
        public string descrip { get; set; }

        public int dThanhtien
        {
            get
            {
                return (iSoluong * dDongia);
            }
        }

        public Giohang() { }

        public Giohang(int MaSP)
        {
            TradeShopDataContext data = new TradeShopDataContext();
            iMasp = MaSP;
            SanPham sanPham = data.SanPhams.Single(model => model.id == iMasp);
            id_seller = (int)sanPham.id_user;
            sTensp = sanPham.product_name;
            sAnhbia = sanPham.product_image;
            dDongia = int.Parse(sanPham.product_price.ToString());
            iSoluong = 1;
            descrip = sanPham.descrip;
        }
    }
}
