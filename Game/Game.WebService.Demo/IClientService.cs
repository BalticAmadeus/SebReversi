﻿using System.ServiceModel;
using System.ServiceModel.Web;
using Game.WebService.TransportClasses;

namespace Game.WebService
{
    [ServiceContract]
    public interface IClientService
    {
        /* Authentication */

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        InitLoginResp InitLogin(InitLoginReq req);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CompleteLoginResp CompleteLogin(CompleteLoginReq req);

        /* GameModel setup */

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CreatePlayerResp CreatePlayer(CreatePlayerReq req);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        WaitGameStartResp WaitGameStart(WaitGameStartReq req);

        /* GameModel progress */

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        LeaveGameResp LeaveGame(LeaveGameReq req);
    }

}
