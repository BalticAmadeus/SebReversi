using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.AdminClient.AdminService;

namespace Game.AdminClient.Infrastructure
{
    public class ServerMessageException : ApplicationException
    {
        public string Status { get; private set; }

        public ServerMessageException(string status, string message)
            : base(message)
        {
            Status = status;
        }

        public static void ThrowOnError(BaseResp resp)
        {
            if (resp.Status == "OK")
                return;
            throw new ServerMessageException(resp.Status, resp.Message);
        }
    }
}
