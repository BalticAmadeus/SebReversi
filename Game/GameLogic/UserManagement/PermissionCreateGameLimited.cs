using System.Linq;

namespace GameLogic.UserManagement
{
    public class PermissionCreateGameLimited : PermissionCreateGame
    {
        public override bool HasAccess(object[] parameters)
        {
            var team = (Team)parameters.Single(p => p.GetType() == typeof(Team));

            return base.HasAccess(parameters) || team.Role == TeamRole.Normal;
        }
    }
}
