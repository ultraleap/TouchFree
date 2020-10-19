﻿using System.ComponentModel;

/**
    Labels the possible selections for Interactions
*/
public enum InteractionSelection    // WARNING: When adding to this enum, adding to anywhere but the last index will result in references being incorrect in prefabs. (interactions and settings). Do so with caution.
{                                   // TLDR: Add new elements to the end of the enum list
    [Description("Null - used if not set")]
    Null,
    [Description("Push")]
    Push,
    [Description("Poke")]
    Poke,
    [Description("Grab")]
    Grab,
    [Description("Hover")]
    Hover,
}
