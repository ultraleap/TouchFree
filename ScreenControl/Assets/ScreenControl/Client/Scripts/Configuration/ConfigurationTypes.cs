using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ultraleap.ScreenControl.Client
{
    namespace ScreenControlTypes
    {
        public class HoverAndHoldInteractionSettings
        {
            private float _HoverCursorStartTimeS;
            private float _HoverCursorCompleteTimeS;

            public float HoverCursorStartTimeS
            {
                get
                {
                    return _HoverCursorStartTimeS;
                }
                set
                {
                    if (configValues.ContainsKey("HoverCursorStartTimeS"))
                    {
                        configValues["HoverCursorStartTimeS"] = value;
                    }
                    else
                    {
                        configValues.Add("HoverCursorStartTimeS", value);
                    }

                    _HoverCursorStartTimeS = value;
                }
            }
            public float HoverCursorCompleteTimeS
            {
                get
                {
                    return _HoverCursorCompleteTimeS;
                }
                set
                {
                    if (configValues.ContainsKey("HoverCursorCompleteTimeS"))
                    {
                        configValues["HoverCursorCompleteTimeS"] = value;
                    }
                    else
                    {
                        configValues.Add("HoverCursorCompleteTimeS", value);
                    }

                    _HoverCursorCompleteTimeS = value;
                }
            }

            public Dictionary<string, object> configValues = new Dictionary<string, object>();
        }

        public class InteractionConfig
        {
            private Vector3 _UseScrollingOrDragging;
            private float _DeadzoneRadius;

            public Vector3 UseScrollingOrDragging
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
            public HoverAndHoldInteractionSettings HoverAndHold = new HoverAndHoldInteractionSettings();

            public Dictionary<string, object> configValues = new Dictionary<string, object>();
        }

        public class PhysicalConfig
        {
            private float _ScreenHeightM = 0.33f;
            private Vector3 _LeapPositionRelativeToScreenBottomM = new Vector3(0f, -0.12f, -0.25f);
            private Vector3 _LeapRotationD = Vector3.zero;
            private float _ScreenRotationD = 0f;

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

            public Dictionary<string, object> configValues = new Dictionary<string, object>();
        }

        public class GlobalSettings
        {
            public int ScreenWidth;
            public int ScreenHeight;
        }
    }
}