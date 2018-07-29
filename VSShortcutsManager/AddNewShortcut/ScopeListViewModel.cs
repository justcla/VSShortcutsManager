using System.Collections.Generic;

namespace VSShortcutsManager.AddNewShortcut
{
    public class ScopeListViewModel
    {
        public IEnumerable<ScopeDisplayItem> DataSource { get; }

        public ScopeListViewModel(IEnumerable<KeybindingScope> keybindingScopes)
        {
            this.DataSource = GetScopeDisplayData(keybindingScopes);
        }

        private static List<ScopeDisplayItem> GetScopeDisplayData(IEnumerable<KeybindingScope> keybindingScopes)
        {
            var displayData = new List<ScopeDisplayItem>();
            foreach (KeybindingScope keybindingScope in keybindingScopes)
            {
                ScopeDisplayItem displayItem = new ScopeDisplayItem()
                {
                    Name = keybindingScope.Name,
                    Guid = keybindingScope.Guid.ToString()
                };
                displayData.Add(displayItem);
            }

            return displayData;
        }

    }

    public class ScopeDisplayItem
    {
        public string Name { get; set; }
        public string Guid { get; set; }
    }

}
