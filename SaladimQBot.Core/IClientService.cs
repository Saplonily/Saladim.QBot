using System;
using SaladimQBot.Core;

namespace SaladimQBot.Core.Services;

public interface IClientService
{
    IClient Client { get; }
}
