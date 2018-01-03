using System;

namespace CoOpBot.Database
{
    class RoleTranslations : DbBase
    {
        #region Fields
        public string translateFrom { get; set; }

        public string translateTo { get; set; }
        #endregion

        public override string defaultFindField()
        {
            return nameof(translateFrom);
        }

        public override bool validateWrite()
        {
            // Make sure mandatory fields aren't blank
            if (this.translateFrom == "" ||
                this.translateTo == "")
            {
                return false;
            }
            return true;
        }

        public override bool validateInsert()
        {
            // Check we aren't translating to something that is already being translated from
            if (this.exists(nameof(translateFrom), translateTo))
            {
                return false;
            }
            // Check we aren't translating from something that is already being translated from
            if (this.exists(nameof(translateFrom), translateFrom))
            {
                return false;
            }
            // Check we aren't translating from something that is already being translated to
            if (this.exists(nameof(translateTo), translateFrom))
            {
                return false;
            }

            return base.validateInsert();
        }
    }
}
