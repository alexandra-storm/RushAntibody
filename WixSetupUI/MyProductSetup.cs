using System;
using WixSharp.UI;

namespace WixSetupUI
{
    public class MyProductSetup : GenericSetup
    {
        public bool InitialCanInstall { get; set; }

        public bool InitialCanUnInstall { get; set; }

        public bool InitialCanRepair { get; set; }
        public bool IsUpdating { get; set; }

        public void StartRepair()
        {
            //The MSI will abort any attempt to start unless CUSTOM_UI is set. This  a feature for preventing starting the MSI without this custom GUI.
            base.StartRepair("CUSTOM_UI=true");
        }

        public void StartChange()
        {
            //Adjust the MSI properties to indicate which feature you want to install
            //base.StartRepair("CUSTOM_UI=true");
            base.StartRepair();
        }

        public void StartInstall()
        {
            //CheckIfUpdating();
            base.StartInstall();
        }

        public MyProductSetup(string msiFile, bool enableLoging = true)
            : base(msiFile, enableLoging)
        {
            InitialCanInstall = CanInstall;
            InitialCanUnInstall = CanUnInstall;
            InitialCanRepair = CanRepair;


            SetupStarted += MyProductSetup_SetupStarted;
            SetupComplete += MyProductSetup_SetupComplete;

            //Uncomment if you want to see current action name changes. Otherwise it is too quick.
            //ProgressStepDelay = 50;
        }

        private void MyProductSetup_SetupComplete()
        {
            if (IsUpdating)
            {
                using (SystemInfo systemInfo = new SystemInfo())
                {
                    CurrentActionName = "Restoring config file";
                   // systemInfo.RestoreConfigFiles();
                    CurrentActionName = "";
                }
            }

            if (InstallState == InstallStates.Installing)
            {
                if (ErrorState == ErrorStates.Success)
                    ErrorStatus = "Installation Successful";
                else
                    ErrorStatus = "Installation Failed";
            }
            else
            {
                if (ErrorState == ErrorStates.Success)
                    ErrorStatus = "Uninstall Successful";
                else
                    ErrorStatus = "Uninstall Failed";
            }
        }

        public void CheckIfUpdating()
        {
            using (SystemInfo systemInfo = new SystemInfo())
            {
                if (systemInfo.CheckIfAppExistsAlready("AppName"))
                {
                    IsUpdating = true;
                    //CurrentActionName = "Backing up config file";
                    systemInfo.BackupConfigFiles();
                }
            }
        }

        private void MyProductSetup_SetupStarted()
        {
            //this.LogFileCreated
            // IsNotRuning = false;
        }
    }
}