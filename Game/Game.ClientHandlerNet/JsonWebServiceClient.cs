using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;

namespace Game.ClientHandlerNet
{
    public class JsonWebServiceClient
    {
        public TResponse Post<TResponse>(string serviceUrl, object request)
        {
            var httpClient = new HttpClient(new HttpClientHandler()
            {
                UseProxy = false
            });

            var body = JsonConvert.SerializeObject(request, Formatting.None);

            var response = httpClient.PostAsync(serviceUrl, 
                new StringContent(body, Encoding.UTF8,"application/json")
                ).Result;

            return JsonConvert.DeserializeObject<TResponse>(
                    response.Content.ReadAsStringAsync().Result
                );
        }
    }
}
