using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalConfigToUI : MonoBehaviour
{
    [Header("Side-On")]
    public Vector2 sideonLocalClampMin;
    public Vector2 sideonLocalClampMax;
    public RectTransform sideonScreen;
    public RectTransform sideonLeap;

    [Space, Header("Side-On")]
    public Vector2 frontonLocalClampMin;
    public Vector2 frontonLocalClampMax;
    public RectTransform frontonScreen;
    public RectTransform frontonLeap;

    float dynamicScreenSizePX;

    private void Awake()
    {
        dynamicScreenSizePX = frontonScreen.sizeDelta.y;
    }

    private void Update()
    {
        // side-on view
        float sideLeapPosX = ScreenControlUtility.MapRangeToRange(-PhysicalConfigurable.Config.LeapPositionRelativeToScreenBottomM.z, 0, PhysicalConfigurable.Config.ScreenHeightM, 0, sideonScreen.sizeDelta.y);
        float sideLeapPosY = ScreenControlUtility.MapRangeToRange(PhysicalConfigurable.Config.LeapPositionRelativeToScreenBottomM.y, 0, PhysicalConfigurable.Config.ScreenHeightM, 0, sideonScreen.sizeDelta.y);

        sideonScreen.localRotation = Quaternion.Euler(0, 0, PhysicalConfigurable.Config.ScreenRotationD);

        sideonLeap.localPosition = sideonScreen.localPosition + new Vector3(sideLeapPosX, sideLeapPosY, 0);
        sideonLeap.localRotation = Quaternion.Euler(PhysicalConfigurable.Config.LeapRotationD.z, 0, PhysicalConfigurable.Config.LeapRotationD.x);

        sideonLeap.localPosition = new Vector3(Mathf.Clamp(sideonLeap.localPosition.x, sideonLocalClampMin.x, sideonLocalClampMax.x),
                                                Mathf.Clamp(sideonLeap.localPosition.y, sideonLocalClampMin.y, sideonLocalClampMax.y), 0);

        // front-on view
        var aspectRatio = (float)GlobalSettings.ScreenWidth / (float)GlobalSettings.ScreenHeight;

        if(aspectRatio > 1)
        {
            // landscape
            frontonScreen.sizeDelta = new Vector2(dynamicScreenSizePX, dynamicScreenSizePX * ((float)GlobalSettings.ScreenHeight / (float)GlobalSettings.ScreenWidth));
        }
        else
        {
            //portrait
            frontonScreen.sizeDelta = new Vector2(dynamicScreenSizePX * aspectRatio, dynamicScreenSizePX);
        }

        float frontLeapPosX = ScreenControlUtility.MapRangeToRange(PhysicalConfigurable.Config.LeapPositionRelativeToScreenBottomM.x, 0, PhysicalConfigurable.Config.ScreenHeightM, 0, frontonScreen.sizeDelta.y);
        float frontLeapPosY = ScreenControlUtility.MapRangeToRange(PhysicalConfigurable.Config.LeapPositionRelativeToScreenBottomM.y, 0, PhysicalConfigurable.Config.ScreenHeightM, 0, frontonScreen.sizeDelta.y);

        frontonLeap.localPosition = frontonScreen.localPosition + new Vector3(frontLeapPosX, (-frontonScreen.sizeDelta.y / 2) + frontLeapPosY, 0);
        frontonLeap.localRotation = Quaternion.Euler(0, 0, PhysicalConfigurable.Config.LeapRotationD.z);

        frontonLeap.localPosition = new Vector3(Mathf.Clamp(frontonLeap.localPosition.x, frontonLocalClampMin.x, frontonLocalClampMax.x),
                                                Mathf.Clamp(frontonLeap.localPosition.y, frontonLocalClampMin.y, frontonLocalClampMax.y), 0);
    }
}