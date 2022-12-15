using System;
using SaladimQBot.Core;

namespace SaladimQBot.SimCommand;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class CommandAttribute : Attribute
{
    public string Name { get; protected set; }

    public CommandAttribute(string name)
    {
        this.Name = name;
    }
}
