using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace PublishToProGet
{
    internal sealed class OutputWindowLogger : Logger
    {
        private readonly IVsOutputWindowPane outputWindow;

        public OutputWindowLogger(IVsOutputWindowPane outputWindow)
        {
            this.outputWindow = outputWindow;
        }

        public override void Initialize(IEventSource eventSource)
        {
            eventSource.WarningRaised += EventSource_WarningRaised;
            eventSource.ErrorRaised += EventSource_ErrorRaised;
        }

        private void WriteLine(string message)
        {
            var hr = this.outputWindow.OutputStringThreadSafe(message + "\n");
            ErrorHandler.ThrowOnFailure(hr);
        }

        private void EventSource_WarningRaised(object sender, BuildWarningEventArgs e)
        {
            this.WriteLine(this.FormatWarningEvent(e));
        }

        private void EventSource_ErrorRaised(object sender, BuildErrorEventArgs e)
        {
            this.WriteLine(this.FormatErrorEvent(e));
        }
    }
}