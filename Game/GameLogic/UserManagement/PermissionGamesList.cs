using System.Linq;

namespace GameLogic.UserManagement
{
    public class PermissionGamesList : PermissionGameAccess
    {
        public override bool HasAccess(object[] parameters)
        {
            var team = (Team)parameters.Single(p => p.GetType() == typeof(Team));

            return team.Role == TeamRole.Observer || base.HasAccess(parameters);
        }
    }
}
