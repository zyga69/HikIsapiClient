using HikIsapi.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hikisapiclient
{
    public class IsapiConfig : IIsapiConfig
    {
        public string Address { get; set; }

        public string User { get; set; }

        public string Password { get; set; }
    }
}