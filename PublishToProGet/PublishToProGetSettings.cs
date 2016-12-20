using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace PublishToProGet
{
    public sealed class PublishToProGetSettings : DialogPage
    {
        [Category("Defaults")]
        [DisplayName("Universal feed URL")]
        public string DefaultUniversalFeed {get;set;}
    }
}