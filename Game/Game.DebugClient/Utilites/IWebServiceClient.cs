using System.Threading.Tasks;

namespace Game.DebugClient.Utilites
{
    public interface IWebServiceClient
    {
        Task<TResponse> Post<TRequest, TResponse>(string serviceUrl, TRequest request);
    }
}