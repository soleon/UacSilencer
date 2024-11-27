using System.ServiceProcess;
using UacSilencer;

if (Environment.UserInteractive)
{
    // Running as a console app.
    Silencer.Start();
    await Task.Delay(-1).ConfigureAwait(false);
}
else
{
    // Running as a service.
    ServiceBase.Run(new SilencerService());
}