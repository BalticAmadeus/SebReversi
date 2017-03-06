using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Game.ClientHandlerNet
{
    public class JsonWebServiceClient
    {
        public TResponse Post<TResponse>(string serviceUrl, object request)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(serviceUrl);
            webRequest.Method = WebRequestMethods.Http.Post;

            var content = new UTF8Encoding().GetBytes(JsonConvert.SerializeObject(request));

            webRequest.ContentLength = content.Length;
            webRequest.ContentType = "application/json";

            using (var requestStream = webRequest.GetRequestStream())
            {
                requestStream.Write(content, 0, content.Length);
            }

            using (var webResponse = (HttpWebResponse)webRequest.GetResponse())
            using (var streamReader = new StreamReader(webResponse.GetResponseStream()))
            {
                var response = JsonConvert.DeserializeObject<TResponse>(streamReader.ReadToEnd());
                return response;
            }
        }
    }
}
