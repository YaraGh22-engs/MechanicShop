using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MechanicShop.Domain.Common
{
    // INotification من MediatR - للتعامل مع الأحداث
    public abstract class DomainEvent : INotification;
}
