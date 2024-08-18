using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Domian.DomainEventHelper
{
    public class Entity
    {
        private List<DomainEvent> events = new List<DomainEvent>();
        public void AddEvent(DomainEvent @event)
        {
            if (@event != null)
            {
                events.Add(@event);
            }
        }

        public void ClearEvents()
        {
            events.Clear();
        }


        public IReadOnlyList<DomainEvent> Events => events.ToList();

    }
}
