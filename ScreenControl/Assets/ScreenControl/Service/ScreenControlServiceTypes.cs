using System;

using UnityEngine;

using Ultraleap.ScreenControl.Core;
using Ultraleap.ScreenControl.Core.ScreenControlTypes;

namespace Ultraleap.ScreenControl.Service.ScreenControlTypes
{
    internal enum Compatibility
    {
        COMPATIBLE,
        CORE_OUTDATED,
        CLIENT_OUTDATED
    }
}