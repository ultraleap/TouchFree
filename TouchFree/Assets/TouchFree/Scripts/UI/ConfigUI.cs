using UnityEngine;
using UnityEngine.UI;

namespace Ultraleap.TouchFree
{
    public class ConfigUI : MonoBehaviour
    {
        #region CursorSettings
        public Toggle EnableCursorToggle;
        // public SliderAndInputFieldMirrorValues CursorSizeInputSlider;
        public ToggleGroup CursorColorPresetToggles;
        public Color PrimaryColor;
        public Color SecondaryColor;
        public Color TertiaryColor;
        #endregion

        #region CTISettings
        public Toggle EnableCTIToggle;
        public Text CurrentCTIFilepath;
        public ToggleGroup CTIHideTriggerToggles;
        public InputField CTIShowDelayField;
        #endregion

        #region InteractionZone
        public Toggle InteractionZoneToggle;
        // public SliderAndInputFieldMirrorValues InteractionMinimumDistanceInputSlider;
        // public SliderAndInputFieldMirrorValues InteractionMaximumDistanceInputSlider;
        #endregion
    }
}