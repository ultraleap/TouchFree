using System.Collections.Generic;
using UnityEngine;

namespace Ultraleap.ScreenControl.Client.Configuration
{
    // Class: InteractionConfig
    // This class is a container for all of the settings related to the interactions being processed
    // by the ScreenControl Service. The settings at the root of this object will affect all
    // sensations. There are also some settings specific to the Hover and Hold interaction which can
    //  be modified by changing the contained <HoverAndHoldInteractionSettings>.
    //
    // In order to modify the settings of the service, create an instance of this class, make the
    // changes you wish to see, and then send it to the server using the <ConfigurationManager>.
    //
    // Like all of the Settings classes found in this file, all members are optional. If you do
    // not modify a member of this class, its value will not change when the instance is sent to
    // ScreenControl Service.
    public class InteractionConfig
    {
        // Property: UseScrollingOrDragging
        // If true, allows interactions to send up/down events seperately, enabling dragging or
        // touchscreen-like scrolling behaviours. If false, up/down events will be sent together,
        // and every down will function like a click of its own.
        public bool UseScrollingOrDragging
        {
            get
            {
                return _UseScrollingOrDragging;
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

                _UseScrollingOrDragging = value;
            }
        }

        // Property: DeadzoneRadius
        // All interactions use a small deadzone to stabilise the position of the cursor, to prevent
        // small user movements from making the cursor shake in place. This setting controls the
        // radius of that deadzone.
        public float DeadzoneRadius
        {
            get
            {
                return _DeadzoneRadius;
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

                _DeadzoneRadius = value;
            }
        }

        // Interaction-specific settings
        public HoverAndHoldInteractionSettings hoverAndHold = new HoverAndHoldInteractionSettings();

        // Variable: configValues
        // This dictionary is used to store the edited members only, and is accessed when serializing the data for transfer to the Service.
        //
        // *DO NOT MODIFY THIS DICTIONARY MANUALLY*
        public Dictionary<string, object> configValues = new Dictionary<string, object>();

        private bool _UseScrollingOrDragging;
        private float _DeadzoneRadius;
    }

    // Class: HoverAndHoldInteractionSettings
    // This class is a container for settings that only apply to the Hover and Hold interaction. In
    // order to modify these settings of the ScreenControl Service, create an <InteractionConfig>,
    // which contains an instance of this class, modify it as required, and then pass to the service
    // using the <ConfigurationManager>.
    //
    // Like all of the Settings classes found in this file, all members are optional. If you do
    // not modify a member of this class, its value will not change when the instance is sent to
    // ScreenControl Service.
    public class HoverAndHoldInteractionSettings
    {
        // Property: HoverStartTimeS
        // This determines how long (in seconds) the user must hold their hand in place before the
        // interaction will begin. If the hand remains in place until the interaction completes,
        // a click event will be sent.
        public float HoverStartTimeS
        {
            get
            {
                return _HoverStartTimeS;
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

                _HoverStartTimeS = value;
            }
        }

        // Property: HoverCompleteTimeS
        // This determines how long (in seconds) the user must hold their hand in place after the
        // interaction has begun before the interaction will complete, and a click event will be
        // sent.
        public float HoverCompleteTimeS
        {
            get
            {
                return _HoverCompleteTimeS;
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

                _HoverCompleteTimeS = value;
            }
        }

        // Variable: configValues
        // This dictionary is used to store the edited members only, and is accessed when
        // serializing the data for transfer to the Service.
        //
        // *DO NOT MODIFY THIS DICTIONARY MANUALLY*
        public Dictionary<string, object> configValues = new Dictionary<string, object>();

        private float _HoverStartTimeS;
        private float _HoverCompleteTimeS;
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
    // ScreenControl Service.
    public class PhysicalConfig
    {
        // Property: ScreenHeightM
        // The height of the screen in meters. This is needed in order to determine the relationship
        // between hand location in the real world and pixel locations on screen.
        public float ScreenHeightM
        {
            get
            {
                return _ScreenHeightM;
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

                _ScreenHeightM = value;
            }
        }

        // Property: LeapPositionRelativeToScreenBottomM
        // The position (measured in meters) in 3d space of the Leap Motion camera relative to the
        // center of the bottom of the screen.
        public Vector3 LeapPositionRelativeToScreenBottomM
        {
            get
            {
                return _LeapPositionRelativeToScreenBottomM;
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
                _LeapPositionRelativeToScreenBottomM = value;
            }
        }

        // Property: LeapRotationD
        // The rotation of the Leap Motion Camera relative to the unity world space, measured in
        // degrees
        public Vector3 LeapRotationD
        {
            get
            {
                return _LeapRotationD;
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
                _LeapRotationD = value;
            }
        }

        // Property: ScreenRotationD
        // The rotation of the physical screen relative to the unity world space, measured in
        // degrees
        public float ScreenRotationD
        {
            get
            {
                return _ScreenRotationD;
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
                _ScreenRotationD = value;
            }
        }

        // Variable: configValues
        // This dictionary is used to store the edited members only, and is accessed when serializing the data for transfer to the Service.
        //
        // *DO NOT MODIFY THIS DICTIONARY MANUALLY*
        public Dictionary<string, object> configValues = new Dictionary<string, object>();

        private float _ScreenHeightM = 0.33f;
        private Vector3 _LeapPositionRelativeToScreenBottomM = new Vector3(0f, -0.12f, -0.25f);
        private Vector3 _LeapRotationD = Vector3.zero;
        private float _ScreenRotationD = 0f;
    }
}