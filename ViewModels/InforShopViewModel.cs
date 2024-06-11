using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TradeShop.Models;

namespace TradeShop.ViewModels
{
    public class InforShopViewModel
    {
        public IEnumerable<TaiKhoan> inforShop { get; set; }
        public IEnumerable<SanPham> listProduct { get; set; }

    }
}