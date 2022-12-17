using System;
using Microsoft.Extensions.Hosting;
using SaladimQBot.Core;

namespace SaladimQBot.Services.Abstraction;

public interface IClientService
{
    IClient Client { get; }
}
