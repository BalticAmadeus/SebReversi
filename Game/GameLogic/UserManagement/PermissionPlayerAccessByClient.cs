using System.Linq;

namespace GameLogic.UserManagement
{
    public class PermissionPlayerAccessByClient : PermissionStrategy
    {
        public override bool HasAccess(object[] parameters)
        {
            var player = (Player)parameters.Single(p => p.GetType() == typeof(Player));
            var clientCode = (ClientCode)parameters.Single(p => p.GetType() == typeof(ClientCode));

            return player.Name == clientCode.ClientName && player.Team.Name == clientCode.TeamName;
        }
    }
}
