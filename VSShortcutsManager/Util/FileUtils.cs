using System;
using System.Diagnostics;
using System.IO;

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

        /// <summary>
        /// Credit: https://social.msdn.microsoft.com/Forums/vstudio/en-US/919c95d4-7ec8-404e-a488-c01c2730c7f5/how-to-wait-for-file-access?forum=netfxbcl
        /// 
        /// This method loops with a sleep until either
        ///  - It was able to obtain file write access, or
        ///  - A timeout has occurred
        /// </summary>
        public static void WaitForFileAccess(String path, double intervalMillis = 100, double timeoutSeconds = 3)
        {
            TimeSpan sleep = TimeSpan.FromMilliseconds(intervalMillis);
            TimeSpan timeout = TimeSpan.FromSeconds(timeoutSeconds);

            DateTime start = DateTime.Now;

            // A do-while loop is used to ensure that we check for file access at least
            // once
            do
            {
                try
                {
                    // Try to get write access, closing the file when done
                    using (var file = File.OpenWrite(path))
                    {
                    }
                    return;
                }
                catch (IOException e)
                {
                    // We were unable to get file write access
                    //LogMessage(LogLevel.Info, "Waiting for file to be accessible : " + e.Message);
                    System.Threading.Thread.Sleep(sleep);
                }
            }
            while (DateTime.Now - start < timeout);

            // We can only reach this point if the while loop exited due to a timeout
            //LogMessage(LogLevel.Warning, "Giving up waiting for file to be accessible : " + path);
        }


    }
}
