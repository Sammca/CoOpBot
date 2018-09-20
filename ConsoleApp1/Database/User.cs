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

        public string OriginName { get; set; }

        public string gwAPIKey { get; set; }

        public string name { get; set; }

        public string description { get; set; }

        public string titleText { get; set; }

        public string titleURL { get; set; }

        public string footerText { get; set; }

        public string footerIconURL { get; set; }
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
            // Mase sure user record doesn't exist already
            if (this.exists(nameof(userID), $"{userID}"))
            {
                return false;
            }

            return base.validateInsert();
        }
    }
}
