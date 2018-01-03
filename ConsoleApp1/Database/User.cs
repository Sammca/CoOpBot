using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoOpBot.Database
{
    class User : DbBase
    {
        #region Fields
        public ulong userID { get; set; }

        public string steamID { get; set; }

        public string gwAPIKey { get; set; }
        #endregion

        public override string defaultFindField()
        {
            return nameof(userID);
        }

        public override bool validateWrite()
        {
            // Make sure mandatory fields aren't blank
            if (this.userID == ulong.MinValue)
            {
                return false;
            }
            return true;
        }

        public override bool validateInsert()
        {
            // Check we aren't translating to something that is already being translated from
            if (this.exists(nameof(userID), $"{userID}"))
            {
                return false;
            }

            return base.validateInsert();
        }
    }
}
