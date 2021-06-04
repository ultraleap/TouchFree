using UnityEngine;
using UnityEngine.UI;

namespace Ultraleap.TouchFree.Tooling.Examples
{
    public class TextColoriser : MonoBehaviour
    {
        public Text targetText;
        public Color targetColor;

        public void SetTextColor()
        {
            targetText.color = targetColor;
        }
    }
}