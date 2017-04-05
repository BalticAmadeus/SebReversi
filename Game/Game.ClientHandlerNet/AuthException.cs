using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.ClientHandlerNet
{
    public class AuthException : Exception
    {
        public AuthException(string message)
            : base(message)
        {
            // nothing more
        }
    }
}
