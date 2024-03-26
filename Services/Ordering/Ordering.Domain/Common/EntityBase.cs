using Ordering.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Ordering.Domain.Common
{
    public abstract class EntityBase
    {
        /*
             order.CreatedBy = "System";
                order.CreatedDate = DateTime.UtcNow;
                order.LastModifiedBy = "system";
                order.LastModifiedDate = DateTime.UtcNow;
         */
        protected EntityBase()
        {
            CreatedBy = "System";
            CreatedDate = DateTime.UtcNow;
            LastModifiedBy = "system";
            LastModifiedDate = DateTime.UtcNow;
        }
        public int Id { get; protected set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
    }
}
