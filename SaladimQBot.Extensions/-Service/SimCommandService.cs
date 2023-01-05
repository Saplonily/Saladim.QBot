namespace SaladimQBot.Extensions;

public class SimCommandService
{
    public SimCommandExecuter Executor { get; protected set; }

    public SimCommandService(SimCommandConfig config)
    {
        var mi = config.ModuleInstancer;
        if (mi is null)
            Executor = new(config.RootPrefix);
        else
            Executor = new(config.RootPrefix, mi);
        foreach (var m in config.Modules)
        {
            Executor.AddModule(m);
        }
    }
}
