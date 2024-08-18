using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Domian.DomainEventHelper
{
    public interface DomainEvent : INotification
    {
    }
}
