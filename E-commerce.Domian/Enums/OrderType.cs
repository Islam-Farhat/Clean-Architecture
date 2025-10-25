using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Domian.Enums
{
    public enum OrderType
    {
        [Display(Name = "دائم")] //مبيت
        Permanent = 1,

        [Display(Name = "شهرى")] //مثبت
        Monthly,

        [Display(Name = "اسبوعى")] //مثبت
        Weekly,

        [Display(Name = "ساعات")] //ساعات
        Hourly
    }
}
