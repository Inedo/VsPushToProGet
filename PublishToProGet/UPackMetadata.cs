using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

namespace PublishToProGet
{
    [JsonObject]
    internal sealed class UPackMetadata
    {
        private static readonly Regex GroupRegex = new Regex(@"^(?:[0-9a-zA-Z\-._](?:[0-9a-zA-Z\-./_]{0,48}[0-9a-zA-Z\-._])?)?$", RegexOptions.Compiled);
        private string group;
        [JsonProperty("group", NullValueHandling = NullValueHandling.Ignore)]
        public string Group
        {
            get
            {
                return this.group;
            }

            set
            {
                if (value != null && !GroupRegex.IsMatch(value))
                {
                    throw new FormatException("Group must be zero to fifty characters: numbers (0-9), upper- and lower-case letters (a-Z), dashes (-), periods (.), forward-slashes (/), and underscores (_); may not start or end with a forward-slash.");
                }
                this.group = string.IsNullOrEmpty(value) ? null : value;
            }
        }

        private static readonly Regex NameRegex = new Regex(@"^[0-9a-zA-Z\-._]{1,50}$", RegexOptions.Compiled);
        private string name;
        [JsonProperty("name", Required = Required.Always)]
        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (value == null || !NameRegex.IsMatch(value))
                {
                    throw new FormatException("Name must be one to fifty characters: numbers (0-9), upper- and lower-case letters (a-Z), dashes (-), periods (.), and underscores (_).");
                }
                this.name = value;
            }
        }

        private static readonly Regex VersionRegex = new Regex(@"^[0-9]+\.[0-9]+\.[0-9]+$", RegexOptions.Compiled);
        private string version;
        [JsonProperty("version", Required = Required.Always)]
        public string Version
        {
            get
            {
                return this.version;
            }

            set
            {
                if (value == null || !VersionRegex.IsMatch(value))
                {
                    throw new FormatException("Version must be a Semantic Version; this is a three-part, dot-separated specification.");
                }
                this.version = value;
            }
        }

        private string title;
        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title
        {
            get
            {
                return this.title;
            }

            set
            {
                if (value != null && value.Length > 50)
                {
                    throw new ArgumentException("Title must be no more than fifty characters.");
                }
                this.title = string.IsNullOrEmpty(value) ? null : value;
            }
        }

        private string description;
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description
        {
            get
            {
                return this.description;
            }

            set
            {
                this.description = string.IsNullOrEmpty(value) ? null : value;
            }
        }
    }
}
