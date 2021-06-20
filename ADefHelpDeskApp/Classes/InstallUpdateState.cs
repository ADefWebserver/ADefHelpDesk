using System;
using System.Collections.Generic;
using System.Text;

namespace ADefHelpDeskApp
{
    class InstallUpdateState
    {
        public string InstallUpgradeWizardStage { get; set; }
        public bool DatabaseReady { get; set; }
        public string DatabaseConectionString { get; set; }
    }
}
