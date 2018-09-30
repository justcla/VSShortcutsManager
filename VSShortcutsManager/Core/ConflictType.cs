namespace VSShortcutsManager
{
    public enum ConflictType
    {
        /// <summary>
        /// The given keybinding is in the global scope but will be hidden by keybindings in more specific scopes
        /// </summary>
        HiddenInSomeScopes,

        /// <summary>
        /// The given keybinding has the effect of making keybindings from the global scope inaccessible when the associated scope of the binding is active
        /// </summary>
        HidesGlobalBindings,

        /// <summary>
        /// The given keybinding has the effect of replacing keybindings in the associated scope
        /// </summary>
        ReplacesBindings
    }
}
