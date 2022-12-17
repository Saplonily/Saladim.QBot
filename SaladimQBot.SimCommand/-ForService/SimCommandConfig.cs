using System.Linq;
using System.Reflection;

namespace SaladimQBot.SimCommand;

public class SimCommandConfig
{
    public string RootPrefix { get; set; }

    public List<Type> Modules { get; set; }

    public Func<Type, CommandModule>? ModuleInstancer { get; set; }

    public SimCommandConfig(string rootPrefix, IEnumerable<Type> modules, Func<Type, CommandModule>? moduleInstancer = null)
    {
        this.RootPrefix = rootPrefix;
        Modules = new();
        Modules.AddRange(modules);
        this.ModuleInstancer = moduleInstancer;
    }

    public SimCommandConfig(string rootPrefix, Assembly modulesAsm, Func<Type, CommandModule>? moduleInstancer = null)
    {
        this.RootPrefix = rootPrefix;
        Modules = new();
        Modules.AddRange(modulesAsm.GetTypes().Where(t => t.IsSubclassOf(typeof(CommandModule))));
        this.ModuleInstancer = moduleInstancer;
    }

    public SimCommandConfig AddAllModulesIn(Assembly asm)
    {
        Modules.AddRange(asm.GetTypes());
        return this;
    }
}
