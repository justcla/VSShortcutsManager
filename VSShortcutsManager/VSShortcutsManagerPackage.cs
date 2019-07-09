using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace VSShortcutsManager
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(VSShortcutsManagerPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideToolWindow(typeof(CommandShortcuts))]
    [ProvideAutoLoad(UIContextGuids.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class VSShortcutsManagerPackage : AsyncPackage
    {
        /// <summary>
        /// VSSettingsManagerPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "2145fd5c-c814-4772-b19d-b840113afede";
        // Initialize settings manager (TODO: could be done lazily on get)
        private const string SID_SVsSettingsPersistenceManager = "9B164E40-C3A2-4363-9BC5-EB4039DEF653";
        public static ISettingsManager SettingsManager { get; private set; }

        public VSShortcutsManagerPackage()
        {
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override async System.Threading.Tasks.Task InitializeAsync(System.Threading.CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            // When initialized asynchronously, we *may* be on a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            // Otherwise, remove the switch to the UI thread if you don't need it.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // Initialize settings manager (TODO: could be done lazily on get)
            SettingsManager = (ISettingsManager)await this.GetServiceAsync(typeof(SVsSettingsPersistenceManager));

            // Adds commands handlers for the VS Shortcuts operations (Apply, Backup, Restore, Reset)
            VSShortcutsManager.Initialize(this);
        }

        // A horrible hack but SVsSettingsPersistenceManager isn't public and we need something with the right GUID to get the service.
        [Guid(SID_SVsSettingsPersistenceManager)]
        private class SVsSettingsPersistenceManager
        { }

    }
}
