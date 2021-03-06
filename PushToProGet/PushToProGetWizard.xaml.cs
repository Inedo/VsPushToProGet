﻿using EnvDTE;
using Microsoft.Build.Evaluation;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media;

namespace PushToProGet
{
    /// <summary>
    /// Interaction logic for PushToProGetWizard.xaml
    /// </summary>
    public partial class PushToProGetWizard : System.Windows.Window
    {
        private readonly IVsProject Project;
        private readonly EnvDTE.Project dteProject;
        private readonly IServiceProvider ServiceProvider;
        private readonly PushToProGetSettings Settings;
        internal readonly UPackMetadata Metadata = new UPackMetadata();

        internal PushToProGetWizard(IVsProject project, IServiceProvider serviceProvider)
        {
            this.Project = project;
            this.ServiceProvider = serviceProvider;
            InitializeComponent();

            if (this.ServiceProvider is Microsoft.VisualStudio.Shell.Package package)
            {
                this.Settings = (PushToProGetSettings)package.GetDialogPage(typeof(PushToProGetSettings));
                this.universalFeedURL.Text = this.Settings.DefaultUniversalFeed;
                this.universalFeedUser.Text = this.Settings.DefaultUserName;
            }

            if (string.IsNullOrEmpty(this.universalFeedURL.Text))
            {
                this.universalFeedURL.Focus();
            }
            else if (string.IsNullOrEmpty(this.universalFeedUser.Text))
            {
                this.universalFeedUser.Focus();
            }
            else
            {
                this.universalFeedPassword.Focus();
            }

            var dte = (DTE)this.ServiceProvider.GetService(typeof(DTE));
            ErrorHandler.ThrowOnFailure(((IVsHierarchy)this.Project).GetProperty((uint)VSConstants.VSITEMID.Root, (int)__VSHPROPID.VSHPROPID_ExtObject, out object projectObject));
            this.dteProject = (EnvDTE.Project)projectObject;
            this.packageName.Text = this.dteProject.Properties.Item("AssemblyName").Value as string ?? this.dteProject.Name;
            var version = this.dteProject.Properties.Item("AssemblyVersion").Value as string ?? "";
            var splitVersion = version.Split('.');
            if (splitVersion.Length == 4 && splitVersion.All(part => part.All(c => c >= '0' && c <= '9')))
            {
                version = string.Join(".", splitVersion, 0, 3);
            }
            this.packageVersion.Text = version;
            this.packageDirectory.Text = Path.Combine(Path.GetDirectoryName(dteProject.FullName), this.dteProject.ConfigurationManager?.ActiveConfiguration?.Properties?.Item("OutputPath")?.Value as string ?? "");
        }

        private void PackageDirectoryButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var folderBrowser = new FolderBrowserDialog()
            {
                SelectedPath = this.packageDirectory.Text,
                ShowNewFolderButton = false
            };
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.packageDirectory.Text = folderBrowser.SelectedPath;
            }
        }

        private void Page1_Next(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                new Uri(this.universalFeedURL.Text, UriKind.Absolute);
            }
            catch (FormatException ex)
            {
                MessageBox.Show("Universal feed URL is invalid.\n\n" + ex.Message, "Form validation error");
                this.universalFeedURL.Focus();
                return;
            }

            this.page1.Visibility = System.Windows.Visibility.Hidden;
            this.page2.Visibility = System.Windows.Visibility.Visible;
            if (string.IsNullOrEmpty(this.Settings.DefaultUniversalFeed))
            {
                this.Settings.DefaultUniversalFeed = this.universalFeedURL.Text;
                this.Settings.DefaultUserName = this.universalFeedUser.Text;
                this.Settings.SaveSettingsToStorage();
            }
            this.packageName.Focus();
        }

        private void Page2_Next(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                this.Metadata.Name = this.packageName.Text;
            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message, "Form validation error");
                this.packageName.Focus();
                return;
            }

            try
            {
                this.Metadata.Version = this.packageVersion.Text;
            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message, "Form validation error");
                this.packageVersion.Focus();
                return;
            }

            this.page2.Visibility = System.Windows.Visibility.Hidden;
            this.page3.Visibility = System.Windows.Visibility.Visible;
            this.packageTitle.Focus();
        }

        private void Page3_Next(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                this.Metadata.Title = this.packageTitle.Text.Length == 0 ? null : this.packageTitle.Text;
            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message, "Form validation error");
                this.packageTitle.Focus();
                return;
            }

            try
            {
                this.Metadata.Group = this.packageGroupName.Text.Length == 0 ? null : this.packageGroupName.Text;
            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message, "Form validation error");
                this.packageGroupName.Focus();
                return;
            }

            try
            {
                this.Metadata.Description = this.packageDescription.Text.Length == 0 ? null : this.packageDescription.Text;
            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message, "Form validation error");
                this.packageDescription.Focus();
                return;
            }

            this.confirmUniversalFeedURL.Content = this.universalFeedURL.Text;
            if (string.IsNullOrEmpty(this.universalFeedUser.Text))
            {
                this.confirmUniversalFeedUser.Visibility = System.Windows.Visibility.Hidden;
                this.confirmUniversalFeedUserAnonymous.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.confirmUniversalFeedUser.Content = this.universalFeedUser.Text;
                this.confirmUniversalFeedUser.Visibility = System.Windows.Visibility.Visible;
                this.confirmUniversalFeedUserAnonymous.Visibility = System.Windows.Visibility.Hidden;
            }
            this.confirmUniversalFeedPassword.Visibility = this.universalFeedPassword.SecurePassword.Length == 0 ? System.Windows.Visibility.Hidden : System.Windows.Visibility.Visible;
            this.confirmPackageName.Content = this.Metadata.Name;
            this.confirmPackageVersion.Content = this.Metadata.Version;
            this.confirmPackageDirectory.Content = this.packageDirectory.Text;
            this.confirmPackageTitle.Content = this.Metadata.Title ?? "";
            this.confirmPackageGroupName.Content = this.Metadata.Group ?? "";
            this.confirmPackageDescription.Content = this.Metadata.Description ?? "";

            this.page3.Visibility = System.Windows.Visibility.Hidden;
            this.page4.Visibility = System.Windows.Visibility.Visible;
        }

        private void Page4_Publish(object sender, System.Windows.RoutedEventArgs e)
        {
            this.page4.Visibility = System.Windows.Visibility.Hidden;
            this.page5.Visibility = System.Windows.Visibility.Visible;

            this.Closing += PreventClose;
            ThreadPool.QueueUserWorkItem(state =>
            {
                var args = (Tuple<UPackMetadata, string, string, int, string, string>)state;
                var metadata = args.Item1;
                var directoryName = args.Item2;
                var universalFeedURL = args.Item3;
                var authType = args.Item4;
                var username = args.Item5;
                var password = args.Item6;

                var buffer = new byte[8192];

                using (var stream = new MemoryStream())
                {
                    using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
                    {
                        using (var manifest = new JsonTextWriter(new StreamWriter(archive.CreateEntry("upack.json").Open())))
                        {
                            JsonSerializer.CreateDefault().Serialize(manifest, this.Metadata);
                        }

                        try
                        {
                            if (!this.TryBuildProject())
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    this.Closing -= PreventClose;
                                    this.status.Content = "Build failed! Check the build output window for more information.";
                                    this.progress.IsIndeterminate = false;
                                    this.progress.Value = this.progress.Maximum = 1;
                                    this.progress.Foreground = (Brush)this.Resources["ProgressErrorBrush"];
                                    this.page5Footer.Visibility = System.Windows.Visibility.Visible;
                                });
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                this.Closing -= PreventClose;
                                this.status.Content = "Build failed!\n\n" + ex.Message;
                                this.progress.IsIndeterminate = false;
                                this.progress.Value = this.progress.Maximum = 1;
                                this.progress.Foreground = (Brush)this.Resources["ProgressErrorBrush"];
                                this.page5Footer.Visibility = System.Windows.Visibility.Visible;
                            });
                            return;
                        }

                        try
                        {
                            Dispatcher.Invoke(() =>
                            {
                                this.status.Content = "Finding files...";
                            });

                            var directoryUri = new Uri(directoryName);
                            var fileNames = Directory.EnumerateFiles(directoryName, "*", SearchOption.AllDirectories).ToList();
                            long totalSize = 0;

                            foreach (var fileName in fileNames)
                            {
                                var fi = new FileInfo(fileName);
                                totalSize += fi.Length;
                            }

                            Dispatcher.Invoke(() =>
                            {
                                this.progress.IsIndeterminate = false;
                                this.progress.Value = 0;
                                this.progress.Maximum = totalSize;
                            });

                            int i = 0;
                            foreach (var fileName in fileNames)
                            {
                                i++;
                                var relativeFileName = "package/" + directoryUri.MakeRelativeUri(new Uri(fileName)).ToString();

                                Dispatcher.Invoke(() =>
                                {
                                    this.status.Content = string.Format("Packaging files ({0} / {1})...\n{2}", i, fileNames.Count, relativeFileName);
                                });

                                using (var input = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                                using (var output = archive.CreateEntry(relativeFileName).Open())
                                {
                                    while (true)
                                    {
                                        int n = input.Read(buffer, 0, buffer.Length);
                                        if (n == 0)
                                        {
                                            break;
                                        }

                                        output.Write(buffer, 0, n);

                                        Dispatcher.Invoke(() =>
                                        {
                                            this.progress.Value += n;
                                        });
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                this.Closing -= PreventClose;
                                this.status.Content = "Packaging the project failed!\n\n" + ex.Message;
                                this.progress.IsIndeterminate = false;
                                this.progress.Value = this.progress.Maximum = 1;
                                this.progress.Foreground = (Brush)this.Resources["ProgressErrorBrush"];
                                this.page5Footer.Visibility = System.Windows.Visibility.Visible;
                            });
                            return;
                        }
                    }

                    try
                    {
                        var request = WebRequest.CreateHttp(universalFeedURL + "/upload");
                        request.Method = HttpMethod.Put.Method;
                        request.ContentType = "application/zip";
                        switch (authType)
                        {
                            case 0:
                                request.UseDefaultCredentials = true;
                                break;
                            case 1:
                            case 2:
                                if (authType == 1)
                                {
                                    password = username;
                                    username = "api";
                                }
                                if (!string.IsNullOrEmpty(username))
                                {
                                    var credentialBytes = new byte[Encoding.UTF8.GetByteCount(username) + 1 + Encoding.UTF8.GetByteCount(password)];
                                    var index = Encoding.UTF8.GetBytes(username, 0, username.Length, credentialBytes, 0);
                                    index += Encoding.UTF8.GetBytes(":", 0, 1, credentialBytes, index);
                                    Encoding.UTF8.GetBytes(password, 0, password.Length, credentialBytes, index);
                                    request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + Convert.ToBase64String(credentialBytes));
                                }
                                break;
                        }
                        Dispatcher.Invoke(() =>
                        {
                            this.status.Content = string.Format("Pushing package to {0}...", universalFeedURL);
                            this.progress.Value = 0;
                            this.progress.Maximum = stream.Length;
                        });
                        stream.Position = 0;
                        using (var output = request.GetRequestStream())
                        {
                            while (true)
                            {
                                int n = stream.Read(buffer, 0, buffer.Length);
                                if (n == 0)
                                {
                                    break;
                                }

                                output.Write(buffer, 0, n);

                                Dispatcher.Invoke(() =>
                                {
                                    this.progress.Value += n;
                                });
                            }
                        }
                        Dispatcher.Invoke(() =>
                        {
                            this.status.Content = "Waiting for response...";
                            this.progress.IsIndeterminate = true;
                        });
                        try
                        {
                            using (var response = (HttpWebResponse)request.GetResponse())
                            {
                                if (response.StatusCode != HttpStatusCode.Created)
                                {
                                    using (var input = new StreamReader(response.GetResponseStream()))
                                    {
                                        throw new Exception(string.Format("{0} {1}\n\n{2}", (int)response.StatusCode, response.StatusDescription, input.ReadToEnd()));
                                    }
                                }
                            }
                        }
                        catch (WebException ex) when (ex.Status == WebExceptionStatus.ProtocolError)
                        {
                            var response = (HttpWebResponse)ex.Response;
                            using (var input = new StreamReader(response.GetResponseStream()))
                            {
                                throw new Exception(string.Format("{0} {1}\n\n{2}", (int)response.StatusCode, response.StatusDescription, input.ReadToEnd()));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            this.Closing -= PreventClose;
                            this.status.Content = "Upload failed!\n\n" + ex.Message;
                            this.progress.IsIndeterminate = false;
                            this.progress.Value = this.progress.Maximum = 1;
                            this.progress.Foreground = (Brush)this.Resources["ProgressErrorBrush"];
                            this.page5Footer.Visibility = System.Windows.Visibility.Visible;
                        });
                        return;
                    }
                }

                Dispatcher.Invoke(() =>
                {
                    this.Closing -= PreventClose;
                    this.status.Content = "Done!";
                    this.progress.IsIndeterminate = false;
                    this.progress.Value = this.progress.Maximum = 1;
                    this.page5Footer.Visibility = System.Windows.Visibility.Visible;
                });
            }, new Tuple<UPackMetadata, string, string, int, string, string>(this.Metadata, this.packageDirectory.Text, this.universalFeedURL.Text, this.universalFeedAuthType.SelectedIndex, this.universalFeedAuthType.SelectedIndex == 1 ? this.universalFeedApiKey.Text : this.universalFeedUser.Text, this.universalFeedPassword.Password));
        }

        private static void PreventClose(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
        }
        
        private bool TryBuildProject()
        {
            var project = ProjectCollection.GlobalProjectCollection.GetLoadedProjects(this.dteProject.FullName).FirstOrDefault();
            if (project != null)
            {
                IVsOutputWindowPane buildOutputWindow = null;
                Dispatcher.Invoke(() =>
                {
                    var shell = (IVsUIShell)this.ServiceProvider.GetService(typeof(SVsUIShell));
                    var hr = shell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fFindFirst, VSConstants.StandardToolWindows.Output, out IVsWindowFrame outputFrame);
                    ErrorHandler.ThrowOnFailure(hr);
                    outputFrame.ShowNoActivate();
                    var outputWindow = (IVsOutputWindow)this.ServiceProvider.GetService(typeof(SVsOutputWindow));
                    hr = outputWindow.GetPane(VSConstants.OutputWindowPaneGuid.BuildOutputPane_guid, out buildOutputWindow);
                    ErrorHandler.ThrowOnFailure(hr);
                    buildOutputWindow.Activate();
                });
                return project.Build(new OutputWindowLogger(buildOutputWindow));
            }
            return true;
        }

        private void Page2_Previous(object sender, System.Windows.RoutedEventArgs e)
        {
            this.page2.Visibility = System.Windows.Visibility.Hidden;
            this.page1.Visibility = System.Windows.Visibility.Visible;

            if (string.IsNullOrEmpty(this.universalFeedURL.Text))
            {
                this.universalFeedURL.Focus();
            }
            else if (string.IsNullOrEmpty(this.universalFeedUser.Text))
            {
                this.universalFeedUser.Focus();
            }
            else
            {
                this.universalFeedPassword.Focus();
            }
        }

        private void Page3_Previous(object sender, System.Windows.RoutedEventArgs e)
        {
            this.page3.Visibility = System.Windows.Visibility.Hidden;
            this.page2.Visibility = System.Windows.Visibility.Visible;
            this.packageName.Focus();
        }

        private void Page4_Previous(object sender, System.Windows.RoutedEventArgs e)
        {
            this.page4.Visibility = System.Windows.Visibility.Hidden;
            this.page3.Visibility = System.Windows.Visibility.Visible;
            this.packageTitle.Focus();
        }

        private void Page5_Close(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }

        private static readonly Regex UniversalFeedUrl = new Regex(@"^https?://[^/?#]+/upack/[^/?#]+/?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private void CheckUniversalFeedUrl(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.universalFeedURL.Text) || UniversalFeedUrl.IsMatch(this.universalFeedURL.Text))
            {
                this.warnNotUniversal.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                this.warnNotUniversal.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void AuthType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (this.universalFeedAuthType.SelectedIndex)
            {
                case 0:
                    this.apiKeyAuth.Visibility = System.Windows.Visibility.Collapsed;
                    this.userPasswordAuth.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case 1:
                    this.apiKeyAuth.Visibility = System.Windows.Visibility.Visible;
                    this.userPasswordAuth.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case 2:
                    this.apiKeyAuth.Visibility = System.Windows.Visibility.Collapsed;
                    this.userPasswordAuth.Visibility = System.Windows.Visibility.Visible;
                    break;
            }
        }
    }
}
