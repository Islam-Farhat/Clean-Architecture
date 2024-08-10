using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Domian
{
    public class Category
    {
        public int Id { get;private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public readonly HashSet<Product> Products = new();
    }
}
