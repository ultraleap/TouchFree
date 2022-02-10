using System;

namespace Ultraleap.TouchFree.Library.Configuration
{
    public class InteractionConfigFile : ConfigFile<InteractionConfigForFile, InteractionConfigFile>
    {
        protected override string _ConfigFileName => "InteractionConfig.json";
    }

    [Serializable]
    public class HoverAndHoldInteractionSettingsForFile
    {
        public float HoverStartTimeS = 0.5f;
        public float HoverCompleteTimeS = 0.6f;
    }

    [Serializable]
    public class TouchPlaneInteractionSettingsForFile
    {
        public float TouchPlaneActivationDistanceCm = 5f;
        public TrackedPosition TouchPlaneTrackedPosition = TrackedPosition.NEAREST;
    }

    [Serializable]
    public class InteractionConfigForFile
    {
        public bool UseScrollingOrDragging = false;
        public float DeadzoneRadius = 0.003f;

        public bool InteractionZoneEnabled = false;
        public float InteractionMinDistanceCm = 0.0f;
        public float InteractionMaxDistanceCm = 25.0f;

        public InteractionType InteractionType = InteractionType.PUSH;

        // Interaction-specific settings
        public HoverAndHoldInteractionSettingsForFile HoverAndHold = new HoverAndHoldInteractionSettingsForFile();
        public TouchPlaneInteractionSettingsForFile TouchPlane = new TouchPlaneInteractionSettingsForFile();
    }
}