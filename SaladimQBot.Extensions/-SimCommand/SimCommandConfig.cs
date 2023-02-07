namespace SaladimQBot.Extensions;

public class SimCommandConfig
{
    public string[] RootPrefixes { get; set; }

    public Func<Type, CommandModule>? ModuleInstancer { get; set; }

    public SimCommandConfig(Func<Type, CommandModule>? moduleInstancer = null, params string[] rootPrefixes)
    {
        this.RootPrefixes = rootPrefixes;
        this.ModuleInstancer = moduleInstancer;
    }
}
