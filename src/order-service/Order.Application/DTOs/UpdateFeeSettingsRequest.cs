using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.DTOs
{
    public class UpdateFeeSettingsRequest
    {
        public int Type { get; set; }
        public decimal FeePercent { get; set; }
        public decimal CommissionPercent { get; set; }
    }
}
