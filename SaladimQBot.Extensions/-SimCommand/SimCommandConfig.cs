namespace SaladimQBot.Extensions;

public class SimCommandConfig
{
    public string RootPrefix { get; set; }

    public Func<Type, CommandModule>? ModuleInstancer { get; set; }

    public SimCommandConfig(string rootPrefix, Func<Type, CommandModule>? moduleInstancer = null)
    {
        this.RootPrefix = rootPrefix;
        this.ModuleInstancer = moduleInstancer;
    }
}
