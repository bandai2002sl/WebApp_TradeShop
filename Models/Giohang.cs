using System.Linq;
using System;

namespace TradeShop.Models
{
    public class Giohang
    {
        TradeShopDataContext data = new TradeShopDataContext();
        public int iMasp { set; get; }
        public int id_seller { set; get; }
        public String sTensp { set; get; }
        public String sAnhbia { set; get; }
        public int dDongia { get; set; }
        public int iSoluong { set; get; }
        public int dThanhtien
        {
            get
            {
                return iSoluong * dDongia;
            }
        }
        public Giohang(int MaSP)
        {
            iMasp = MaSP;
            SanPham sanPham = data.SanPhams.Single(model => model.id == iMasp);
            id_seller = (int)sanPham.id_user;
            sTensp = sanPham.product_name;
            sAnhbia = sanPham.product_image;
            dDongia = int.Parse(sanPham.product_price.ToString());
            iSoluong = 1;
        }

    }
}
