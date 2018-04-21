using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace VSShortcutsManager
{
    public class FileUtils
    {
        public static string BrowseForFile(string fileFilter, string initialFileOrFolder)
        {
            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
            // Set file filter
            if (fileDialog.Filter != null)
            {
                fileDialog.Filter = fileFilter;
            }
            // Set initial directory
            string initialDirectory = GetDirectoryFromPath(initialFileOrFolder);
            if (initialDirectory != null)
            {
                fileDialog.InitialDirectory = initialDirectory;
            }
            // Open dialog and prompt for file
            string chosenFile = null;
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                chosenFile = fileDialog.FileName;
            }
            return chosenFile;
        }

        private static string GetDirectoryFromPath(string startPath)
        {
            if (File.Exists(startPath))
            {
                return Path.GetDirectoryName(startPath);
            }
            if (Directory.Exists(startPath))
            {
                return startPath;
            }
            return null;
        }

        public static void CopyVSKToIDEDir(string fileToCopy)
        {
            // Bail out if file does not exist
            if (!File.Exists(fileToCopy)) return;

            CopyFileUsingXCopy(fileToCopy, VSPathUtils.GetVsInstallPath());
        }

        public static void CopyFileUsingXCopy(string fileToCopy, string destination)
        {
            // Bail out if file does not exist
            if (!File.Exists(fileToCopy)) return;

            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = @"xcopy.exe";
            process.StartInfo.Arguments = string.Format(@"""{0}"" ""{1}""", fileToCopy, destination);
            if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major > 5)
            {
                process.StartInfo.Verb = "runas";
            }
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
        }

    }
}
