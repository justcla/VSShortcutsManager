using System;
using System.Collections.Generic;

namespace VSShortcutsManager
{
    /// <summary>
    /// A class representing a binding conflict of some type
    /// </summary>
    public class BindingConflict
    {
        public BindingConflict(ConflictType type, IEnumerable<Tuple<CommandBinding, Command>> affectedBindings)
        {
            this.Type = type;
            this.AffectedBindings = affectedBindings;
        }

        /// <summary>
        /// The type of the conflict
        /// </summary>
        public ConflictType Type {  get; private set;}

        /// <summary>
        /// The bindings affected by the conflict
        /// </summary>
        public IEnumerable<Tuple<CommandBinding, Command>> AffectedBindings;
    }
}