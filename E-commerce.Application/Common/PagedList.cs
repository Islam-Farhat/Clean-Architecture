using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Common
{
    public class PagedList<T>
    {
        public List<T> List { get; set; } = new List<T>();
        public int TotalCount { get; set; }
    }
}
