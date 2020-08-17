using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using UnityProjectTemplates.Resources;
using Task = System.Threading.Tasks.Task;

namespace UnityProjectTemplates
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(Guids.GuidUnityProjectTemplateCommandPackageString)]
    [ProvideUIContextRule(Guids.GuidUnityTemplatesUIContext,
        name: "Unity Item Templates UI Context",
        expression: "HasUnityProjects",
        termNames: new [] { "HasUnityProjects" },
        termValues: new[] { "ActiveProjectFlavor:E097FAD1-6243-4DAD-9C02-E9B9EFC3FFC1" }
        )]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class UnityProjectTemplateCommandPackage : AsyncPackage
    {
        public static DTE2 dte;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityProjectTemplateCommandPackage"/> class.
        /// </summary>
        public UnityProjectTemplateCommandPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members
        
        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            dte = await GetServiceAsync(typeof(DTE)) as DTE2;

            await RegisterCommandsAsync();
        }

        private async Task RegisterCommandsAsync()
        {
            await RegisterCommandAsync(Guids.GuidUnityNewScriptCmdSet, CommandId.NewMonoBehaviourCommandId, ToEventHandlerExecuteCommand(Strings.UnityMonoBehaviour));
            await RegisterCommandAsync(Guids.GuidUnityNewScriptCmdSet, CommandId.NewScriptableObjectCommandId, ToEventHandlerExecuteCommand(Strings.UnityScriptableObject));
            await RegisterCommandAsync(Guids.GuidUnityNewScriptCmdSet, CommandId.NewStateMachineBehaviourCommandId, ToEventHandlerExecuteCommand(Strings.UnityStateMachineBehaviour));
            await RegisterCommandAsync(Guids.GuidUnityNewTestCmdSet, CommandId.NewUnityTestCommandId, ToEventHandlerExecuteCommand(Strings.UnityTestCase));
            await RegisterCommandAsync(Guids.GuidUnityNewShaderCmdSet, CommandId.NewImageEffectShaderCommandId, ToEventHandlerExecuteCommand(Strings.UnityImageEffectShader));
            await RegisterCommandAsync(Guids.GuidUnityNewShaderCmdSet, CommandId.NewSurfaceShaderCommandId, ToEventHandlerExecuteCommand(Strings.UnitySurfaceShader));
            await RegisterCommandAsync(Guids.GuidUnityNewShaderCmdSet, CommandId.NewUnlitShaderCommandId, ToEventHandlerExecuteCommand(Strings.UnityUnlitShader));
        }

        private EventHandler ToEventHandlerExecuteCommand(string templateName)
        {
            return (sender, args) => ExecuteNewFileFromTemplateByCommandId(templateName);
        }

        private async Task RegisterCommandAsync(Guid group, CommandId id, EventHandler execute)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(DisposalToken);

            try
            {
                var commandService = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;

                if (commandService == null)
                    return;

                var command = new CommandID(group, (int)id);
                var menuItem = new MenuCommand(execute, command);

                commandService.AddCommand(menuItem);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
            }            
        }

        private void ExecuteNewFileFromTemplateByCommandId(string templateName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            InvokeAddNewItemDialog(templateName);
        }

        private void InvokeAddNewItemDialog(string templateName)
        {
            try
            {
                var strFilter = string.Empty;

                var hierarchy = GetCurrentVsHierarchySelection(out uint projectItemId);
                if (hierarchy == null)
                    return;

                var project = ToDteProject(hierarchy);
                if (project == null)
                    return;

                var vsProject = ToVsProject(project);
                if (vsProject == null)
                    return;

                var dialogService = GetService(typeof(IVsAddProjectItemDlg)) as IVsAddProjectItemDlg;
                if (dialogService == null)
                    return;

                const uint uiFlags = (uint)(__VSADDITEMFLAGS.VSADDITEM_AddNewItems | __VSADDITEMFLAGS.VSADDITEM_SuggestTemplateName | __VSADDITEMFLAGS.VSADDITEM_AllowHiddenTreeView);

                var projGuid = Guids.GuidCSharpProject;

                string projectDirectoryPath;
                if (hierarchy.GetCanonicalName(projectItemId, out projectDirectoryPath) != VSConstants.S_OK)
                    return;

                dialogService.AddProjectItemDlg(projectItemId,
                                                ref projGuid,
                                                vsProject,
                                                uiFlags,
                                                Strings.UnityTemplateCategory,
                                                templateName,
                                                ref projectDirectoryPath,
                                                ref strFilter,
                                                out int dontShowAgain);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
            }            
        }

        private static IVsHierarchy GetCurrentVsHierarchySelection(out uint projectItemId)
        {
            var monitorSelection = (IVsMonitorSelection)GetGlobalService(typeof(SVsShellMonitorSelection));
            monitorSelection.GetCurrentSelection(out IntPtr hierarchyPtr, out projectItemId, out _, out _);

            var hierarchy = Marshal.GetTypedObjectForIUnknown(hierarchyPtr, typeof(IVsHierarchy)) as IVsHierarchy;
            return hierarchy;
        }

        private static Project ToDteProject(IVsHierarchy hierarchy)
        {
            if (hierarchy == null)
                throw new ArgumentNullException(nameof(hierarchy));

            if (hierarchy.GetProperty(0xfffffffe, (int)__VSHPROPID.VSHPROPID_ExtObject, out object prjObject) == VSConstants.S_OK)
                return (Project)prjObject;

            throw new ArgumentException("Hierarchy is not a project.");
        }

        private IVsProject ToVsProject(Project project)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            var vsSln = GetService(typeof(IVsSolution)) as IVsSolution;
            if (vsSln == null)
                throw new ArgumentException("Project is not a VS project.");

            vsSln.GetProjectOfUniqueName(project.UniqueName, out IVsHierarchy vsHierarchy);

            var vsProject = vsHierarchy as IVsProject;
            if (vsProject != null)
                return vsProject;

            throw new ArgumentException("Project is not a VS project.");
        }

        #endregion
    }
}
