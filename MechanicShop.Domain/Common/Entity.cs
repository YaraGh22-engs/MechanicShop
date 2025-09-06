using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MechanicShop.Domain.Common
{
    public abstract class Entity
    {
        public Guid Id { get; }

        protected Entity() { }

        protected Entity(Guid id)
        {
            id = Id == Guid.Empty ? Guid.Empty : id;
        }
    }
}
