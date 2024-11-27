using System.Management;
using Microsoft.Win32;
using NLog;
using NLog.Layouts;

namespace UacSilencer;

internal class Silencer
{
    private const string ConsentPromptBehaviorAdminValueName = "ConsentPromptBehaviorAdmin";
    private const string SystemPoliciesKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly ManagementEventWatcher Watcher;

    static Silencer()
    {
        Watcher = new ManagementEventWatcher(new WqlEventQuery(
            "SELECT * FROM RegistryValueChangeEvent " +
            "WHERE Hive = 'HKEY_LOCAL_MACHINE' " +
            $"AND KeyPath = '{SystemPoliciesKeyPath.Replace(@"\", @"\\")}' " +
            $"AND ValueName = '{ConsentPromptBehaviorAdminValueName}'"));
        
        LogManager.Setup().LoadConfiguration(builder =>
            builder.ForLogger().WriteToFile($"log {DateTime.Now.ToString("s").Replace(':', '-')}.txt",
                Layout.FromString("${longdate} ${level:uppercase=true}: ${message:withexception=true}")));
        
        Watcher.EventArrived += (_, _) => TrySetRegistryKeyValue();
    }

    internal static void Start()
    {
        TrySetRegistryKeyValue();
        Watcher.Start();
    }

    internal static void Stop()
    {
        Watcher.Stop();
        Watcher.Dispose();
    }

    private static void TrySetRegistryKeyValue()
    {
        Logger.Info("Try setting registry key value.");
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(SystemPoliciesKeyPath, true);
            if (key == null) return;
            var value = key.GetValue(ConsentPromptBehaviorAdminValueName);
            if (0.Equals(value)) return;
            key.SetValue(ConsentPromptBehaviorAdminValueName, 0, RegistryValueKind.DWord);
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
    }
}