using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MagicVilla_coupon.Models;

namespace MagicVilla_coupon.Data
{
    public static class CouponStore
    {
        public static List<Coupon> couponList = new List<Coupon> {
            new Coupon {Id= 1, Name = "10OFF", Percent = 10, isActive = true},
             new Coupon {Id= 2, Name = "20OFF", Percent = 20, isActive = false}
        };
    }
}