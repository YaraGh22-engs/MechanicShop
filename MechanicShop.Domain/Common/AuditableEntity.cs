﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MechanicShop.Domain.Common
{
    // يحصل على:
    // - Id (من Entity)
    // - Domain Events (من Entity)
    // - CreatedAtUtc, CreatedBy, LastModifiedUtc, LastModifiedBy (من AuditableEntity)
    public abstract class AuditableEntity : Entity
    {
        protected AuditableEntity() { }
        protected AuditableEntity(Guid id) : base(id) { }
        public DateTimeOffset CreatedAtUtc { get; set; }

        public string? CreatedBy { get; set; }

        public DateTimeOffset LastModifiedUtc { get; set; }

        public string? LastModifiedBy { get; set; }
    }
}
