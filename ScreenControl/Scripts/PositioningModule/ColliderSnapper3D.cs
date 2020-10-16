using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderSnapper3D : CursorSnapper
{
    public Camera cursorCamera;

    [Header("Snapping")]
    public float snapRadius = 0.25f;
    public float releaseDistance = 0.1f;
    [Range(0.001f, 1000)] public float maxRayDistance = 100;
    public bool drawDebug = false;

    [Tooltip ("Larger numbers make the cursor stick to last collider. Set to 1 to always return the closest collider.")]
    public float snapMultiplier = 1.5f;

    [Tooltip ("Make sure to set the layermask so the cursor snaps only to the colliders you want")]
    public LayerMask layerMask = -1;

    private RaycastHit _currentHit;
    private Collider _currentSnappedCollider;

    void Start()
    {
        if (cursorCamera == null) cursorCamera = Camera.main;
    }
    
    void OnDrawGizmos()
    {
        if (Application.isPlaying && drawDebug)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_currentHit.point, snapRadius);
            
            if (_currentSnappedCollider != null) 
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(_currentSnappedCollider.transform.position, snapRadius);
            }
        }
    }

    /// <Summary>
    /// Snap the input cursor position to the nearest collider along the path of a raycast into the scene.
    /// Snapping is biased towards the collider it is currently snapped to.
    /// </Summmary>
    public override Vector2 CalculateSnappedPosition(Vector2 _position)
    {
        var ray = cursorCamera.ViewportPointToRay(cursorCamera.ScreenToViewportPoint(_position));

        var hits = Physics.SphereCastAll(ray.origin, snapRadius, ray.direction, maxRayDistance, layerMask);
        
        if (drawDebug) Debug.DrawRay(ray.origin, ray.direction * maxRayDistance, Color.cyan );

        if (hits.Length > 0)
        {
            _currentSnappedCollider = SnapCollider(_position, hits, out _currentHit);

            if (drawDebug) DrawHitRays(hits);
            
            return cursorCamera.WorldToScreenPoint(_currentSnappedCollider.transform.position);        
        }
        else
        {
            return NoHitPoint(_position);
        }        
    }

    private Collider SnapCollider(Vector2 _cursorPos, RaycastHit[] _hits, out RaycastHit closestHit)
    {
        var closestCollider = _currentSnappedCollider;

        HitCurrentCollider(_hits, out closestHit);
       
        var snappedDistance = closestCollider == null ? Mathf.Infinity : HitDistance(_cursorPos, closestHit);
                
        foreach(var hit in _hits)
        {
            if (hit.collider != _currentSnappedCollider)
            {
                var hitDistance = HitDistance(_cursorPos, hit) * snapMultiplier;

                if (hitDistance < snappedDistance)
                {
                    snappedDistance = hitDistance;
                    closestCollider = hit.collider;
                    closestHit = hit;
                }
            }
        }
        
        return closestCollider;
    }

    private Vector2 NoHitPoint(Vector2 _cursorPos)
    {
        if (_currentSnappedCollider == null)
        {
            return _cursorPos;
        }
        
        if (HitDistance(_cursorPos, _currentHit) > releaseDistance )
        {
            _currentSnappedCollider = null;
            return _cursorPos;
        }
        else 
        {
            return cursorCamera.WorldToScreenPoint(_currentSnappedCollider.transform.position);
        }
    }

    private float HitDistance(Vector3 _cursorPos, RaycastHit _hit)
    {
        return Vector2.Distance(cursorCamera.WorldToViewportPoint(_hit.point), cursorCamera.ScreenToViewportPoint(_cursorPos));
    }

    private bool HitCurrentCollider(RaycastHit[] _hits, out RaycastHit currentHit)
    {
        foreach(var hit in _hits)
        {
            if (hit.collider == _currentSnappedCollider)
            {
                currentHit = hit;
                return true;
            }
        }
        currentHit = _currentHit;
        return false;
    }

    private void DrawHitRays(RaycastHit[] _hits)
    {
        foreach(var hit in _hits)
        {
            Debug.DrawLine(cursorCamera.transform.position, hit.point, Color.yellow);
        }
    }

    public RaycastHit GetHitPoint()
    {
        return _currentHit;
    }

    public Collider GetSnappedCollider()
    {
        return _currentSnappedCollider;
    }

}
