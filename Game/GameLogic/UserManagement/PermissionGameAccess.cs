using System.Linq;

namespace GameLogic.UserManagement
{
    public class PermissionGameAccess : PermissionStrategy
    {
        public override bool HasAccess(object[] parameters)
        {
            var team = (Team)parameters.Single(p => p.GetType() == typeof(Team));
            var gameModel = (GameModel)parameters.Single(p => p.GetType() == typeof(GameModel));

            return team.Role == TeamRole.Power || gameModel.Owner.Equals(team);
        }
    }
}
