using System;

namespace VSShortcutsManager
{
    public class VskMappingInfo
    {
        public string Name;
        public string Filepath;
        public DateTime lastWriteTime;
        public int updateFlag; // 0 = never; 1 = prompt; 2 = always
    }

}