using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CursorSnapper : MonoBehaviour
{
    public abstract Vector2 CalculateSnappedPosition(Vector2 _position);

}
