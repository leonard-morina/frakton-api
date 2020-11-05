using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frakton.Models
{
    public class JwtToken
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
