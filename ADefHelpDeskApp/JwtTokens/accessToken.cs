using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdefHelpDeskBase.JwtTokens
{
    public class accessToken
    {
        public bool authorized { get; set; }
        public string access_token { get; set; }
        public int expires_in { get; set; }
    }
}
