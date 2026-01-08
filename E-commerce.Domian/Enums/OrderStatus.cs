using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Domian.Enums
{
    public enum OrderStatus
    {
        Active = 1,
        Completed,
        Cancelled,
        New,
        Deleted,
    }
}
