using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payment.Application.DTOs
{
    public class VnPayIpnResponse
    {
        public string RspCode { get; set; } = default!;
        public string Message { get; set; } = default!;

        public VnPayIpnResponse() { }

        public VnPayIpnResponse(string code, string message)
        {
            RspCode = code;
            Message = message;
        }
    }
}
