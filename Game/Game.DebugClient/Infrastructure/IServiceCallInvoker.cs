using System.Threading.Tasks;
using Game.DebugClient.DataContracts;

namespace Game.DebugClient.Infrastructure
{
    public interface IServiceCallInvoker
    {
        Task<TResp> InvokeAsync<TReq, TResp>(string serviceUrl, TReq req)
            where TReq : BaseReq
            where TResp : BaseResp, new();

        event InvokedEventHandler InvokeBegan;
        event InvokedEventHandler InvokeFinished;
    }

    public delegate void InvokedEventHandler(object sender, InvokeEventArgs args);
}