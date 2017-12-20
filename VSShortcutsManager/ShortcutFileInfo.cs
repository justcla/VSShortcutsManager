using System;
using System.IO;
using System.Xml;

namespace VSShortcutsManager
{
    public class ShortcutFileInfo
    {

        public static readonly string DATETIME_FORMAT = "yyyy'-'MM'-'dd'T'HH':'mm':'ss";

        public string DisplayName { get; set; }
        public string Filepath { get; set; }
        public string ExtensionName { get; set; }
        public DateTime LastWriteTime { get; set; }
        public int NotifyFlag { get; set; }

        public ShortcutFileInfo() { }

        public ShortcutFileInfo(string filepath)
        {
            DisplayName = Path.GetFileNameWithoutExtension(filepath);
            Filepath = filepath;
            ExtensionName = GetExtensionNameFromPath(filepath);
            LastWriteTime = new FileInfo(filepath).LastWriteTime;
            NotifyFlag = 1;
        }

        private string GetExtensionNameFromPath(string filepath)
        {
            string directory = Path.GetDirectoryName(filepath);
            string extensionManifest = Path.Combine(directory, "extension.manifest");
            if (File.Exists(extensionManifest))
            {
                // TODO: Read extension manifest as XML and parse for extension name
                return GetExtensionNameFromExtensionManifest(extensionManifest);
            }
            return Path.GetFileNameWithoutExtension(filepath);  // HACK!
        }

        private string GetExtensionNameFromExtensionManifest(string extensionManifestFile)
        {
            // Load the document and set the root element.  
            XmlDocument doc = new XmlDocument();
            doc.Load(extensionManifestFile);
            XmlNode root = doc.DocumentElement;
            XmlNode node = root.SelectSingleNode("/PackageManifest/Metadata/DisplayName");
            if (node != null)
            {
                return node.Value;
            }
            return null;
        }

        public bool LastWriteTimeEquals(DateTime lastWriteTime)
        {
            return DateTimesAreEqual(this.LastWriteTime, lastWriteTime);
        }

        public static bool DateTimesAreEqual(DateTime dateTime1, DateTime dateTime2)
        {
            return dateTime1.ToString(DATETIME_FORMAT).Equals(dateTime2.ToString(DATETIME_FORMAT));
        }
    }

}