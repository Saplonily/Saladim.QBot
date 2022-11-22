using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaladimQBot.Core;

public interface IPrivateMessage : IMessage
{
    MessageTempSource TempSource { get; }
}
