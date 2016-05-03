using System;

namespace GameLogic.UserManagement
{
    public abstract class PermissionStrategy
    {
        public virtual bool HasAccess(object[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}
