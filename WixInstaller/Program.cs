using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using WixSharp;

namespace WixInstaller
{
    [System.Runtime.InteropServices.Guid("1230653A-0966-4829-9E0D-C6508B1277EA")]
    class Program
    {
        static void Main()
        {
            string productMsi = BuildMsi();
        }

        static string BuildMsi()
        {
            //Source for file to be installed
            string sRootDir = @"J:\MATCHITAntibodyRushExport\RushAntibody\WixInstaller\PublishedFiles";
            // Install Path
            string installPath = @"%ProgramFiles%\LIFECODES\MATCH IT! RUSH Export Utility";

            WixEntity[] installFiles = new WixEntity[0];
            installFiles = BuildDirInfo(sRootDir, installFiles);

            /* Install with one install directory shown below. If more than one directory, such as needing to install to the programdata folder, 
            * then add another new Dir({putInstallFolderPathHere},{putNewWixEntityHereForFiles}).
            */

            //string commonPath = @"C:\TFSSource\Wix Installer Projects\WixInstaller\PublishedFiles\CommonApp";
            //// Install Path
            //string commonInstallPath = @"%CommonAppDataFolder%\LIFECODES";

            //WixEntity[] commonFiles = new WixEntity[0];
            //commonFiles = BuildDirInfo(commonPath, commonFiles);

            var project = new Project("MATCH IT! RUSH Export Utility", new Dir(installPath, installFiles))
            {
                Version = new Version(1, 0, 0),
                UpgradeCode = new Guid("{3ffda3aa-0053-4072-9d00-42416389e47a}"),
                Id = "MATCHITRUSH_Export_Utility", // this is for internal application use, if more than one application is going to be install with this package.
                ControlPanelInfo = new ProductInfo() { Manufacturer = "Immucor, Inc." },
                InstallScope = InstallScope.perMachine,
                UI = WUI.WixUI_ProgressOnly,
                Platform = Platform.x86
            };

            project.MajorUpgradeStrategy = MajorUpgradeStrategy.Default;

            project.ResolveWildCards();

            // Edit Files
            var exeFile = project.AllFiles.Single(f => f.Name.EndsWith("MATCHITAntibodyRushExport.exe"));

            exeFile.Shortcuts = new[]
            {
                new FileShortcut("MATCH IT! RUSH Export Utility", @"%ProgramMenu%\Lifecodes") { WorkingDirectory = installPath },
                new FileShortcut("MATCH IT! RUSH Export Utility", @"%Desktop%") { WorkingDirectory = installPath }
            };

            Compiler.PreserveTempFiles = true;

            // Output path for MSI file.
            return project.BuildMsi(@"J:\MATCHITAntibodyRushExport\RushAntibody\WixSetupUI\Resources\MATCHITRUSH_Export_UtilitySetup.msi");
        }

        /// <summary>
        /// This method will find all files and directories in the source file location
        /// 
        private static WixEntity[] BuildDirInfo(string sRootDir, WixEntity[] weDir)
        {
            DirectoryInfo RootDirInfo = new DirectoryInfo(sRootDir);
            if (RootDirInfo.Exists)
            {
                DirectoryInfo[] DirInfo = RootDirInfo.GetDirectories();
                List<string> lMainDirs = new List<string>();
                foreach (DirectoryInfo DirInfoSub in DirInfo)
                    lMainDirs.Add(DirInfoSub.FullName);
                int cnt = lMainDirs.Count;
                weDir = new WixEntity[cnt + 1];
                if (cnt == 0)
                    weDir[0] = new DirFiles(RootDirInfo.FullName + @"\*.*");
                else
                {
                    weDir[cnt] = new DirFiles(RootDirInfo.FullName + @"\*.*");
                    for (int i = 0; i < cnt; i++)
                    {
                        DirectoryInfo RootSubDirInfo = new DirectoryInfo(lMainDirs[i]);
                        if (!RootSubDirInfo.Exists)
                            continue;
                        WixEntity[] weSubDir = new WixEntity[0];
                        weSubDir = BuildDirInfo(RootSubDirInfo.FullName, weSubDir);
                        weDir[i] = new Dir(RootSubDirInfo.Name, weSubDir);
                    }
                }
            }
            return weDir;
        }
    }
}