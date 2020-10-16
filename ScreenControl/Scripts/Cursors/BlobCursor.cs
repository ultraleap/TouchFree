using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

/**
 * Descriptions of Blob States:
 * 
 * PASSIVE
 *  The cursor is present but not interacting with UI elements
 * 
 * ACTIVE
 *  The cursor is present and interacts with UI elements
 *  
 * HIDDEN
 *  The cursor is not visible
 *
 */

public enum BlobState
{
    PASSIVE,
    ACTIVE,
    HIDDEN
}

public class BlobCursor : Cursor
{
    public float cursorLerpSpeed = 15f;

    public Canvas cursorCanvas;
    public GameObject BlobMeshPrefab;
    [Header("Blob Params")]

    public Gradient passiveColour;
    public Gradient activeColour;

    public int numPoints;
    public float blobRadius;
    public float maxScreenDistance = 0.2f;

    [Range(0.001f, 10)]
    public float minDistance;
    [Range(0.001f, 10)]
    public float maxDistance;

    [Range(0.001f, 0.1f)]
    public float passiveLineWidth = 0.01f;
    [Range(0.001f, 0.1f)]
    public float activeLineWidth;

    public Vector3 blobOffset;
    public float wobbleAmount;
    [Range(-10, 10)]
    public float depthEffect;

    public AnimationCurve wobbleCurve;
    public AnimationCurve snapFrom;
    
    [Header("Mesh")]
    public bool drawFill = true;
    public bool maintainAspect = true;
    
    [Header("Debug")]
    public bool drawDebug;

    private float lineWidth;
    private BlobState cursorState;
    private Collider2D buttonCollider;
    private LineRenderer blobLine;
    private MeshFilter blobMesh;

    private List<Vector3> blobPoints;
    private List<Vector3> basePoints;
    private Vector3[] buttonOutline;
    private int wobbleActive;
    private GameObject closestPointGameObject;
    private Vector2 cursorPosition, clickPosition;

    void Start()
    {
        BlobMeshPrefab = Instantiate(BlobMeshPrefab);
        blobMesh = BlobMeshPrefab.GetComponent<MeshFilter>();
        blobMesh.mesh.MarkDynamic();
        cursorCanvas = gameObject.GetComponentInParent<Canvas>();

        ChangeToCursor(BlobState.PASSIVE);
        InteractionManager.HandleInputAction += OnHandleInputAction;
        blobPoints = new List<Vector3>();
        blobLine = GetComponent<LineRenderer>();

        InitialiseBasePoints();
        InitBlob();

        maxScreenDistance = 1;
        blobLine.colorGradient = passiveColour;


        wobbleActive = 1;
        lineWidth = passiveLineWidth;

        closestPointGameObject = new GameObject("tempCollider");
        gameObject.layer = 8;

        foreach (Transform child in transform)
        {
            child.gameObject.layer = 8;
        }
    }

    void OnDestroy()
    {
        InteractionManager.HandleInputAction -= OnHandleInputAction;
    }

    public void UpdateBlobCursor()
    {
        _targetPos = cursorPosition;

        if (numPoints != basePoints.Count)
        {
            InitialiseBasePoints();
            InitBlob();
        }

        blobLine.widthMultiplier = Mathf.Lerp(blobLine.widthMultiplier, lineWidth, cursorLerpSpeed * Time.deltaTime);
        buttonOutline = ButtonOutline(Camera.main.ScreenToWorldPoint(clickPosition));
        UpdateBlobVisual(Camera.main.ScreenToWorldPoint(cursorPosition));
    }

    public void ChangeToCursor(BlobState _state)
    {
        if (cursorState != _state)
        {
            cursorState = _state;

            switch (_state)
            {
                case BlobState.PASSIVE:
                    blobLine.colorGradient = passiveColour;
                    wobbleActive = 1;
                    lineWidth = passiveLineWidth;
                    break;

                case BlobState.ACTIVE:
                    blobLine.colorGradient = activeColour;
                    wobbleActive = 0;
                    blobLine.widthMultiplier *= 2;
                    lineWidth = activeLineWidth;
                    break;

                case BlobState.HIDDEN:
                    blobLine.colorGradient = passiveColour;
                    wobbleActive = 1;
                    blobLine.widthMultiplier *= 0.5f;
                    lineWidth = passiveLineWidth;
                    break;
            }
        }
    }

    public BlobState GetBlobState()
    {
        return cursorState;
    }

    protected override void OnHandleInputAction(InputActionData _inputData)
    {
        InputType _type = _inputData.Type;
        Vector2 _cursorPosition = _inputData.CursorPosition;
        Vector2 _clickPosition = _inputData.ClickPosition;
        float _distanceFromScreen = _inputData.ProgressToClick;

        clickPosition = _clickPosition;
        cursorPosition = _cursorPosition;

        switch (_type)
        {
            case InputType.MOVE:
                UpdateBlobCursor();
                if (_distanceFromScreen > maxScreenDistance)
                {
                    ChangeToCursor(BlobState.HIDDEN);
                }
                else if (cursorState == BlobState.HIDDEN)
                {
                    ChangeToCursor(BlobState.PASSIVE);
                }
                break;

            case InputType.DOWN:
                if (_distanceFromScreen <= maxScreenDistance)
                {
                    ChangeToCursor(BlobState.ACTIVE);
                }
                break;

            case InputType.DRAG:
                break;

            case InputType.CANCEL:
                break;

            case InputType.UP:
                if (cursorState != BlobState.HIDDEN)
                {
                    ChangeToCursor(BlobState.PASSIVE);
                }
                break;

            case InputType.HOVER:
                break;
        }
    }

    public override void HideCursor()
    {
        base.HideCursor();
        ClearBlob();
    } 

    public override void ShowCursor()
    {
        base.ShowCursor();
        InitBlob();
    }
    
    // Return a new color with the specified opacity
    private Color ChangeOpacity(Color _oldColor, float _newOpacity)
    {
        return new Color(_oldColor.r, _oldColor.g, _oldColor.b, _newOpacity);
    }

    private void InitialiseBasePoints()
    {
        basePoints = new List<Vector3>();

        for (int i = 0; i < numPoints; i++)
        {
            var point = new Vector3()
            {
                x = (Mathf.Cos((2 * Mathf.PI / numPoints) * i) * blobRadius) * cursorCanvas.transform.lossyScale.x,
                y = (Mathf.Sin((2 * Mathf.PI / numPoints) * i) * blobRadius) * cursorCanvas.transform.lossyScale.y,
                z = 0
            };
            basePoints.Add(point);
        }
    }

    private void UpdateBlobVisual(Vector2 _worldPosition)
    {
        var wobble = Wobble();

        for (int i = 0; i < numPoints; i++)
        {
            var point = new Vector3()
            {
                x = _worldPosition.x + (basePoints[i].x * 0.5f),
                y = _worldPosition.y + (basePoints[i].y * 0.5f),
                z = blobOffset.z
            };
            point = Vector3.Lerp(point, buttonOutline[i] + wobble, DistanceToCollider(_targetPos, buttonCollider));
            blobPoints[i] = Vector3.Lerp(blobPoints[i], point, cursorLerpSpeed * Time.deltaTime);
        }

        blobLine.positionCount = numPoints;
        blobLine.SetPositions(blobPoints.ToArray());

        if (drawFill)
        {
            ConvertOutlineToMesh(blobPoints);
        }
    }

    private float DistanceToCollider(Vector2 _targetPosition, Collider2D _buttonCollider)
    {
        if (_buttonCollider != null)
        {
            CircleCollider2D closestPointCollider = closestPointGameObject.AddComponent<CircleCollider2D>();
            closestPointCollider.radius = blobRadius;
            closestPointGameObject.transform.position = Camera.main.ScreenToWorldPoint(_targetPosition);
            ColliderDistance2D distanceCollider = closestPointCollider.Distance(_buttonCollider);
            Destroy(closestPointCollider);

            float buttonDistance = Mathf.Abs(distanceCollider.distance) / maxDistance - minDistance;
            float distance = snapFrom.Evaluate(buttonDistance + (1 - wobbleActive));
            return distance;
        }
        else
        {
            return 0;
        }
    }

    private Vector3 Wobble()
    {
        var wobble = Vector3.zero;

        if (buttonCollider != null)
        {
            wobble = (Camera.main.ScreenToWorldPoint(_targetPos) - buttonCollider.transform.position);
            wobble.z = 0;
            wobble = wobble.normalized * wobbleCurve.Evaluate(wobble.magnitude * wobbleAmount);
        }

        return wobble * wobbleActive;
    }
    private void FadeBlob()
    {
        for (int i = 0; i < numPoints; i++)
        {
            blobPoints[i] = Vector3.Lerp(blobPoints[i], Camera.main.ScreenToWorldPoint(_targetPos), cursorLerpSpeed * Time.deltaTime);
        }

        blobLine.SetPositions(blobPoints.ToArray());
    }

    private void ClearBlob()
    {
        for (int i = 0; i < numPoints; i++)
        {
            blobPoints[i] = (Camera.main.ScreenToWorldPoint(_targetPos));
        }

        blobLine.positionCount = 0;
        blobLine.SetPositions(blobPoints.ToArray());
        ClearMesh();
    }

    private void InitBlob()
    {
        blobPoints.Clear();

        for (int i = 0; i < numPoints; i++)
        {
            blobPoints.Add(Camera.main.ScreenToWorldPoint(_targetPos));
        }
    }

    /**
     * Generates points on the button collider
     * Iterates over the blob base radius & finds the closest point on the collider to map a point to
     */
    private Vector3[] ButtonOutline(Vector3 _currentPosition)
    {
        int layerMask = LayerMask.GetMask("Cursor");
        layerMask = ~layerMask;
        Collider2D[] results = new Collider2D[1];
        Physics2D.OverlapPointNonAlloc(_currentPosition, results, layerMask);
        buttonCollider = results[0];

        var points = new Vector3[numPoints];

        if (buttonCollider != null)
        {
            //Alternatively
            for (int i = 0; i < numPoints; i++)
            {
                points[i] = buttonCollider.ClosestPoint(buttonCollider.transform.position + (basePoints[i] * blobRadius));
                points[i].z = blobOffset.z;
            }
        }

        return points;
    }

    private void SmoothBlob()
    {
        var next = 1;
        var prev = numPoints - 1;

        for (int i = 0; i < numPoints; i++)
        {
            blobPoints[i] = ((blobPoints[prev] + blobPoints[i] + blobPoints[next]) / 3);

            next = (next + 1) % numPoints;
            prev = (prev + 1) % numPoints;
        }
    }

    private void ConvertOutlineToMesh(List<Vector3> _points)
    {
        var centre = BlobCentre();
        centre.z = _points[0].z;

        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uvs = new List<Vector2>();
        var size = BlobSize();

        vertices.Add(centre);
        vertices.Add(_points[0]);
        uvs.Add(new Vector2(0.5f, 0.5f));
        uvs.Add(BlobVertToUV(_points[0], centre, size));

        for (int i = 2; i < numPoints; i++)
        {
            vertices.Add(_points[i]);
            uvs.Add(BlobVertToUV(_points[i], centre, size));

            triangles.Add(0);
            triangles.Add(i);
            triangles.Add(i - 1);

            if (drawDebug)
            {
                Debug.DrawLine(vertices[0], vertices[i - 1], Color.cyan);
                Debug.DrawLine(vertices[0], vertices[i], Color.cyan);
                Debug.DrawLine(vertices[i - 1], vertices[i], Color.yellow);
            }
        }

        triangles.Add(0);
        triangles.Add(1);
        triangles.Add(numPoints - 1);

        if (drawDebug)
        {
            Debug.DrawLine(vertices[0], vertices[numPoints - 1], Color.cyan);
            Debug.DrawLine(vertices[0], vertices[1], Color.cyan);
            Debug.DrawLine(vertices[1], vertices[numPoints - 1], Color.yellow);
        }

        blobMesh.mesh.Clear();
        blobMesh.mesh.SetVertices(vertices);
        blobMesh.mesh.SetTriangles(triangles, 0);
        blobMesh.mesh.SetUVs(0, uvs);

        blobMesh.mesh.RecalculateNormals();
    }

    private Vector2 BlobVertToUV(Vector3 vert, Vector3 centre, Vector2 size)
    {
        var uvScale = Mathf.Max(size.x, size.y);
        if (maintainAspect) size = new Vector2(uvScale, uvScale);
        
        var uv = new Vector2();
        uv.x = ((vert.x - centre.x) / size.x) + 0.5f;
        uv.y = ((vert.y - centre.y) / size.y) + 0.5f;
        return uv;
    }
    public Vector2 BlobSize()
    {
        var minX = float.PositiveInfinity;
        var maxX = float.NegativeInfinity;
        
        var minY = float.PositiveInfinity;
        var maxY = float.NegativeInfinity;

        foreach (var point in blobPoints)
        {
            minX = Mathf.Min(point.x, minX);
            minY = Mathf.Min(point.y, minY);
            maxX = Mathf.Max(point.x, maxX);
            maxY = Mathf.Max(point.y, maxY);
        }

        return new Vector2(maxX - minX, maxY - minY);
    }


    public Vector3 BlobCentre()
    {
        Vector3 result = Vector3.zero;

        for (int i = 0; i < numPoints; i++)
        {
            result += blobPoints[i];
        }

        return result / numPoints;
    }

    private void ClearMesh()
    {
        // TODO - Work out why once this has been called it stops rendering forever
        blobMesh.mesh.Clear();
    }
}