namespace GameLogic.UserManagement
{
    public class ReversiRoleManager : IGameRoleManager
    {
        public bool HasAccess(PermissionStrategy permissionStrategy, object[] parameters)
        {
            return permissionStrategy.HasAccess(parameters);
        }
    }
}
