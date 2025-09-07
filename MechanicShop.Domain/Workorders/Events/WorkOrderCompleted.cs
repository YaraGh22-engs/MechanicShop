using MechanicShop.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MechanicShop.Domain.Workorders.Events
{
    public sealed class WorkOrderCompleted : DomainEvent
    {
        public Guid WorkOrderId { get; set; }
    }
}
