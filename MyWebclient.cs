using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace unturned.ROCKS.Votifier
{
    public class MyWebClient : WebClient
    {
        //time in milliseconds
        private int timeout;
        public int Timeout
        {
            get
            {
                return timeout;
            }
            set
            {
                timeout = value;
            }
        }

        public MyWebClient(int timeout = 5000)
        {
            this.timeout = timeout;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var result = base.GetWebRequest(address);
            result.Timeout = this.timeout;
            return result;
        }
    }

}
