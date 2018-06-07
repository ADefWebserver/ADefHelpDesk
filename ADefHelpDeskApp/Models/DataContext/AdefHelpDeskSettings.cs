using System;
using System.Collections.Generic;

namespace AdefHelpDeskBase.Models.DataContext
{
    public partial class AdefHelpDeskSettings
    {
        public int SettingId { get; set; }
        public int PortalId { get; set; }
        public string SettingName { get; set; }
        public string SettingValue { get; set; }
    }
}
