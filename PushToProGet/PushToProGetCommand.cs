//------------------------------------------------------------------------------
// <copyright file="PushToProGetCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;

namespace PushToProGet
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class PushToProGetCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("4fa54517-6bdb-4820-a486-bddc6933973a");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="PushToProGetCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private PushToProGetCommand(Package package)
        {
            this.package = package ?? throw new ArgumentNullException("package");

            if (ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static PushToProGetCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new PushToProGetCommand(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            var wizard = new PushToProGetWizard(GetSelectedProject(), this.ServiceProvider);
            wizard.Show();
        }

        private IVsProject GetSelectedProject()
        {
            var monitorSelection = this.ServiceProvider.GetService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
            if (monitorSelection == null)
            {
                throw new Exception("Could not get SVsShellMonitorSelection");
            }

            ErrorHandler.ThrowOnFailure(monitorSelection.GetCurrentSelection(out var ppHier, out uint pitemid, out var ppMIS, out var ppSC));
            return (IVsProject)Marshal.GetObjectForIUnknown(ppHier);
        }
    }
}
