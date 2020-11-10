using System;

using UnityEngine;

using Ultraleap.ScreenControl.Core;
using Ultraleap.ScreenControl.Core.ScreenControlTypes;

namespace Ultraleap.ScreenControl.Service.ScreenControlTypes
{
    internal enum ActionCodes
    {
        INPUT_ACTION,
        CONFIGURATION_STATE,
        CONFIGURATION_RESPONSE
    }

    internal enum Compatibility
    {
        COMPATIBLE,
        CORE_OUTDATED,
        CLIENT_OUTDATED
    }

    internal struct CommunicationWrapper<T>
    {
        public string action;
        public T content;

        public CommunicationWrapper(string _actionCode, T _content)
        {
            action = _actionCode;
            content = _content;
        }
    }
}