using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CP77Tools.Commands;
using WolvenKit.Common.Tools.Oodle;

namespace WolvenKit.CLI
{
    internal class Program
    {

        [STAThread]
        public static void Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                new ArchiveCommand(),

                new UnbundleCommand(),
                new UncookCommand(),
                new ImportCommand(),
                new PackCommand(),
                new ExportCommand(),

                new DumpCommand(),
                new CR2WCommand(),

                new HashCommand(),
                new OodleCommand(),

                new TweakCommand(),

                new SettingsCommand(),
            };

            var parser = new CommandLineBuilder(rootCommand)
                .UseDefaults()
                .UseHost(GenericHost.CreateHostBuilder)
                .Build();

            parser.Invoke(args);
        }

        private delegate void StrDelegate(string value);

        private static string TryGetGameInstallDir()
        {
            var cp77BinDir = "";

#pragma warning disable CA1416
#if _WINDOWS
            var cp77exe = "";
            // check for CP77_DIR environment variable first
            var CP77_DIR = System.Environment.GetEnvironmentVariable("CP77_DIR", EnvironmentVariableTarget.User);
            if (!string.IsNullOrEmpty(CP77_DIR) && new DirectoryInfo(CP77_DIR).Exists)
            {
                cp77BinDir = Path.Combine(CP77_DIR, "bin", "x64");
            }

            if (File.Exists(Path.Combine(cp77BinDir, "Cyberpunk2077.exe")))
            {
                return cp77BinDir;
            }

            // else: look for install location
            const string uninstallkey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\";
            const string uninstallkey2 = "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\";
            const string gameName = "Cyberpunk 2077";
            const string exeName = "Cyberpunk2077.exe";
            var exePath = "";
            StrDelegate strDelegate = msg => cp77exe = msg;

            try
            {
                Parallel.ForEach(Microsoft.Win32.Registry.LocalMachine.OpenSubKey(uninstallkey)?.GetSubKeyNames(), item =>
                {
                    var programName = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(uninstallkey + item)
                        ?.GetValue("DisplayName");
                    var installLocation = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(uninstallkey + item)
                        ?.GetValue("InstallLocation");
                    if (programName != null && installLocation != null)
                    {
                        if (programName.ToString().Contains(gameName) ||
                            programName.ToString().Contains(gameName))
                        {
                            exePath = Directory.GetFiles(installLocation.ToString(), exeName,
                                SearchOption.AllDirectories).First();
                        }
                    }

                    strDelegate.Invoke(exePath);
                });
                Parallel.ForEach(Microsoft.Win32.Registry.LocalMachine.OpenSubKey(uninstallkey2)?.GetSubKeyNames(), item =>
                {
                    var programName = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(uninstallkey2 + item)
                        ?.GetValue("DisplayName");
                    var installLocation = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(uninstallkey2 + item)
                        ?.GetValue("InstallLocation");
                    if (programName != null && installLocation != null)
                    {
                        if (programName.ToString().Contains(gameName) ||
                            programName.ToString().Contains(gameName))
                        {
                            if (Directory.Exists(installLocation.ToString()))
                            {
                                exePath = Directory.GetFiles(installLocation.ToString(), exeName,
                                    SearchOption.AllDirectories).First();
                            }
                        }
                    }

                    strDelegate.Invoke(exePath);
                });

                if (File.Exists(cp77exe))
                {
                    cp77BinDir = new FileInfo(cp77exe).Directory.FullName;
                }
            }
            catch (Exception)
            {
                return null;
            }

            if (string.IsNullOrEmpty(cp77BinDir))
            {
                return null;
            }

            if (!File.Exists(Path.Combine(cp77BinDir, "Cyberpunk2077.exe")))
            {
                return null;
            }
#endif
#pragma warning restore CA1416

            return cp77BinDir;
        }
    }
}
