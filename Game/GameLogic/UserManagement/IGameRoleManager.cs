namespace GameLogic.UserManagement
{
    public interface IGameRoleManager
    {
        bool HasAccess(PermissionStrategy permissionStrategy, object[] parameters);
    }
}
