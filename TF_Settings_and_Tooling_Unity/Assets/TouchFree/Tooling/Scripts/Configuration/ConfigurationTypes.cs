using System.Collections.Generic;
using UnityEngine;

namespace Ultraleap.TouchFree.Tooling.Configuration
{
    // Class: InteractionConfig
    // This class is a container for all of the settings related to the interactions being processed
    // by the TouchFree Service. The settings at the root of this object will affect all
    // sensations. There are also some settings specific to the Hover and Hold interaction which can
    //  be modified by changing the contained <HoverAndHoldInteractionSettings>.
    //
    // In order to modify the settings of the service, create an instance of this class, make the
    // changes you wish to see, and then send it to the server using the <ConfigurationManager>.
    //
    // Like all of the Settings classes found in this file, all members are optional. If you do
    // not modify a member of this class, its value will not change when the instance is sent to
    // TouchFree Service.
    [System.Serializable]
    public class InteractionConfig
    {
        // Property: useScrollingOrDragging
        // If true, allows interactions to send up/down events seperately, enabling dragging or
        // touchscreen-like scrolling behaviours. If false, up/down events will be sent together,
        // and every down will function like a click of its own.
        public bool useScrollingOrDragging
        {
            get
            {
                return UseScrollingOrDragging;
            }
            set
            {
                if (configValues.ContainsKey("UseScrollingOrDragging"))
                {
                    configValues["UseScrollingOrDragging"] = value;
                }
                else
                {
                    configValues.Add("UseScrollingOrDragging", value);
                }

                UseScrollingOrDragging = value;
            }
        }

        // Property: useSwipeInteraction
        // If true, enables the swipe interaction alongside AirPush, Hover and Hold or TouchPlane if
        // one of those interactions is configured
        public bool useSwipeInteraction
        {
            get
            {
                return UseSwipeInteraction;
            }
            set
            {
                if (configValues.ContainsKey("UseSwipeInteraction"))
                {
                    configValues["UseSwipeInteraction"] = value;
                }
                else
                {
                    configValues.Add("UseSwipeInteraction", value);
                }

                UseSwipeInteraction = value;
            }
        }

        // Property: deadzoneRadius
        // All interactions use a small deadzone to stabilise the position of the cursor, to prevent
        // small user movements from making the cursor shake in place. This setting controls the
        // radius of that deadzone.
        public float deadzoneRadius
        {
            get
            {
                return DeadzoneRadius;
            }
            set
            {
                if (configValues.ContainsKey("DeadzoneRadius"))
                {
                    configValues["DeadzoneRadius"] = value;
                }
                else
                {
                    configValues.Add("DeadzoneRadius", value);
                }

                DeadzoneRadius = value;
            }
        }

        // Property: interactionZoneEnabled
        // Changes whether the Interaction Zone values will be used
        public bool interactionZoneEnabled
        {
            get
            {
                return InteractionZoneEnabled;
            }
            set
            {
                if (configValues.ContainsKey("InteractionZoneEnabled"))
                {
                    configValues["InteractionZoneEnabled"] = value;
                }
                else
                {
                    configValues.Add("InteractionZoneEnabled", value);
                }

                InteractionZoneEnabled = value;
            }
        }

        // Property: interactionMinDistanceCm
        // The minimum distance from the screen that users can interact within
        public float interactionMinDistanceCm
        {
            get
            {
                return InteractionMinDistanceCm;
            }
            set
            {
                if (configValues.ContainsKey("InteractionMinDistanceCm"))
                {
                    configValues["InteractionMinDistanceCm"] = value;
                }
                else
                {
                    configValues.Add("InteractionMinDistanceCm", value);
                }

                InteractionMinDistanceCm = value;
            }
        }

        // Property: interactionMaxDistanceCm
        // The maximum distance from the screen that users can interact within
        public float interactionMaxDistanceCm
        {
            get
            {
                return InteractionMaxDistanceCm;
            }
            set
            {
                if (configValues.ContainsKey("InteractionMaxDistanceCm"))
                {
                    configValues["InteractionMaxDistanceCm"] = value;
                }
                else
                {
                    configValues.Add("InteractionMaxDistanceCm", value);
                }

                InteractionMaxDistanceCm = value;
            }
        }

        // Property: interactionType
        // This represents the type of interaction currently selected
        public InteractionType interactionType
        {
            get
            {
                return InteractionType;
            }
            set
            {
                if (configValues.ContainsKey("InteractionType"))
                {
                    configValues["InteractionType"] = value;
                }
                else
                {
                    configValues.Add("InteractionType", value);
                }

                InteractionType = value;
            }
        }

        // Interaction-specific settings
        public HoverAndHoldInteractionSettings HoverAndHold = new HoverAndHoldInteractionSettings();
        public TouchPlaneInteractionSettings TouchPlane = new TouchPlaneInteractionSettings();

        // Variable: configValues
        // This dictionary is used to store the edited members only, and is accessed when serializing the data for transfer to the Service.
        //
        // *DO NOT MODIFY THIS DICTIONARY MANUALLY*
        public Dictionary<string, object> configValues = new Dictionary<string, object>();

        [SerializeField] private bool UseScrollingOrDragging;
        [SerializeField] private bool UseSwipeInteraction;
        [SerializeField] private float DeadzoneRadius;
        [SerializeField] private bool InteractionZoneEnabled;
        [SerializeField] private float InteractionMinDistanceCm;
        [SerializeField] private float InteractionMaxDistanceCm;
        [SerializeField] private InteractionType InteractionType;
    }

    // Class: HoverAndHoldInteractionSettings
    // This class is a container for settings that only apply to the Hover and Hold interaction. In
    // order to modify these settings of the TouchFree Service, create an <InteractionConfig>,
    // which contains an instance of this class, modify it as required, and then pass to the service
    // using the <ConfigurationManager>.
    //
    // Like all of the Settings classes found in this file, all members are optional. If you do
    // not modify a member of this class, its value will not change when the instance is sent to
    // TouchFree Service.
    [System.Serializable]
    public class HoverAndHoldInteractionSettings
    {
        // Property: hoverStartTimeS
        // This determines how long (in seconds) the user must hold their hand in place before the
        // interaction will begin. If the hand remains in place until the interaction completes,
        // a click event will be sent.
        public float hoverStartTimeS
        {
            get
            {
                return HoverStartTimeS;
            }
            set
            {
                if (configValues.ContainsKey("HoverStartTimeS"))
                {
                    configValues["HoverStartTimeS"] = value;
                }
                else
                {
                    configValues.Add("HoverStartTimeS", value);
                }

                HoverStartTimeS = value;
            }
        }

        // Property: hoverCompleteTimeS
        // This determines how long (in seconds) the user must hold their hand in place after the
        // interaction has begun before the interaction will complete, and a click event will be
        // sent.
        public float hoverCompleteTimeS
        {
            get
            {
                return HoverCompleteTimeS;
            }
            set
            {
                if (configValues.ContainsKey("HoverCompleteTimeS"))
                {
                    configValues["HoverCompleteTimeS"] = value;
                }
                else
                {
                    configValues.Add("HoverCompleteTimeS", value);
                }

                HoverCompleteTimeS = value;
            }
        }

        // Variable: configValues
        // This dictionary is used to store the edited members only, and is accessed when
        // serializing the data for transfer to the Service.
        //
        // *DO NOT MODIFY THIS DICTIONARY MANUALLY*
        public Dictionary<string, object> configValues = new Dictionary<string, object>();

        [SerializeField] private float HoverStartTimeS;
        [SerializeField] private float HoverCompleteTimeS;
    }

    // Class: TouchPlaneInteractionSettings
    // This class is a container for settings that only apply to the TouchPlane interaction. In
    // order to modify these settings of the TouchFree Service, create an <InteractionConfig>,
    // which contains an instance of this class, modify it as required, and then pass to the service
    // using the <ConfigurationManager>.
    //
    // Like all of the Settings classes found in this file, all members are optional. If you do
    // not modify a member of this class, its value will not change when the instance is sent to
    // TouchFree Service.
    [System.Serializable]
    public class TouchPlaneInteractionSettings
    {
        // Property: touchPlaneActivationDistanceCM
        // This determines how far (in cm) the TouchPlane is from the screen surface. This
        // represents the plane that the user must pass to begin and end a click event.
        public float touchPlaneActivationDistanceCM
        {
            get
            {
                return TouchPlaneActivationDistanceCM;
            }
            set
            {
                if (configValues.ContainsKey("TouchPlaneActivationDistanceCM"))
                {
                    configValues["TouchPlaneActivationDistanceCM"] = value;
                }
                else
                {
                    configValues.Add("TouchPlaneActivationDistanceCM", value);
                }

                TouchPlaneActivationDistanceCM = value;
            }
        }

        // Property: touchPlaneTrackedPosition
        // This determines which bone position will be tracked when performing the interaction.
        public TrackedPosition touchPlaneTrackedPosition
        {
            get
            {
                return TouchPlaneTrackedPosition;
            }
            set
            {
                if (configValues.ContainsKey("TouchPlaneTrackedPosition"))
                {
                    configValues["TouchPlaneTrackedPosition"] = value;
                }
                else
                {
                    configValues.Add("TouchPlaneTrackedPosition", value);
                }

                TouchPlaneTrackedPosition = value;
            }
        }

        // Variable: configValues
        // This dictionary is used to store the edited members only, and is accessed when
        // serializing the data for transfer to the Service.
        //
        // *DO NOT MODIFY THIS DICTIONARY MANUALLY*
        public Dictionary<string, object> configValues = new Dictionary<string, object>();

        [SerializeField] private float TouchPlaneActivationDistanceCM;
        [SerializeField] private TrackedPosition TouchPlaneTrackedPosition;
    }

    // Class: PhysicalConfig
    // This class is a container for all of the settings related to the physical setup of the
    // hardware, both the tracking camera and the display.
    //
    // In order to modify the settings of the service, create an instance of this class, make the
    // changes you wish to see, and then send it to the server using the <ConfigurationManager>.
    //
    // Like all of the Settings classes found in this file, all members are optional. If you do
    // not modify a member of this class, its value will not change when the instance is sent to
    // TouchFree Service.
    [System.Serializable]
    public class PhysicalConfig
    {
        // Property: screenHeightM
        // The height of the screen in meters. This is needed in order to determine the relationship
        // between hand location in the real world and pixel locations on screen.
        public float screenHeightM
        {
            get
            {
                return ScreenHeightM;
            }
            set
            {
                if (configValues.ContainsKey("ScreenHeightM"))
                {
                    configValues["ScreenHeightM"] = value;
                }
                else
                {
                    configValues.Add("ScreenHeightM", value);
                }

                ScreenHeightM = value;
            }
        }

        // Property: leapPositionRelativeToScreenBottomM
        // The position (measured in meters) in 3d space of the Leap Motion camera relative to the
        // center of the bottom edge of the screen.
        //
        // This uses a left handed coordinate system where:
        // X = left/right (right = positive)
        // Y = up/down (up = positive)
        // Z = forward/backward (forward = positive)
        public Vector3 leapPositionRelativeToScreenBottomM
        {
            get
            {
                return LeapPositionRelativeToScreenBottomM;
            }
            set
            {
                if (configValues.ContainsKey("LeapPositionRelativeToScreenBottomM"))
                {
                    configValues["LeapPositionRelativeToScreenBottomM"] = value;
                }
                else
                {
                    configValues.Add("LeapPositionRelativeToScreenBottomM", value);
                }
                LeapPositionRelativeToScreenBottomM = value;
            }
        }

        // Property: leapRotationD
        // The rotation of the Leap Motion Camera relative to the unity world space, measured in
        // degrees
        public Vector3 leapRotationD
        {
            get
            {
                return LeapRotationD;
            }
            set
            {
                if (configValues.ContainsKey("LeapRotationD"))
                {
                    configValues["LeapRotationD"] = value;
                }
                else
                {
                    configValues.Add("LeapRotationD", value);
                }
                LeapRotationD = value;
            }
        }

        // Property: screenRotationD
        // The rotation of the physical screen relative to the unity world space, measured in
        // degrees
        public float screenRotationD
        {
            get
            {
                return ScreenRotationD;
            }
            set
            {
                if (configValues.ContainsKey("ScreenRotationD"))
                {
                    configValues["ScreenRotationD"] = value;
                }
                else
                {
                    configValues.Add("ScreenRotationD", value);
                }
                ScreenRotationD = value;
            }
        }

        // Variable: configValues
        // This dictionary is used to store the edited members only, and is accessed when serializing the data for transfer to the Service.
        //
        // *DO NOT MODIFY THIS DICTIONARY MANUALLY*
        public Dictionary<string, object> configValues = new Dictionary<string, object>();

        [SerializeField] private float ScreenHeightM = 0.33f;
        [SerializeField] private Vector3 LeapPositionRelativeToScreenBottomM = new Vector3(0f, -0.12f, -0.25f);
        [SerializeField] private Vector3 LeapRotationD = Vector3.zero;
        [SerializeField] private float ScreenRotationD = 0f;
    }

    // Enum: TrackedPosition
    // INDEX_STABLE - Towards the screen from the proximal knuckle position of the index finger 
    // INDEX_TIP - The index finger tip position
    // WRIST - The wrist position
    // NEAREST - The nearest bone to the screen
    [System.Serializable]
    public enum TrackedPosition
    {
        INDEX_STABLE,
        INDEX_TIP,
        WRIST,
        NEAREST
    }
}