using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ultraleap.ScreenControl.Client.Cursors;

public class PinchGrabLineCursor : DotCursor
{
    [Header("Grab Line")]
    [Header("Line Renderers")]
    public bool DrawDebugGizmos;
    public LineRenderer lineRenderer;
    public LineRenderer lineRendererOutline;
    [Range(1, 100)] public int LineRendererPositions;
    [Range(0.01f, 1)] public float BaseLineWidthScale;
    [Range(0.01f, 2)] public float BaseOutlineWidthScale;

    [Header("Arcs")]
    public AnimationCurve radiusScale;
    [Range(0.01f, 4)] public float CircleCentreOffsetBaseScale;
    [Range(0, 1)] public float ArcLengthPercentage;
    [Range(0, 1)] public float OutlineArcLengthPercentage;
    [Range(0.01f, 4)] public float ArcMidpointOffsetScale;
    [Range(0, 360)] public float ArcAngleRight = 135;
    [Range(0, 360)] public float ArcAngleLeft = 135;

    [Header("Grab Line Pulse")]
    [Range(0.01f, 4)] public float LineGrowthMultiplier;
    [Range(0.01f, 1)] public float LineGrowthSeconds;
    public AnimationCurve lineGrowCurve, lineShrinkCurve;

    protected HandChirality cursorChirality = HandChirality.RIGHT;
    protected bool lineShrunk;
    protected float lineWidth;
    protected Coroutine lineScalingRoutine;

    private float storedGrabStrengthForGizmos = 0f;

    void Start()
    {
        SetLineRendererWidthScale(1);
    }

    protected override void OnConfigUpdated()
    {
        base.OnConfigUpdated();

        cursorDotSize *= 0.6f;

        UpdateLineRenderers(0f);
        SetLineRendererWidthScale(1);

        Color fillColour = ScreenControlUtility.ParseColor(SettingsConfig.Config.CursorRingColor, SettingsConfig.Config.CursorRingOpacity);
        Color borderColour = ScreenControlUtility.ParseColor(SettingsConfig.Config.CursorDotBorderColor, SettingsConfig.Config.CursorDotBorderOpacity);

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(fillColour, 0.0f), new GradientColorKey(fillColour, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(fillColour.a, 0.0f), new GradientAlphaKey(fillColour.a, 1.0f) }
        );
        lineRenderer.colorGradient = gradient;

        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(borderColour, 0.0f), new GradientColorKey(borderColour, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(borderColour.a, 0.0f), new GradientAlphaKey(borderColour.a, 1.0f) }
        );

        lineRendererOutline.colorGradient = gradient;
    }

    protected override void OnHandleInputAction(InputActionData _inputData)
    {
        InputType _type = _inputData.Type;
        Vector2 _cursorPosition = _inputData.CursorPosition;
        Vector2 _clickPosition = _inputData.ClickPosition;

        base.OnHandleInputAction(_inputData);

        if(_inputData.Chirality != cursorChirality && _inputData.Chirality != HandChirality.UNKNOWN)
        {
            cursorChirality = _inputData.Chirality;
        }

        switch (_type)
        {
            case InputType.MOVE:
                UpdateLineRenderers(_inputData.ProgressToClick);
                storedGrabStrengthForGizmos = _inputData.ProgressToClick;
                break;
            case InputType.DOWN:
                if (lineShrunk)
                {
                    if (lineScalingRoutine != null)
                    {
                        StopCoroutine(lineScalingRoutine);
                    }
                    lineScalingRoutine = StartCoroutine(GrowLine());
                }
                break;
            case InputType.UP:
            case InputType.HOVER:
                if (!lineShrunk)
                {
                    if (lineScalingRoutine != null)
                    {
                        StopCoroutine(lineScalingRoutine);
                    }
                    lineScalingRoutine = StartCoroutine(ShrinkLine());
                }
                break;
            case InputType.CANCEL:
                break;
        }
    }

    private IEnumerator GrowLine()
    {
        SetLineRendererWidthScale(1);
        lineShrunk = false;
        YieldInstruction yieldInstruction = new YieldInstruction();
        float elapsedTime = 0.0f;

        while (elapsedTime < LineGrowthSeconds)
        {
            yield return yieldInstruction;
            elapsedTime += Time.deltaTime;
            float scale = ScreenControlUtility.MapRangeToRange(lineGrowCurve.Evaluate(elapsedTime / LineGrowthSeconds), 0, 1, 1, LineGrowthMultiplier);
            SetLineRendererWidthScale(scale);
        }

        lineScalingRoutine = null;
    }

    private IEnumerator ShrinkLine()
    {
        lineShrunk = true;
        YieldInstruction yieldInstruction = new YieldInstruction();
        float elapsedTime = 0.0f;
        while (elapsedTime < LineGrowthSeconds)
        {
            yield return yieldInstruction;
            elapsedTime += Time.deltaTime;
            float scale = ScreenControlUtility.MapRangeToRange(lineShrinkCurve.Evaluate(elapsedTime / LineGrowthSeconds), 0, 1, 1, LineGrowthMultiplier);
            SetLineRendererWidthScale(scale);
        }

        SetLineRendererWidthScale(1);
        lineScalingRoutine = null;
    }

    private void SetLineRendererWidthScale(float scale)
    {
        lineWidth = cursorDotSize * BaseLineWidthScale * scale * _screenScale;
    }

    private void UpdateLineRenderers(float _grabStrength)
    {
        float arcAngle = (cursorChirality == HandChirality.LEFT) ? ArcAngleLeft : ArcAngleRight;

        List<Vector3> positions = GenerateArcPositions(ArcLengthPercentage, arcAngle, _grabStrength);
        lineRenderer.positionCount = LineRendererPositions;
        lineRenderer.SetPositions(positions.ToArray());
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        positions = GenerateArcPositions(OutlineArcLengthPercentage, arcAngle, _grabStrength);
        lineRendererOutline.positionCount = LineRendererPositions;
        lineRendererOutline.SetPositions(positions.ToArray());
        lineRendererOutline.startWidth = lineWidth * BaseOutlineWidthScale;
        lineRendererOutline.endWidth = lineWidth * BaseOutlineWidthScale;
    }

    List<Vector3> GenerateArcPositions(float _arcLengthPercentage, float _arcAngle, float _grabStrength)
    {
        float circleCentreOffsetBase = cursorDotSize * CircleCentreOffsetBaseScale * _screenScale;
        float arcMidpointRadius = cursorDotSize * ArcMidpointOffsetScale * _screenScale;
        float currentCircleOffsetRadius = ScreenControlUtility.MapRangeToRange(radiusScale.Evaluate(_grabStrength), 1, 0, arcMidpointRadius / 2, circleCentreOffsetBase);

        Vector2 anchoredPosition = Camera.main.ScreenToWorldPoint(cursorTransform.anchoredPosition);

        Vector2 arcOffset = new Vector2()
        {
            x = arcMidpointRadius * Mathf.Cos(Mathf.Deg2Rad * _arcAngle),
            y = arcMidpointRadius * Mathf.Sin(Mathf.Deg2Rad * _arcAngle)
        };

        Vector2 currentCircleOffset = new Vector2()
        {
            x = currentCircleOffsetRadius * Mathf.Cos(Mathf.Deg2Rad * (_arcAngle + 180)),
            y = currentCircleOffsetRadius * Mathf.Sin(Mathf.Deg2Rad * (_arcAngle + 180))
        };

        Vector3 arcMidpoint = new Vector3()
        {
            x = anchoredPosition.x + arcOffset.x,
            y = anchoredPosition.y + arcOffset.y,
            z = transform.position.z,
        };

        Vector3 circleCentre = new Vector3()
        {
            x = arcMidpoint.x + currentCircleOffset.x,
            y = arcMidpoint.y + currentCircleOffset.y,
            z = arcMidpoint.z,
        };

        float finalOffsetRadius = arcMidpointRadius / 2;
        float arcLength = (float)(2 * Math.PI * finalOffsetRadius) * _arcLengthPercentage;

        Vector2 delta = (circleCentre - arcMidpoint);
        float thetaMidpoint = (Mathf.Deg2Rad * 180) + Mathf.Atan2(delta.y, delta.x);

        float thetaMin = thetaMidpoint - ((arcLength / 2) / currentCircleOffsetRadius);
        float thetaMax = thetaMidpoint + ((arcLength / 2) / currentCircleOffsetRadius);

        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i < LineRendererPositions; i++)
        {
            float currentTheta = thetaMin + (i * (thetaMax - thetaMin) / LineRendererPositions);
            positions.Add(new Vector3()
            {
                x = circleCentre.x + Mathf.Cos(currentTheta) * currentCircleOffsetRadius,
                y = circleCentre.y + Mathf.Sin(currentTheta) * currentCircleOffsetRadius,
                z = circleCentre.z
            });
        }
        return positions;
    }

    private void OnDrawGizmos()
    {
        if (Application.isEditor && Application.isPlaying && DrawDebugGizmos)
        {
            float arcAngle = (cursorChirality == HandChirality.LEFT) ? ArcAngleLeft : ArcAngleRight;
            float circleCentreOffsetBase = cursorDotSize * CircleCentreOffsetBaseScale * _screenScale;
            float arcMidpointRadius = cursorDotSize * ArcMidpointOffsetScale * _screenScale;
            float currentCircleOffsetRadius = ScreenControlUtility.MapRangeToRange(radiusScale.Evaluate(storedGrabStrengthForGizmos), 1, 0, arcMidpointRadius / 2, circleCentreOffsetBase);
            float finalOffsetRadius = arcMidpointRadius / 2;

            Vector2 anchoredPosition = Camera.main.ScreenToWorldPoint(_targetPos);

            Vector2 arcOffset = new Vector2()
            {
                x = arcMidpointRadius * Mathf.Cos(Mathf.Deg2Rad * arcAngle),
                y = arcMidpointRadius * Mathf.Sin(Mathf.Deg2Rad * arcAngle)
            };

            Vector2 currentCircleOffset = new Vector2()
            {
                x = currentCircleOffsetRadius * Mathf.Cos(Mathf.Deg2Rad * (arcAngle + 180)),
                y = currentCircleOffsetRadius * Mathf.Sin(Mathf.Deg2Rad * (arcAngle + 180))
            };

            Vector2 finalOffset = new Vector2()
            {
                x = finalOffsetRadius * Mathf.Cos(Mathf.Deg2Rad * (arcAngle + 180)),
                y = finalOffsetRadius * Mathf.Sin(Mathf.Deg2Rad * (arcAngle + 180))
            };

            Vector3 arcMidpoint = new Vector3()
            {
                x = anchoredPosition.x + arcOffset.x,
                y = anchoredPosition.y + arcOffset.y,
                z = transform.position.z,
            };

            Vector3 circleCentre = new Vector3()
            {
                x = arcMidpoint.x + currentCircleOffset.x,
                y = arcMidpoint.y + currentCircleOffset.y,
                z = arcMidpoint.z,
            };

            Vector3 finalCircleCentre = new Vector3()
            {
                x = arcMidpoint.x + finalOffset.x,
                y = arcMidpoint.y + finalOffset.y,
                z = arcMidpoint.z,
            };

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(circleCentre, currentCircleOffsetRadius);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(finalCircleCentre, finalOffsetRadius);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(Camera.main.ScreenToWorldPoint(_targetPos), 0.1f);
        }
    }

    public override void ShowCursor()
    {
        base.ShowCursor();
        lineRenderer.enabled = true;
        lineRendererOutline.enabled = true;
    }

    public override void HideCursor()
    {
        base.HideCursor();
        lineRenderer.enabled = false;
        lineRendererOutline.enabled = false;
    }

    public override void SetScreenScale(float _scale)
    {
        base.SetScreenScale(_scale);
        SetLineRendererWidthScale(1);
    }
}