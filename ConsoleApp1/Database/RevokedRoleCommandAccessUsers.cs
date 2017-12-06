using System;

namespace CoOpBot.Database
{
    class RevokedRoleCommandAccessUsers : DbBase
    {
        #region Fields
        public ulong userID { get; set; }
        #endregion

        public override string defaultFindField()
        {
            return nameof(userID);
        }

        public override bool validateWrite()
        {
            // Make sure mandatory fields aren't blank
            if (userID == ulong.MinValue)
            {
                return false;
            }
            return true;
        }

        public override bool validateInsert()
        {
            // Check the user isn't already on the revoke list
            if (this.exists(nameof(userID), $"{userID}"))
            {
                return false;
            }

            return base.validateInsert();
        }
    }
}
