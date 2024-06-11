using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TradeShop.Models;

namespace TradeShop.ViewModels
{
    public class OrderBuyViewModel
    {
        public IEnumerable<DonDatHang> listorder { get; set; }
        public IEnumerable<ChiTietDonHang> listdetail { get; set; }

    }
}