using Microsoft.Web.WebView2.Core;
using Serein.Base;
using Serein.Items;
using Serein.Server;
using System;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Serein.Windows.Pages.Server
{
    public partial class Panel : UiPage, IDisposable
    {
        public Panel()
        {
            InitializeComponent();
            ResourcesManager.InitConsole();
            Catalog.Server.Panel?.Dispose();
            Timer UpdateInfoTimer = new Timer(2000) { AutoReset = true };
            UpdateInfoTimer.Elapsed += (sender, e) => UpdateInfos();
            UpdateInfoTimer.Start();
            PanelWebBrowser.EnsureCoreWebView2Async(CoreWebView2Environment.CreateAsync(userDataFolder: IO.GetPath("cache", "WebViewData")).GetAwaiter().GetResult());
            Catalog.Server.Panel = this;
        }

        public void Dispose()
        {
            Dispatcher.Invoke(() =>
            {
                PanelWebBrowser.Stop();
                PanelWebBrowser.Dispose();
            });
        }

        public void AppendText(string Line)
            => Dispatcher.Invoke(() =>
                PanelWebBrowser.CoreWebView2.ExecuteScriptAsync($"AppendText(\"{Line.Replace(@"\", "\\").Replace("\"", "\\\"")}\")"));

        private void PanelWebBrowser_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            Logger.Out(LogType.Debug, string.Join(";", Catalog.Server.Cache));
            Catalog.Server.Cache.ForEach((Text) => AppendText(Text));
        }

        private void PanelWebBrowser_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                PanelWebBrowser.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
                if (!Global.Settings.Serein.DevelopmentTool.EnableDebug)
                {
                    PanelWebBrowser.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
                    PanelWebBrowser.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                    PanelWebBrowser.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                    PanelWebBrowser.CoreWebView2.Settings.AreDevToolsEnabled = false;
                    PanelWebBrowser.CoreWebView2.Settings.AreHostObjectsAllowed = false;
                    PanelWebBrowser.CoreWebView2.Settings.IsSwipeNavigationEnabled = false;
                    PanelWebBrowser.CoreWebView2.Settings.IsStatusBarEnabled = false;
                }
                PanelWebBrowser.CoreWebView2.Navigate(@"file:\\\" + Global.Path + $"console\\console.html?type=panel&theme={(Theme.GetAppTheme() == ThemeType.Light ? "light" : "dark")}");
            }
            else
            {
                Logger.Out(LogType.Debug, e.InitializationException);
                Logger.MsgBox("控制台加载失败\n" + e.InitializationException.Message, "Serein", 0, 48);
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
            => ServerManager.Start();

        private void Stop_Click(object sender, RoutedEventArgs e)
            => ServerManager.Stop();

        private void Restart_Click(object sender, RoutedEventArgs e)
            => ServerManager.RestartRequest();

        private void Kill_Click(object sender, RoutedEventArgs e)
            => ServerManager.Kill();

        private void Enter_Click(object sender, RoutedEventArgs e)
        {
            ServerManager.InputCommand(InputBox.Text);
            InputBox.Text = "";
        }

        private void InputBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    ServerManager.InputCommand(InputBox.Text);
                    InputBox.Text = "";
                    e.Handled = true;
                    break;
                case Key.Up:
                case Key.PageUp:
                    if (ServerManager.CommandHistoryIndex > 0)
                    {
                        ServerManager.CommandHistoryIndex--;
                    }
                    if (ServerManager.CommandHistoryIndex >= 0 && ServerManager.CommandHistoryIndex < ServerManager.CommandHistory.Count)
                    {
                        InputBox.Text = ServerManager.CommandHistory[ServerManager.CommandHistoryIndex];
                    }
                    InputBox.SelectionStart = InputBox.Text.Length;
                    break;
                case Key.Down:
                case Key.PageDown:
                    if (ServerManager.CommandHistoryIndex < ServerManager.CommandHistory.Count)
                    {
                        ServerManager.CommandHistoryIndex++;
                    }
                    if (ServerManager.CommandHistoryIndex >= 0 && ServerManager.CommandHistoryIndex < ServerManager.CommandHistory.Count)
                    {
                        InputBox.Text = ServerManager.CommandHistory[ServerManager.CommandHistoryIndex];
                    }
                    else if (ServerManager.CommandHistoryIndex == ServerManager.CommandHistory.Count && ServerManager.CommandHistory.Count != 0)
                    {
                        InputBox.Text = string.Empty;
                    }
                    InputBox.SelectionStart = InputBox.Text.Length;
                    break;
            }
        }

        private void UpdateInfos()
            => Dispatcher.Invoke(() =>
            {
                Status.Content = ServerManager.Status ? "已启动" : "未启动";
                Version.Content = ServerManager.Status ? ServerManager.Version : "-";
                Version.Content = ServerManager.Status ? ServerManager.Version : "-";
                if (ServerManager.Finished)
                {
                    Version.Content = ServerManager.Status ? ServerManager.Version : "-";
                    Difficulity.Content = ServerManager.Status ? ServerManager.Difficulty : "-";
                    Level.Content = ServerManager.Status ? ServerManager.LevelName : "-";
                }
                Time.Content = ServerManager.Status ? ServerManager.GetTime() : "-";
                CPUPerc.Content = ServerManager.Status ? "%" + ServerManager.CPUUsage.ToString("N2") : "-";
                Catalog.MainWindow.UpdateTitle(ServerManager.Status ? ServerManager.StartFileName : null);
            });
    }
}
