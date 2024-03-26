using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Messages.MessageContracts
{
    public class DiscountRequest
    {
        public string ProductName { get; set; }
    }
}
