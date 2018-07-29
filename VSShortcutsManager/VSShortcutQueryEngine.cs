using CommandTable;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using Task = System.Threading.Tasks.Task;

namespace VSShortcutsManager
{
    internal sealed class VSShortcutQueryEngine
    {
        #region Private Fields / Consts

        private const string KeyBindingTableRegKeyName = "KeyBindingTables";
        private const string PackageRegPropertyName = "Package";
        private const string DisplayNameRegPropertyName = "DisplayName";
        private const string AllowNavKeyBindingPropertyName = "AllowNavKeyBinding";

        /// <summary>
        /// The identifier of the shell package, which owns the localized key resources that PopulateLocalizedKeyNameMap needs.
        /// </summary>
        private static readonly Guid CLSID_VsEnvironmentPackage = new Guid("{DA9FB551-C724-11d0-AE1F-00A0C90FFFC3}");

        /// <summary>
        /// Base of resource identifiers, all ids are this + some fixed value.
        /// </summary>
        private static readonly uint ID_Intl_Base = 13000;

        /// <summary>
        /// ID of the resource identifying the 'global' scope in VS
        /// </summary>
        private static readonly uint GlobalScopeResId = ID_Intl_Base + 18;

        /// <summary>
        /// Removes access keys from the command strings returned from the VSCT file
        /// </summary>
        private static readonly AccessKeyRemovingConverter AccessKeyRemovingConverter = new AccessKeyRemovingConverter();

        // Backing store for various lazily fetched shell services
        private DTE dte;
        private Commands commands;
        private IVsShell shell;
        private IVsUIShell5 uiShell;
        private IServiceProvider serviceProvider;

        /// <summary>
        /// Static map from Key identifier to resource identifier in VS.
        /// </summary>
        private static readonly Dictionary<Key, uint> KeyResourceIdMap = new Dictionary<Key, uint>()
        {
            {Key.F1, ID_Intl_Base + 371},
            {Key.F2, ID_Intl_Base + 372},
            {Key.F3, ID_Intl_Base + 373},
            {Key.F4, ID_Intl_Base + 374},
            {Key.F5, ID_Intl_Base + 375},
            {Key.F6, ID_Intl_Base + 376},
            {Key.F7, ID_Intl_Base + 377},
            {Key.F8, ID_Intl_Base + 378},
            {Key.F9, ID_Intl_Base + 379 },
            {Key.F10, ID_Intl_Base + 380},
            {Key.F11, ID_Intl_Base + 381},
            {Key.F12, ID_Intl_Base + 382},
            {Key.F13, ID_Intl_Base + 383},
            {Key.F14, ID_Intl_Base + 384},
            {Key.F15, ID_Intl_Base + 385},
            {Key.F16, ID_Intl_Base + 386},
            {Key.F17, ID_Intl_Base + 2231},
            {Key.F18, ID_Intl_Base + 2232},
            {Key.F19, ID_Intl_Base + 2233},
            {Key.F20, ID_Intl_Base + 2234},
            {Key.F21, ID_Intl_Base + 2235},
            {Key.F22, ID_Intl_Base + 2236},
            {Key.F23, ID_Intl_Base + 2237},
            {Key.F24, ID_Intl_Base + 2238},
            {Key.Back, ID_Intl_Base + 361},
            {Key.Tab, ID_Intl_Base + 362},
            {Key.Cancel, ID_Intl_Base + 363},
            {Key.Pause, ID_Intl_Base + 364},
            {Key.Space, ID_Intl_Base + 365},
            {Key.Prior, ID_Intl_Base + 366},
            {Key.Next, ID_Intl_Base + 367},
            {Key.Home, ID_Intl_Base + 368},
            {Key.Insert, ID_Intl_Base + 369},
            {Key.Delete, ID_Intl_Base + 370},
            {Key.Left, ID_Intl_Base + 387},
            {Key.Right, ID_Intl_Base + 388},
            {Key.Up, ID_Intl_Base + 389},
            {Key.Down , ID_Intl_Base + 390},
            {Key.End , ID_Intl_Base + 391},
            {Key.Return, ID_Intl_Base + 392},
            {Key.Escape , ID_Intl_Base + 393},
            {Key.LWin, ID_Intl_Base + 875},
            {Key.RWin, ID_Intl_Base + 876},
            {Key.NumPad0, ID_Intl_Base + 1143},
            {Key.NumPad1, ID_Intl_Base + 1144},
            {Key.NumPad2, ID_Intl_Base + 1145},
            {Key.NumPad3, ID_Intl_Base + 1146},
            {Key.NumPad4, ID_Intl_Base + 1147},
            {Key.NumPad5, ID_Intl_Base + 1148},
            {Key.NumPad6, ID_Intl_Base + 1149},
            {Key.NumPad7, ID_Intl_Base + 1150},
            {Key.NumPad8, ID_Intl_Base + 1151},
            {Key.NumPad9, ID_Intl_Base + 1152},
            {Key.Multiply, ID_Intl_Base + 1153},
            {Key.Add, ID_Intl_Base + 1154},
            {Key.Subtract, ID_Intl_Base + 1155},
            {Key.Decimal, ID_Intl_Base + 1156},
            {Key.Divide, ID_Intl_Base + 1157},
            {Key.Clear, ID_Intl_Base + 394}
        };

        #region Various Maps

        /// <summary>
        /// Maps from a Key id to a localized name. Not all keys appear in this map, only ones for which VS has localization entries for keybinding purposes
        /// </summary>
        private readonly Dictionary<Key, string> keyIdKeyNameMap = new Dictionary<Key, string>();

        ///NOTE/BUG: Key.Cancel and Key.Pause both map to the string "Break", so mapping from "Break" to Cancel/Pause is ambiguous
        ///
        /// <summary>
        /// Map from key string name to key identifier.
        /// </summary>
        private readonly Dictionary<string, Key> keyNameKeyIdMap = new Dictionary<string, Key>();

        /// <summary>
        /// Maps a string form of a modifier key (like Alt) to a ModifierKeys value
        /// </summary>
        private readonly Dictionary<string, ModifierKeys> modifierNameKeyIdMap = new Dictionary<string, ModifierKeys>();

        /// <summary>
        /// Maps a ModifierKeys value to a localized string form
        /// </summary>
        private readonly Dictionary<ModifierKeys, string> modifierKeyIdToNameMap = new Dictionary<ModifierKeys, string>();

        /// <summary>
        /// Maps a scope GUID to information about that scope (such as its display name)
        /// </summary>
        private readonly Dictionary<Guid, KeybindingScope> scopeGuidToScopeInfoMap = new Dictionary<Guid, KeybindingScope>();

        /// <summary>
        /// Maps a scope name to information about that scope (such as its GUID)
        /// </summary>
        private readonly Dictionary<string, KeybindingScope> scopeNameToScopeInfoMap = new Dictionary<string, KeybindingScope>();

        /// <summary>
        /// Map from CommandId to Command object from the CTM. Populated in PopulateCTMCommands
        /// </summary>
        private readonly Dictionary<CommandId, CommandTable.Command> commandIdToCTMCommandMap = new Dictionary<CommandId, CommandTable.Command>();

        #endregion

        #endregion

        #region Private Properties

        private IVsUIShell5 UIShell
        {
            get
            {
                if (this.uiShell == null)
                {
                    this.uiShell = (IVsUIShell5)this.serviceProvider.GetService(typeof(SVsUIShell));
                }

                return this.uiShell;
            }
        }

        private IVsShell Shell
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                if (this.shell == null)
                {
                    this.shell = (IVsShell)this.serviceProvider.GetService(typeof(SVsShell));
                }

                return this.shell;
            }
        }

        private DTE DTE
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                if (this.dte == null)
                {
                    this.dte = (DTE)this.serviceProvider.GetService(typeof(SDTE));
                }

                return this.dte;
            }
        }

        private Commands DTECommands
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                if (this.commands == null)
                {
                    this.commands = this.DTE.Commands;
                }

                return this.commands;
            }
        }

        private Dictionary<Key, string> LocalizedKeyMap
        {
            get
            {
                if (this.keyIdKeyNameMap.Count == 0)
                {
                    ThreadHelper.ThrowIfNotOnUIThread();
                    PopulateKeyMaps();
                }

                return this.keyIdKeyNameMap;
            }
        }

        private Dictionary<string, ModifierKeys> ModifierNameKeyIdMap
        {
            get
            {
                if (this.modifierNameKeyIdMap.Count == 0)
                {
                    ThreadHelper.ThrowIfNotOnUIThread();

                    PopulateModiferKeyMaps();
                }

                return this.modifierNameKeyIdMap;
            }
        }

        private Dictionary<ModifierKeys, string> ModifierKeyIdToNameMap
        {
            get
            {
                if (this.modifierKeyIdToNameMap.Count == 0)
                {
                    ThreadHelper.ThrowIfNotOnUIThread();

                    PopulateModiferKeyMaps();
                }

                return this.modifierKeyIdToNameMap;
            }
        }

        private Dictionary<string, KeybindingScope> ScopeNameToScopeInfoMap
        {
            get
            {
                if (this.scopeNameToScopeInfoMap.Count == 0)
                {
                    PopulateScopeMaps();
                }

                return this.scopeNameToScopeInfoMap;
            }
        }

        private Dictionary<Guid, KeybindingScope> ScopeGuidToScopeInfoMap
        {
            get
            {
                if (this.scopeGuidToScopeInfoMap.Count == 0)
                {
                    PopulateScopeMaps();
                }

                return this.scopeGuidToScopeInfoMap;
            }
        }

        #endregion

        #region Ctor

        internal VSShortcutQueryEngine(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        #endregion

        #region Public Methods

        public async Task<IEnumerable<Command>> GetAllCommandsAsync()
        {
            List<Command> result = new List<Command>();
            if(this.commandIdToCTMCommandMap.Count == 0)
            {
                await this.PopulateCTMCommandsAsync();
            }

            foreach (EnvDTE.Command c in this.DTECommands)
            {
                List<CommandBinding> bindings = new List<CommandBinding>();

                if (c.Bindings != null && c.Bindings is object[] && ((object[])c.Bindings).Length > 0)
                {
                    object[] bindingsObj = (object[])c.Bindings;
                    foreach (string s in bindingsObj)
                    {
                        CommandBinding commandBinding = ParseBindingFromString(new Guid(c.Guid), c.ID, s);
                        if (commandBinding == null)
                        {
                            // Something went wrong with parsing (ie. Scope = "Unknown Editor") Skip this command.
                            continue;
                        }
                        bindings.Add(commandBinding);
                    }
                }

                CommandId id = new CommandId(Guid.Parse(c.Guid), c.ID);
                CommandTable.Command ctmCommand = this.commandIdToCTMCommandMap[id];
                string name = GetDisplayTextFromCTMCommand(ctmCommand);
                result.Add(new Command(id, name, c.Name, bindings));
            }

            return result;
        }

        public Task<IEnumerable<KeybindingScope>> GetAllBindingScopesAsync()
        {
            return Task.FromResult<IEnumerable<KeybindingScope>>(ScopeGuidToScopeInfoMap.Values);
        }

        public async Task<Command> GetCommandByCommandIdAsync(CommandId id)
        {
            IEnumerable<Command> commands = await GetAllCommandsAsync();
            return commands.Where((c) => { return ((c.Id.Id == id.Id) && (c.Id.Guid == id.Guid)); }).FirstOrDefault();
        }

        

        public async Task<IEnumerable<BindingConflict>> GetConflictsAsync(IEnumerable<Command> commands, KeybindingScope scope, IEnumerable<BindingSequence> sequences)
        {
            Dictionary<ConflictType, List<Tuple<CommandBinding, Command>>> conflictsMap = new Dictionary<ConflictType, List<Tuple<CommandBinding, Command>>>();

            BindingSequence[] callerChord = sequences.ToArray();
            bool callerIsTwoChordBinding = callerChord.Length == 2;

            // Populate the conflictsMap with all conflicting commands - grouped by conflict type (shadow, mask, replace)
            foreach(Command c in commands)
            {
                foreach(CommandBinding b in c.Bindings)
                {
                    // Compare the both part of the chords - if both are 2-chord bindings
                    if (callerIsTwoChordBinding && b.Sequences.Count == 2)
                    {
                        // Note: Only need to compare the second part of the chord here because the first part is always compared next.
                        if (!SameBindingSequence(callerChord[1], b.Sequences[1]))
                        {
                            // This binding is a chord and the second part doesn't match the second part of the chord passed in, so ignore it
                            continue;
                        }
                    }

                    // Compare the first part of the chords
                    if (!SameBindingSequence(callerChord[0], b.Sequences[0]))
                    {
                        // This binding doesn't start with the same sequence as the one give by the caller, so ignore it
                        continue;
                    }

                    ConflictType conflictType;
                    List<Tuple<CommandBinding, Command>> conflicts = null;

                    // If the first (and possibly only) chord of the caller supplied sequence and the first (and possibly only) chord of this binding match, then there are
                    // three possible conflicts we want to warn about (these are mututally exclusive, i.e. the given binding can only be ONE type of conflict in the list below).
                    //
                    // 1: If this binding is in a non-global scope, and the caller scope is global, applying the caller supplied binding will not be visible 
                    // inside this scope (ConflictType.HiddenInSomeScopes)
                    //
                    // 2: If this binding is in the global scope, and the caller supplied binding is NOT in the global scope, applying the caller supplied binding 
                    // will shadow it (make it inaccessible) (ConflictType.HidesGlobalBindings)
                    //
                    // 3: If this binding is in the same scope, applying the caller supplied binding will remove it (ConflictType.ReplacesBindings)
                    if(!ScopeIsGlobal(b.Scope.Guid) && ScopeIsGlobal(scope.Guid))
                    {
                        conflictType = ConflictType.HiddenInSomeScopes;
                    }
                    else if(ScopeIsGlobal(b.Scope.Guid) && !ScopeIsGlobal(scope.Guid))
                    {
                        conflictType = ConflictType.HidesGlobalBindings;
                    }
                    else if(b.Scope.Guid == scope.Guid)
                    {
                        conflictType = ConflictType.ReplacesBindings;
                    }
                    else
                    {
                        // This is the case where we have a binding 'conflict' but in scopes that aren't related (or we don't know they are related) so we can ignore it
                        continue;
                    }

                    // Add the conflict to the conflicts map, associated with the appropriate conflict type
                    if (!conflictsMap.TryGetValue(conflictType, out conflicts))
                    {
                        conflictsMap[conflictType] = conflicts = new List<Tuple<CommandBinding, Command>>();
                    }

                    conflicts.Add(new Tuple<CommandBinding, Command>(b,c));
                }
            }

            // Package the conflict data into BindingConflict objects (one for each conflict type)
            List<BindingConflict> result = new List<BindingConflict>();
            foreach(var kvp in conflictsMap)
            {
                result.Add(new BindingConflict(kvp.Key, kvp.Value));
            }

            return result;
        }

        public KeybindingScope GetScopeByName(string scopeName)
        {
            KeybindingScope scope;

            // If the name doesnt exist in the map we will simply return null, so ignore the success/failure aspect of TryGetValue as it will set the out param to null on failure
            ScopeNameToScopeInfoMap.TryGetValue(scopeName, out scope);

            return scope;
        }

        public IEnumerable<BindingSequence> GetBindingSequencesFromBindingString(string bindingString)
        {
            bindingString = bindingString.Trim();

            // Commas are complex as they can appear both as a keybinding key AND as a separator in multi-chord bindings, so we need to understand
            // which we are dealing with
            int commaCount = bindingString.Count((c) => c == ',');

            if (commaCount == 0 || (commaCount == 1 && bindingString.EndsWith(",")))
            {
                // Single chord binding
                Tuple<ModifierKeys, string> sequence = ParseSingleChordFromBindingString(bindingString);
                return new[] { new BindingSequence(sequence.Item1, sequence.Item2) };
                //return new CommandBinding(new CommandId(guid, id), this.ScopeNameToScopeInfoMap[scopeName], new[] { new BindingSequence(sequence.Item1, sequence.Item2) });
            }
            else
            {
                string[] chords;

                // Multi chord binding
                if (commaCount == 1) // easy case, we can just split on the comma as there are no commas as part of the binding
                {
                    chords = bindingString.Split(',');
                }
                else
                {
                    // okay, the binding itself has a comma, so, ugh :P
                    int splitPoint = bindingString.IndexOf(',');
                    string part1 = bindingString.Substring(0, splitPoint);
                    string part2 = bindingString.Substring(splitPoint + 1);

                    chords = new string[] { part1, part2 };
                }

                Tuple<ModifierKeys, string> sequence1 = ParseSingleChordFromBindingString(chords[0].Trim());
                Tuple<ModifierKeys, string> sequence2 = ParseSingleChordFromBindingString(chords[1].Trim());

                return new[] {
                                new BindingSequence(sequence1.Item1, sequence1.Item2),
                                new BindingSequence(sequence2.Item1, sequence2.Item2)
                             };

            }
        }

        Tuple<ModifierKeys, string> ParseSingleChordFromBindingString(string bindingString)
        {
            string[] splitKeys = bindingString.Split('+');

            ModifierKeys mods = ModifierKeys.None;
            foreach (string m in splitKeys)
            {
                if (!this.ModifierNameKeyIdMap.ContainsKey(m))
                {
                    break;
                }

                mods |= this.ModifierNameKeyIdMap[m];
            }

            // Special case bindings like Ctrl + Shift + +, the last + will be stripped so we just explicitly add it back 
            // if the original string ended with one
            return new Tuple<ModifierKeys, string>(mods, bindingString.EndsWith("+") ? "+" : splitKeys[splitKeys.Length - 1]);
        }

        private CommandBinding ParseBindingFromString(Guid guid, int id, string bindingString)
        {
            if (bindingString.Contains("::"))
            {
                string scopeName = bindingString.Substring(0, bindingString.IndexOf("::"));
                if (!this.ScopeNameToScopeInfoMap.ContainsKey(scopeName))
                {
                    System.Diagnostics.Debug.WriteLine("Unable to find ScopeInfo for scope name: " + scopeName);
                    return null;
                }

                string keyPortion = bindingString.Substring(bindingString.IndexOf("::") + 2);

                return new CommandBinding(keyPortion, new CommandId(guid, id), this.ScopeNameToScopeInfoMap[scopeName], GetBindingSequencesFromBindingString(keyPortion));
            }

            return null;
        }

        /// <summary>
        /// Get the localized strings that represent various keyboard keys. Copied from keynameinfo.cpp in env\msenv\core
        /// </summary>
        private void PopulateKeyMaps()
        {
            Guid shellPkg = CLSID_VsEnvironmentPackage;

            IVsShell shell = Shell;
            foreach(KeyValuePair<Key,uint> kvp in KeyResourceIdMap)
            {
                string keyName;
                if(ErrorHandler.Succeeded(shell.LoadPackageString(ref shellPkg, kvp.Value, out keyName)))
                {
                    this.keyNameKeyIdMap[keyName] = kvp.Key;
                    this.keyIdKeyNameMap[kvp.Key] = keyName;
                }
            }
        }

        private void PopulateModiferKeyMaps()
        {
            Guid shellPkg = CLSID_VsEnvironmentPackage;

            IVsShell shell = Shell;

            // Load the localized strings for the modifiers (Ctrl, Alt, Shift)
            string modifierName;
            if (ErrorHandler.Succeeded(shell.LoadPackageString(ref shellPkg, ID_Intl_Base + 358, out modifierName)))
            {
                this.modifierNameKeyIdMap[modifierName] = ModifierKeys.Control;
                this.modifierKeyIdToNameMap[ModifierKeys.Control] = modifierName;
            }

            if (ErrorHandler.Succeeded(shell.LoadPackageString(ref shellPkg, ID_Intl_Base + 359, out modifierName)))
            {
                this.modifierNameKeyIdMap[modifierName] = ModifierKeys.Alt;
                this.modifierKeyIdToNameMap[ModifierKeys.Alt] = modifierName;
            }

            if (ErrorHandler.Succeeded(shell.LoadPackageString(ref shellPkg, ID_Intl_Base + 360, out modifierName)))
            {
                this.modifierNameKeyIdMap[modifierName] = ModifierKeys.Shift;
                this.modifierKeyIdToNameMap[ModifierKeys.Shift] = modifierName;
            }
        }

        /// <summary>
        /// Populates the maps that map from name -> scope info and GUID -> scope info
        /// </summary>
        private void PopulateScopeMaps()
        {
            ShellSettingsManager settingsManager = new ShellSettingsManager(this.serviceProvider);
            SettingsStore settingsStore = settingsManager.GetReadOnlySettingsStore(SettingsScope.Configuration);

            // First build map of all registered scopes
            if(settingsStore.CollectionExists(KeyBindingTableRegKeyName))
            {
                int itemCount = settingsStore.GetSubCollectionCount(KeyBindingTableRegKeyName);

                foreach(string str in settingsStore.GetSubCollectionNames(KeyBindingTableRegKeyName))
                {
                    string collectionName = Path.Combine(KeyBindingTableRegKeyName, str);

                    Guid scopeId;
                    if(!Guid.TryParse(str, out scopeId))
                    {
                        continue;
                    }

                    Guid owningPackage;
                    uint resourceId;
                    bool allowNavKeyBinding = false;

                    if (scopeId == VSConstants.GUID_VSStandardCommandSet97)
                    {
                        owningPackage = CLSID_VsEnvironmentPackage;
                        resourceId = ID_Intl_Base + 18;
                    }
                    else
                    {
                        if (!settingsStore.PropertyExists(collectionName, PackageRegPropertyName))
                        {
                            continue;
                        }

                        if (!Guid.TryParse(settingsStore.GetString(collectionName, PackageRegPropertyName), out owningPackage))
                        {
                            continue;
                        }

                        string resIdString = settingsStore.GetString(collectionName, string.Empty);
                        if (resIdString.StartsWith("#"))
                        {
                            resIdString = resIdString.Substring(1);
                        }

                        if (!uint.TryParse(resIdString, out resourceId))
                        {
                            continue;
                        }

                        if (settingsStore.PropertyExists(collectionName, AllowNavKeyBindingPropertyName))
                        {
                            allowNavKeyBinding = settingsStore.GetUInt32(collectionName, AllowNavKeyBindingPropertyName) == 0 ? false : true;
                        }
                    }

                    string scopeName;
                    if (!ErrorHandler.Succeeded(Shell.LoadPackageString(ref owningPackage, resourceId, out scopeName)))
                    {
                        continue;
                    }

                    KeybindingScope scopeInfo = new KeybindingScope(scopeName, scopeId, allowNavKeyBinding);

                    this.scopeGuidToScopeInfoMap[scopeId] = scopeInfo;
                    this.scopeNameToScopeInfoMap[scopeName] = scopeInfo;
                }
            }

            IVsEnumGuids scopeEnum = UIShell.EnumKeyBindingScopes();

            // Random GUID the shell also skips ("Source Code Text Editor" scope)
            Guid toSkip = new Guid("{72F42A10-B1C5-11d0-A8CD-00A0C921A4D2}");

            Guid[] scopes = new Guid[1];
            uint fetched = 0;
            while(scopeEnum.Next((uint)scopes.Length, scopes, out fetched) == VSConstants.S_OK && fetched != 0)
            {
                // We already have info for this scope
                if(scopeGuidToScopeInfoMap.ContainsKey(scopes[0]))
                {
                    continue;
                }

                // The shell skips this as a possible binding scope
                if(scopes[0] == toSkip)                    
                {
                    continue;
                }

                string path = Path.Combine("Editors", scopes[0].ToString("B"));

                // If it isn't a registered scope, see if it is an editor factory
                if (!settingsStore.CollectionExists(path))
                {
                    continue;
                }

                if(!settingsStore.PropertyExists(path, PackageRegPropertyName))
                {
                    continue;
                }

                Guid packageGuid;
                if (!Guid.TryParse(settingsStore.GetString(path, PackageRegPropertyName), out packageGuid))
                {
                    continue;
                }

                if(!settingsStore.PropertyExists(path, DisplayNameRegPropertyName))
                {
                    continue;
                }

                string displayNameResIdStr = settingsStore.GetString(path, DisplayNameRegPropertyName);
                if(displayNameResIdStr.StartsWith("#"))
                {
                    displayNameResIdStr = displayNameResIdStr.Substring(1);
                }

                uint displayNameResId;
                if(!uint.TryParse(displayNameResIdStr, out displayNameResId))
                {
                    continue;
                }

                string displayName;
                if(!ErrorHandler.Succeeded(shell.LoadPackageString(ref packageGuid, displayNameResId, out displayName)))
                {
                    continue;
                }

                // NOTE: Is false the right default value?
                KeybindingScope scopeInfo = new KeybindingScope(displayName, scopes[0], allowNavKeyBinding: false);

                this.scopeGuidToScopeInfoMap[scopes[0]] = scopeInfo;
                this.scopeNameToScopeInfoMap[displayName] = scopeInfo;
            }
        }

        private string GetDisplayTextFromCTMCommand(CommandTable.Command command)
        {
            string text = null;

            if (command.ItemText.ButtonText != null)
                text = command.ItemText.ButtonText;
            else if (command.ItemText.CommandWellText != null)
                text = command.ItemText.CommandWellText;

            return (string)VSShortcutQueryEngine.AccessKeyRemovingConverter.Convert(text, typeof(string), null, CultureInfo.CurrentUICulture);
        }

        private async Task PopulateCTMCommandsAsync()
        {
            CommandTableFactory factory = new CommandTableFactory();
            ICommandTable commandTable = factory.CreateCommandTableFromHost(serviceProvider, CommandTable.HostLoadType.FromCTM);

            IEnumerable<CommandTable.Command> commands = await commandTable.GetCommands();
            foreach (var command in commands)
            {
                commandIdToCTMCommandMap[new CommandId(command.ItemId.Guid, (int)command.ItemId.DWord)] = command;
            }
        }

        public static void AddCommandBindingToBindingMap(Dictionary<string, IEnumerable<Tuple<CommandBinding, Command>>> bindingMap, Command command, CommandBinding binding, string key)
        {
            // Add the command/binding tuple to the relevant key in the binding map.
            IEnumerable<Tuple<CommandBinding, Command>> commandsForKey;
            if (!bindingMap.TryGetValue(key, out commandsForKey))
            {
                // Create new entry for the key if none exists.
                commandsForKey = new List<Tuple<CommandBinding, Command>>();
                bindingMap[key] = commandsForKey;
            }

           ((List<Tuple<CommandBinding, Command>>)commandsForKey).Add(new Tuple<CommandBinding, Command>(binding, command));
        }

        private static object[] AppendKeyboardBinding(EnvDTE.Command command, string keyboardBindingDefn)
        {
            object[] oldBindings = (object[])command.Bindings;

            // Check that keyboard binding is not already there
            for (int i = 0; i < oldBindings.Length; i++)
            {
                if (keyboardBindingDefn.Equals(oldBindings[i]))
                {
                    // Exit early and return the existing bindings array if new keyboard binding is already there
                    return oldBindings;
                }
            }

            // Build new array with all the old bindings, plus the new one.
            object[] newBindings = new object[oldBindings.Length + 1];
            Array.Copy(oldBindings, newBindings, oldBindings.Length);
            newBindings[newBindings.Length - 1] = keyboardBindingDefn;
            return newBindings;
        }
        /// <summary>
        /// Adds (appends) a binding for the given shortcut
        /// </summary>
        /// <param name="commandName"></param>
        /// <param name="shortcutDef"></param>
        public void BindShortcut(string commandName, string shortcutDef)
        {
            DTE dte = (DTE)serviceProvider.GetService(typeof(DTE));
            EnvDTE.Commands cmds = dte.Commands;
            //EnvDTE.Command shortCutCommand = DTECommands.Item(commandName);
            EnvDTE.Command shortCutCommand = cmds.Item(commandName);
            object[] newBindings = AppendKeyboardBinding(shortCutCommand, shortcutDef);
            shortCutCommand.Bindings = newBindings;
        }

        public static bool SameBindingSequence(BindingSequence bindingSeq1, BindingSequence bindingSeq2)
        {
            if (bindingSeq1 == null) return bindingSeq2 == null;
            if (bindingSeq2 == null) return false;
            return bindingSeq1.Modifiers == bindingSeq2.Modifiers
                && bindingSeq1.Key == bindingSeq2.Key;
        }

        public static bool ScopeIsGlobal(Guid scope)
        {
            return scope == VSConstants.GUID_VSStandardCommandSet97;
        }

        public static bool ScopeMatches(Guid comparisonBase, Guid toTest)
        {
            if (comparisonBase == Guid.Empty)
            {
                return true;
            }

            return comparisonBase == toTest;
        }

        #endregion

    }
}