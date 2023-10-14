using GrpcDotNetNamedPipes;
using Lively.Common;
using Lively.Core;
using Lively.Core.Display;
using Lively.Core.Suspend;
using Lively.Core.Watchdog;
using Lively.Factories;
using Lively.Grpc.Common.Proto.Desktop;
using Lively.RPC;
using Lively.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using Lively.Grpc.Common.Proto.Settings;
using System.Threading.Tasks;
using Lively.Grpc.Common.Proto.Display;
using Lively.Grpc.Common.Proto.Commands;
using System.Linq;
using Lively.Automation;
using Lively.Views.WindowMsg;
using Lively.Common.Helpers.Network;
using System.Windows.Threading;
using Lively.Views;
using Lively.Grpc.Common.Proto.Update;
using Lively.Common.Helpers.Files;
using Lively.Common.Helpers.Archive;
using Lively.Models;
using Lively.Common.Helpers;
using Lively.Helpers.Theme;
using Microsoft.Win32;
using Lively.Common.Services;
using Lively.Common.Models;
using NotionGadgetsServer;
using NotionGadgetsServer.Models;
using NotionGadgetsServer.Settings;

namespace Lively
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly Mutex mutex = new Mutex(false, Constants.SingleInstance.UniqueAppName);
        private readonly NamedPipeServer grpcServer;
        public static Task ServerThread;
        public static CancellationTokenSource CancelTokenSource = new CancellationTokenSource();
        public static CancellationToken CancelToken = CancelTokenSource.Token;

        private readonly IServiceProvider _serviceProvider;
        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance for the current application instance.
        /// </summary>
        public static IServiceProvider Services
        {
            get
            {
                IServiceProvider serviceProvider = ((App)Current)._serviceProvider;
                return serviceProvider ?? throw new InvalidOperationException("The service provider is not initialized");
            }
        }

        public static void GlobalHandler(ThreadStart threadStartTarget)
        {
            // handles and logs all exceptions from the Server thread
            try
            {
                threadStartTarget.Invoke();
            }
            catch (Exception ex)
            {
                //handle exception here
                Logger.Error("NotionGadgets Server Error: " + ex.Message);
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // set API key and Page link if they exist
            NotionSettings notionSettings = SettingsSaver.LoadSettings();
            if (notionSettings != null)
            {
                try
                {
                    ServerThread = new Task(() => GlobalHandler(new ThreadStart(() => Program.StartServer(notionSettings.NotionAPISecret))), CancelToken);
                    ServerThread.Start();
                }

                catch (Exception ex)
                {
                    Logger.Error("Failed to start server thread, got: " + ex.Message);
                }
            }
        }

        public App()
        {
            // copy the cached image to the NotionClient's folder

            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("AppEvents\\Schemes\\Apps\\.Default\\.Default\\.Current", true);

                if (key.GetValueNames().Length > 0)
                {
                    key.DeleteValue(key.GetValueNames()[0]);
                }

            }

            catch (Exception ex)
            {
                Logger.Error("Failed to delete registry value, got: " + ex.Message);
            }

            // cached image path
            bool SetCachedImage = false;
            string gadgetsPath = "";


            try
            {

                var cachedPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft\\Windows\\Themes\\CachedFiles");

                if (Directory.Exists(cachedPath))
                {
                    try
                    {
                        var cachedFiles = Directory.GetFiles(cachedPath);
                        gadgetsPath = Path.Combine(Constants.CommonPaths.AppDataDir, "Library\\wallpapers\\NotionGadgetsClient\\textures");

                        if (cachedFiles.Length > 0)
                        {
                            // copy the cached file
                            var cachedFileName = Path.GetFileName(cachedFiles[0]);
                            var destDir = Path.Combine(gadgetsPath, "default.jpg");

                            if (Directory.Exists(gadgetsPath))
                            {
                                File.Copy(cachedFiles[0], destDir, true);
                                SetCachedImage = true;
                                Logger.Info("[Copy Cached Image] Successfully copied " + cachedFiles[0] + " to " + destDir);
                            }

                            else
                            {
                                Logger.Warn("[Copy Cached Image] NotionGadgetsClient folder did not exist. Nowhere to copy Cached Images to.");
                            }
                        }

                        else
                        {
                            Logger.Warn("[Copy Cached Image] No cached images to copy in directory: " + cachedPath);
                        }
                    }

                    catch (Exception ex)
                    {
                        // failed to do the copy or get files in the directory
                        Logger.Error("[Copy Cached Image] Got error: " + ex.Message);
                    }
                }

                else
                {
                    Logger.Warn("[Copy Cached Image] Directory did not exist: " + cachedPath);
                }
            }

            catch (Exception ex)
            {
                Logger.Error("[Copy Cached Image] Failed Misc Error: " + ex.Message);
            }


            var copiedFilePath = Path.Combine(gadgetsPath, "default.jpg");


            // copey cached failed and default.jpg doesnt already exist from previous app run

            try
            {
                if (!SetCachedImage && !File.Exists(copiedFilePath))
                {
                    var NotionDefaultImageName = "notion-default.jpg";
                    var CopyImageName = "default.jpg";

                    var NotionFilePath = Path.Combine(Constants.CommonPaths.AppDataDir, "Library\\wallpapers\\NotionGadgetsClient\\textures");
                    var NotionFinalPath = Path.Combine(NotionFilePath, NotionDefaultImageName);

                    var CopyFilePath = Path.Combine(NotionFilePath, CopyImageName);
                    if (File.Exists(NotionFinalPath))
                    {
                        File.Move(NotionFinalPath, CopyFilePath);
                    }
                }
            }

            catch (Exception ex)
            {
                Logger.Warn("Failed to copy default Notion image: " + ex.Message);
            }

            try
            {
                //wait a few seconds in case application instance is just shutting down..
                if (!mutex.WaitOne(TimeSpan.FromSeconds(1), false))
                {
                    try
                    {
                        //skipping first element (application path.)
                        var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
                        var client = new CommandsService.CommandsServiceClient(new NamedPipeChannel(".", Constants.SingleInstance.GrpcPipeServerName));
                        var request = new AutomationCommandRequest();
                        request.Args.AddRange(args.Length != 0 ? args : new string[] { "--showApp", "true" });
                        _ = client.AutomationCommandAsync(request);
                    }
                    catch (Exception e)
                    {
                        _ = MessageBox.Show($"Failed to communicate with Core:\n{e.Message}", "Lively Wallpaper");
                    }
                    ShutDown();
                    return;
                }
            }
            catch (AbandonedMutexException e)
            {
                //unexpected app termination.
                Debug.WriteLine(e.Message);
            }

            SetupUnhandledExceptionLogging();
            Logger.Info(LogUtil.GetHardwareInfo());

            //App() -> OnStartup() -> App.Startup event.
            _serviceProvider = ConfigureServices();
            grpcServer = ConfigureGrpcServer();

            try
            {
                //clear temp files from previous run if any..
                FileOperations.EmptyDirectory(Constants.CommonPaths.TempDir);
                FileOperations.EmptyDirectory(Constants.CommonPaths.ThemeCacheDir);
            }
            catch { /* TODO */ }

            try
            {
                //create directories if not exist, eg: C:\Users\<User>\AppData\Local
                Directory.CreateDirectory(Constants.CommonPaths.AppDataDir);
                Directory.CreateDirectory(Constants.CommonPaths.LogDir);
                Directory.CreateDirectory(Constants.CommonPaths.ThemeDir);
                Directory.CreateDirectory(Constants.CommonPaths.TempDir);
                Directory.CreateDirectory(Constants.CommonPaths.TempCefDir);
                Directory.CreateDirectory(Constants.CommonPaths.TempVideoDir);
                Directory.CreateDirectory(Constants.CommonPaths.ThemeCacheDir);
            }
            catch (Exception ex)
            {
                //nothing much can be done here..
                MessageBox.Show(ex.Message, "AppData directory creation failed, exiting Lively..", MessageBoxButton.OK, MessageBoxImage.Error);
                ShutDown();
                return;
            }

            try
            {
                //default livelyproperty for media files..
                var mediaProperty = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "mpv", "api", "LivelyProperties.json");
                if (File.Exists(mediaProperty))
                {
                    File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "mpv", "api", "LivelyProperties.json"),
                        Path.Combine(Constants.CommonPaths.TempVideoDir, "LivelyProperties.json"), true);
                }
            }
            catch { /* TODO */ }

            var userSettings = Services.GetRequiredService<IUserSettingsService>();
            try
            {
                CreateWallpaperDir(userSettings.Settings.WallpaperDir);
            }
            catch (Exception ex)
            {
                Logger.Error($"Wallpaper directory setup failed: {ex.Message}, falling back to default.");
                userSettings.Settings.WallpaperDir = Path.Combine(Constants.CommonPaths.AppDataDir, "Library");
                CreateWallpaperDir(userSettings.Settings.WallpaperDir);
                userSettings.Save<ISettingsModel>();
            }

            Services.GetRequiredService<WndProcMsgWindow>().Show();
            Services.GetRequiredService<RawInputMsgWindow>().Show();
            Services.GetRequiredService<IPlayback>().Start();
            Services.GetRequiredService<ISystray>();

            //Install any new asset collection if present, do this before restoring wallpaper incase wallpaper is updated.
            //On first run default assets are installed by UI to avoid slow startup times and better user experience.
            if (userSettings.Settings.IsUpdated || userSettings.Settings.IsFirstRun)
            {
                SplashWindow spl = userSettings.Settings.IsFirstRun ? new(0, 500) : null; spl?.Show();
                var maxWallpaper = ZipExtract.ExtractAssetBundle(userSettings.Settings.WallpaperBundleVersion,
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bundle", "wallpapers"),
                    Path.Combine(userSettings.Settings.WallpaperDir, Constants.CommonPartialPaths.WallpaperInstallDir));
                var maxTheme = ZipExtract.ExtractAssetBundle(userSettings.Settings.ThemeBundleVersion,
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bundle", "themes"),
                    Path.Combine(Constants.CommonPaths.ThemeDir));
                if (maxTheme != userSettings.Settings.ThemeBundleVersion || maxWallpaper != userSettings.Settings.WallpaperBundleVersion)
                {
                    userSettings.Settings.WallpaperBundleVersion = maxWallpaper;
                    userSettings.Settings.ThemeBundleVersion = maxTheme;
                    userSettings.Save<ISettingsModel>();
                }
                spl?.Close();
            }

            //restore wallpaper(s) from previous run.
            Services.GetRequiredService<IDesktopCore>().RestoreWallpaper();

            //first run Setup-Wizard show..
            if (userSettings.Settings.IsFirstRun)
            {
                Services.GetRequiredService<IRunnerService>().ShowUI();
            }

            //need to load theme later stage of startu to update..
            this.Startup += (s, e) => {
                ChangeTheme(userSettings.Settings.ApplicationTheme);
            };

            //Ref: https://github.com/Kinnara/ModernWpf/blob/master/ModernWpf/Helpers/ColorsHelper.cs
            SystemEvents.UserPreferenceChanged += (s, e) => {
                if (e.Category == UserPreferenceCategory.General)
                {
                    if (userSettings.Settings.ApplicationTheme == Common.AppTheme.Auto)
                    {
                        ChangeTheme(Common.AppTheme.Auto);
                    }
                }
            };

            this.SessionEnding += (s, e) => {
                if (e.ReasonSessionEnding == ReasonSessionEnding.Shutdown || e.ReasonSessionEnding == ReasonSessionEnding.Logoff)
                {
                    e.Cancel = true;
                    ShutDown();
                }
            };

#if DEBUG != true
            var appUpdater = Services.GetRequiredService<IAppUpdaterService>();
            appUpdater.UpdateChecked += AppUpdateChecked;
            _ = appUpdater.CheckUpdate();
            appUpdater.Start();
#endif
            Debug.WriteLine("App Update checking disabled in DEBUG mode.");
        }

        private IServiceProvider ConfigureServices()
        {
            //TODO: Logger abstraction.
            var provider = new ServiceCollection()
                //singleton
                .AddSingleton<IUserSettingsService, JsonUserSettingsService>()
                .AddSingleton<IDesktopCore, WinDesktopCore>()
                .AddSingleton<IWatchdogService, WatchdogProcess>()
                .AddSingleton<IDisplayManager, DisplayManager>()
                .AddSingleton<IScreensaverService, ScreensaverService>()
                .AddSingleton<IPlayback, Playback>()
                .AddSingleton<IRunnerService, RunnerService>()
                .AddSingleton<ISystray, Systray>()
                .AddSingleton<IAppUpdaterService, GithubUpdaterService>()
                .AddSingleton<ITransparentTbService, TranslucentTBService>()
                .AddSingleton<RawInputMsgWindow>()
                .AddSingleton<WndProcMsgWindow>()
                .AddSingleton<WinDesktopCoreServer>()
                .AddSingleton<DisplayManagerServer>()
                .AddSingleton<UserSettingsServer>()
                .AddSingleton<CommandsServer>()
                .AddSingleton<AppUpdateServer>()
                //transient
                //.AddTransient<IApplicationsRulesFactory, ApplicationsRulesFactory>()
                .AddTransient<IWallpaperFactory, WallpaperFactory>()
                .AddTransient<ILivelyPropertyFactory, LivelyPropertyFactory>()
                //.AddTransient<IScreenRecorder, ScreenRecorderlibScreen>()
                .AddTransient<ICommandHandler, CommandHandler>()
                .AddTransient<IDownloadHelper, MultiDownloadHelper>()
                //.AddTransient<SetupView>()
                /*
                .AddLogging(loggingBuilder =>
                {
                    // configure Logging with
                NLog
                    loggingBuilder.ClearProviders();
                    loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    loggingBuilder.AddNLog("Nlog.config");
                })
                */
                .BuildServiceProvider();

            return provider;
        }

        private NamedPipeServer ConfigureGrpcServer()
        {
            var server = new NamedPipeServer(Constants.SingleInstance.GrpcPipeServerName);
            DesktopService.BindService(server.ServiceBinder, Services.GetRequiredService<WinDesktopCoreServer>());
            SettingsService.BindService(server.ServiceBinder, Services.GetRequiredService<UserSettingsServer>());
            DisplayService.BindService(server.ServiceBinder, Services.GetRequiredService<DisplayManagerServer>());
            CommandsService.BindService(server.ServiceBinder, Services.GetRequiredService<CommandsServer>());
            UpdateService.BindService(server.ServiceBinder, Services.GetRequiredService<AppUpdateServer>());
            server.Start();

            return server;
        }

        /// <summary>
        /// Actual apptheme, no Auto allowed.
        /// </summary>
        private static Common.AppTheme _currentTheme = Common.AppTheme.Dark;
        public static void ChangeTheme(Common.AppTheme theme)
        {
            theme = theme == Common.AppTheme.Auto ? ThemeUtil.GetWindowsTheme() : theme;
            if (_currentTheme == theme)
                return;

            _currentTheme = theme;
            Uri uri = theme switch
            {
                Common.AppTheme.Light => new Uri("Themes/Light.xaml", UriKind.Relative),
                Common.AppTheme.Dark => new Uri("Themes/Dark.xaml", UriKind.Relative),
                _ => new Uri("Themes/Dark.xaml", UriKind.Relative)
            };

            try
            {
                ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(resourceDict);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            Logger.Info($"Theme changed: {theme}");
        }

        //number of times to notify user about update.
        private static int updateNotifyAmt = 1;
        private static bool updateNotify = false;
        private void AppUpdateChecked(object sender, AppUpdaterEventArgs e)
        {
            var sysTray = Services.GetRequiredService<ISystray>();
            _ = Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                if (e.UpdateStatus == AppUpdateStatus.available)
                {
                    if (updateNotifyAmt > 0)
                    {
                        updateNotifyAmt--;
                        updateNotify = true;
                        sysTray?.ShowBalloonNotification(4000,
                            "Lively Wallpaper",
                            Lively.Properties.Resources.TextUpdateAvailable);
                    }

                    //If UI program already running then notification is displayed withing the it.
                    if (!Services.GetRequiredService<IRunnerService>().IsVisibleUI && updateNotify)
                    {
                        AppUpdateDialog(e.UpdateUri, e.ChangeLog);
                    }
                }
                Logger.Info($"AppUpdate status: {e.UpdateStatus}");
            }));
        }

        private static AppUpdater updateWindow;
        public static void AppUpdateDialog(Uri uri, string changelog)
        {
            updateNotify = false;
            if (updateWindow == null)
            {
                updateWindow = new AppUpdater(uri, changelog)
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                updateWindow.Closed += (s, e) => { updateWindow = null; };
                updateWindow.Show();
            }
        }

        private void CreateWallpaperDir(string baseDirectory)
        {
            Directory.CreateDirectory(Path.Combine(baseDirectory, Constants.CommonPartialPaths.WallpaperInstallDir));
            Directory.CreateDirectory(Path.Combine(baseDirectory, Constants.CommonPartialPaths.WallpaperInstallTempDir));
            Directory.CreateDirectory(Path.Combine(baseDirectory, Constants.CommonPartialPaths.WallpaperSettingsDir));
        }

        private void SetupUnhandledExceptionLogging()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            Dispatcher.UnhandledException += (s, e) =>
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");

            //ref: https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskscheduler.unobservedtaskexception?redirectedfrom=MSDN&view=net-6.0
            TaskScheduler.UnobservedTaskException += (s, e) => {
                //LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
            };
        }

        private void LogUnhandledException(Exception exception, string source) => Logger.Error(exception);

        public static void ShutDown()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.ResetColor();
            CancelTokenSource.Cancel();
            CancelTokenSource.Dispose();

            try
            {
                ((ServiceProvider)App.Services)?.Dispose();
            }
            catch (InvalidOperationException) { /* not initialised */ }
            ((App)Current).grpcServer?.Dispose();
            //Shutdown needs to be called from dispatcher..
            Application.Current.Dispatcher.Invoke(Application.Current.Shutdown);
        }
    }
}
