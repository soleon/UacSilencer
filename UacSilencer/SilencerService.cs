using System.ServiceProcess;
using UacSilencer;

internal class SilencerService : ServiceBase
{
    public SilencerService()
    {
        ServiceName = "UAC Silencer";
    }

    protected override void OnStart(string[] args)
    {
        Silencer.Start();
    }

    protected override void OnStop()
    {
        Silencer.Stop();
    }
}