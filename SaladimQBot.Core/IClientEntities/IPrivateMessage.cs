﻿namespace SaladimQBot.Core;

public interface IPrivateMessage : IMessage
{
    MessageTempSource TempSource { get; }

    long? TempSourceGroupId { get; }
}
