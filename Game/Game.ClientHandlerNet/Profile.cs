using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.ClientHandlerNet
{
    public class Profile
    {
        [JsonProperty(PropertyName = "serverUri")]
        public string ServerUri;
        [JsonProperty(PropertyName = "teamName")]
        public string TeamName;
        [JsonProperty(PropertyName = "teamPassword")]
        public string TeamPassword;
        [JsonProperty(PropertyName = "clientName")]
        public string ClientName;
        [JsonProperty(PropertyName = "clientType")]
        public string ClientType;
        [JsonProperty(PropertyName = "execName")]
        public string ExecName;
        [JsonProperty(PropertyName = "sessionDir")]
        public string SessionDir;
        [JsonProperty(PropertyName = "jsonUri")]
        public string JsonUri;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ServerUri))
                throw new ApplicationException("serverUri not specified");
            if (string.IsNullOrWhiteSpace(TeamName))
                throw new ApplicationException("teamName not specified");
            if (string.IsNullOrWhiteSpace(TeamPassword))
                throw new ApplicationException("teamPassword not specified");
            if (string.IsNullOrWhiteSpace(ClientName))
                throw new ApplicationException("clientName not specified");
            if (string.IsNullOrWhiteSpace(ClientType))
                throw new ApplicationException("clientType not specified");
            if (string.IsNullOrWhiteSpace(SessionDir))
                SessionDir = "sessions";

            if (ClientType == "exec")
            {
                if (string.IsNullOrWhiteSpace(ExecName))
                    throw new ApplicationException("execName not specified");
                //handler = execHandler;
            }
            else if (ClientType == "json")
            {
                if (string.IsNullOrWhiteSpace(JsonUri))
                    throw new ApplicationException("jsonUri not specified");
                //handler = jsonHandler;
            }
            else if (ClientType == "stdio")
            {
                if (string.IsNullOrWhiteSpace(ExecName))
                    throw new ApplicationException("execName not specified");
            }
            else
            {
                throw new ApplicationException($"clientType not supported - {ClientType}");
            }
        }
    }
}