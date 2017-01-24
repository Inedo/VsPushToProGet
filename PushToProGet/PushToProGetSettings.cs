using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace PushToProGet
{
    public sealed class PushToProGetSettings : DialogPage
    {
        [Category("Defaults")]
        [DisplayName("Universal feed URL")]
        public string DefaultUniversalFeed { get; set; }

        [Category("Defaults")]
        [DisplayName("ProGet username")]
        public string DefaultUserName { get; set; }
    }
}