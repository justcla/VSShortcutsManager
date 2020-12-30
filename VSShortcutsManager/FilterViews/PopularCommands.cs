using System.Collections.Generic;

namespace VSShortcutsManager
{
    public class PopularCommands
    {
        private static List<string> popularCommandNames;

        public static List<string> CommandList
        {
            get
            {
                if (popularCommandNames == null)
                {
                    popularCommandNames = new List<string>
                    {
                        "Edit.Undo",
                        "Edit.Redo",
                        "Edit.Copy",
                        "Edit.Cut",
                        "Edit.Paste"
                    };

                }
                return popularCommandNames;
            }
            set { }
        }
    }
}
