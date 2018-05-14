using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watson.SecretConnect
{
    public class SecretAdoDotNet
    {
        protected string connectString;
        public SecretAdoDotNet(string nameOrConnectionString)
        {
            this.connectString = ConnectDecrypt.GetConnectionString(nameOrConnectionString);
        }
    }
}
