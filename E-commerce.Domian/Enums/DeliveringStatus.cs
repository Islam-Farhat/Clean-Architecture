using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Domian.Enums
{
    public enum DeliveringStatus
    {
        AssignedToDriver = 1,
        New = 2,
        DeliveredToHome = 3,
        EndOfShift = 4,
    }
}
