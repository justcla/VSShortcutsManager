using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSShortcutsManager
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("124118e3-7c75-490e-8ace-742c96f001da")]
    public class CommandShortcuts : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandShortcuts"/> class.
        /// </summary>
        public CommandShortcuts() : base(null)
        {
            this.Caption = "CommandShortcuts";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new CommandShortcutsControl();
        }

        protected override void Initialize()
        {
            base.Initialize();

            ((CommandShortcutsControl)this.Content).DataContext = new CommandShortcutsControlDataContext(this);
        }

        #region Search

        public override IVsSearchTask CreateSearch(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback)
        {
            if (pSearchQuery == null || pSearchCallback == null)
            {
                return null;
            }

            return new CommandShortcutsSearchTask(dwCookie, pSearchQuery, pSearchCallback, this);
        }

        public override void ClearSearch()
        {
            var control = (CommandShortcutsControl)this.Content;
            var controlDataContext = (CommandShortcutsControlDataContext)control.DataContext;
            controlDataContext.ClearSearch();
        }

        private IVsEnumWindowSearchOptions m_optionsEnum;
        public override IVsEnumWindowSearchOptions SearchOptionsEnum
        {
            get
            {
                if (m_optionsEnum == null)
                {
                    List<IVsWindowSearchOption> list = new List<IVsWindowSearchOption>();

                    list.Add(this.MatchCaseOption);

                    m_optionsEnum = new WindowSearchOptionEnumerator(list) as IVsEnumWindowSearchOptions;
                }

                return m_optionsEnum;
            }
        }

        private WindowSearchBooleanOption m_matchCaseOption;
        public WindowSearchBooleanOption MatchCaseOption
        {
            get
            {
                if (m_matchCaseOption == null)
                {
                    m_matchCaseOption = new WindowSearchBooleanOption("Match case", "Match case", false);
                }

                return m_matchCaseOption;
            }
        }

        public override bool SearchEnabled => true;

        #endregion // Search
    }
}
