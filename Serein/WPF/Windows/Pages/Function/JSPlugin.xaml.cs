using Microsoft.Web.WebView2.Core;
using Serein.Base;
using Serein.Items;
using Serein.JSPlugin;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Serein.Windows.Pages.Function
{
    public partial class JSPlugin : UiPage, IDisposable
    {
        public JSPlugin()
        {
            InitializeComponent();
            ResourcesManager.InitConsole();
            Catalog.Function.JSPlugin?.Dispose();
            Load();
            PluginWebBrowser.EnsureCoreWebView2Async(CoreWebView2Environment.CreateAsync(userDataFolder: IO.GetPath("cache", "WebViewData")).GetAwaiter().GetResult());
            Catalog.Function.JSPlugin = this;
        }

        public void Dispose()
        {
            Dispatcher.Invoke(() =>
            {
                PluginWebBrowser.Stop();
                PluginWebBrowser.Dispose();
            });
        }

        private void PluginWebBrowser_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            Logger.Out(LogType.Debug, string.Join(";", Catalog.Function.PluginCache));
            Catalog.Function.PluginCache.ForEach((Text) => AppendText(Text));
        }

        private void PluginWebBrowser_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                PluginWebBrowser.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
                if (!Global.Settings.Serein.DevelopmentTool.EnableDebug)
                {
                    PluginWebBrowser.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
                    PluginWebBrowser.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                    PluginWebBrowser.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                    PluginWebBrowser.CoreWebView2.Settings.AreDevToolsEnabled = false;
                    PluginWebBrowser.CoreWebView2.Settings.AreHostObjectsAllowed = false;
                    PluginWebBrowser.CoreWebView2.Settings.IsSwipeNavigationEnabled = false;
                    PluginWebBrowser.CoreWebView2.Settings.IsStatusBarEnabled = false;
                }
                PluginWebBrowser.CoreWebView2.Navigate(@"file:\\\" + Global.Path + $"console\\console.html?type=plugin&theme={(Theme.GetAppTheme() == ThemeType.Light ? "light" : "dark")}");
            }
            else
            {
                Logger.Out(LogType.Debug, e.InitializationException);
                Logger.MsgBox("控制台加载失败\n" + e.InitializationException.Message, "Serein", 0, 48);
            }
        }

        public void AppendText(string Line)
            => Dispatcher.Invoke(() => PluginWebBrowser.CoreWebView2.ExecuteScriptAsync($"AppendText(\"{Line.Replace(@"\", "\\").Replace("\"", "\\\"")}\")"));

        private void Load()
        {
            JSPluginListView.Items.Clear();
            foreach (Plugin Item in JSPluginManager.PluginDict.Values)
            {
                ListViewItem listViewItem = new ListViewItem
                {
                    Content = Item,
                    Tag = Item.Namespace,
                    Opacity = Item.Available ? 1 : 0.5
                };
                JSPluginListView.Items.Add(listViewItem);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Wpf.Ui.Controls.MenuItem Item && Item != null)
            {
                switch (Item.Tag)
                {
                    case "Reload":
                        JSPluginManager.Reload();
                        Load();
                        break;
                    case "ClearConsole":
                        AppendText("#clear");
                        Catalog.Function.PluginCache.Clear();
                        break;
                    case "LookupDocs":
                        Process.Start(new ProcessStartInfo("https://serein.cc/#/Function/JSDocs") { UseShellExecute = true });
                        break;
                    case "Disable":
                        if (JSPluginListView.SelectedItem is ListViewItem item && JSPluginManager.PluginDict.ContainsKey(item.Tag.ToString()))
                        {
                            JSPluginManager.PluginDict[item.Tag.ToString()].Dispose();
                            Load();
                        }
                        break;
                }
            }
        }

        private void JSPluginListView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
            => Disable.IsEnabled =
            JSPluginListView.SelectedIndex != -1 &&
            JSPluginListView.SelectedItem is ListViewItem item &&
            JSPluginManager.PluginDict.ContainsKey(item.Tag.ToString()) &&
            JSPluginManager.PluginDict[item.Tag.ToString()].Available;

    }
}
