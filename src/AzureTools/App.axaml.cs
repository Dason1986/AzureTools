using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using AzureTools.Models;
using AzureTools.Services;
using AzureTools.Stores;
using AzureTools.ViewModels;
using AzureTools.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace AzureTools
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        internal static void InitializeServices(IServiceProvider services)
        {
            Services = services.CreateScope().ServiceProvider;
        }

        public App()
        {


        }

        #region  异常 
        private void OnUiThreadException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            Serilog.Log.Fatal(ex, "UI线程未捕获异常");

            e.Handled = true;

            _ = ShowErrorDialog("程序发生异常", ex.Message, ex.ToString());
        }
        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            AggregateException ex = e.Exception;
            Serilog.Log.Fatal(ex, "后台Task未观测异常");

            Dispatcher.UIThread.Post(async () =>
            {
                await ShowErrorDialog("后台任务异常", ex.Message, ex.ToString());
            });
        }
        private void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;

            if (!e.IsTerminating)
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    await ShowErrorDialog("后台线程异常", ex?.Message ?? "未知错误", ex?.StackTrace.ToString());
                });
            }
            // IsTerminating=true 时进程马上结束，弹窗大概率弹不出来，只日志
        }
        private async Task ShowErrorDialog(string title, string msg, string stack)
        {
            Window mainWin = ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lt
                ? lt.MainWindow
                : null;

            if (mainWin == null) return;
            IDialogService dialog = Services.GetRequiredService<IDialogService>();
            await dialog.ShowInfoAsync(msg + "\r\n" + stack, title);
        }

        #endregion

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }



        public override void OnFrameworkInitializationCompleted()
        {
            Dispatcher.UIThread.UnhandledException += OnUiThreadException;


            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;


            AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;



            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                IAzureToolsStore store = Services.GetRequiredService<IAzureToolsStore>();
                AppProfile profile = store.GetProfile();
                IThemeService themeService = Services.GetRequiredService<IThemeService>();
                themeService.ApplyTheme(profile?.ThemeMode ?? ThemeMode.System);
                if (profile == null)
                {

                    AppWizardView wizardView = Services.GetRequiredService<Views.AppWizardView>();
                    AppWizardViewModel wizardVm = (AppWizardViewModel)wizardView.DataContext;
                    bool completed = false;

                    wizardVm.Completed += (s, e) => completed = true;
                    wizardView.Closed += (s, e) =>
                    {

                        if (!completed || store.GetProfile() == null)
                        {
                            desktop.MainWindow = null;
                            return;
                        }
                        ShowMainWindow(desktop);
                    };

                    desktop.MainWindow = wizardView;
                    return;
                }

                if (profile.LoginUsePassword)
                {
                    LoginView loginView = Services.GetRequiredService<Views.LoginView>();
                    LoginViewModel loginVm = (LoginViewModel)loginView.DataContext;

                    loginView.Closed += (s, e) =>
                    {

                        if (string.IsNullOrEmpty(loginVm.PasswordHash))
                        {
                            desktop.MainWindow = null;
                            return;
                        }
                        ShowMainWindow(desktop);
                    };

                    desktop.MainWindow = loginView;
                    return;
                }

                ShowMainWindow(desktop);
            }

            base.OnFrameworkInitializationCompleted();
        }

        private static void ShowMainWindow(IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Services.GetRequiredService<Views.MainView>();
            desktop.MainWindow.Show();
        }
    }
}