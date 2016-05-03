using System.Linq;

namespace GameLogic.UserManagement
{
    public class PermissionPlayerAccessByTeam : PermissionStrategy
    {
        public override bool HasAccess(object[] parameters)
        {
            var team = (Team)parameters.Single(p => p.GetType() == typeof(Team));
            var player = (Player)parameters.Single(p => p.GetType() == typeof(Player));

            return team.Role == TeamRole.Power || player.Team.Equals(team);
        }
    }
}
