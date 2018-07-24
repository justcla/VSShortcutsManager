using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSShortcutsManager
{
    internal class CommandShortcutsSearchTask : VsSearchTask
    {
        private CommandShortcuts parentWindow;
        private CommandShortcutsControlDataContext searchControlDataContext;

        public CommandShortcutsSearchTask(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback, CommandShortcuts parentWindow)
            : base(dwCookie, pSearchQuery, pSearchCallback)
        {
            this.parentWindow = parentWindow;
            var control = (CommandShortcutsControl)parentWindow.Content;
            this.searchControlDataContext = (CommandShortcutsControlDataContext)control.DataContext;
        }

        protected override void OnStartSearch()
        {
            uint resultCount = 0;
            this.ErrorCode = VSConstants.S_OK;

            try
            {
                string searchString = this.SearchQuery.SearchString;
                resultCount = this.searchControlDataContext.SearchCommands(searchString);
            }
            catch (Exception e)
            {
                this.ErrorCode = VSConstants.E_FAIL;
            }
            finally
            {
                this.SearchResults = resultCount;
            }

            // This sets the task status to complete and reports task completion.   
            base.OnStartSearch();
        }

        protected override void OnStopSearch()
        {
            this.SearchResults = 0;
        }
    }
}