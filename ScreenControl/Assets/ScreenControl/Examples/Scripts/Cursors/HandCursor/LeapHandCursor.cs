using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeapHandCursor : Ultraleap.ScreenControl.Client.Cursor
{
    public struct HandCursorData
    {
        public HandChirality chirality;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 offsetDirection;
        public RaycastHit raycastHit;
        public float offsetDistance;
        public float grabReleaseLerpSpeed;
        public Color colour;
        public Color targetColour;
        public bool grabbing;
        public bool poking;
    }


    public Camera cursorCamera;

    [Header("Hand Properties")]
    public HandCursorPostProcessor handProcessor;
    public CustomCapsuleHand leftHand;
    public CustomCapsuleHand rightHand;
    public Transform leftHandTransform;
    public Transform rightHandTransform;


    [Header("Hand Colour")]
    public Color baseColour = Color.grey;
    public Color grabColour = Color.magenta;
    public Color pokeColour = Color.cyan;
    private Color currentColour = Color.white;
    private Color targetColour = Color.white;

    [Range(0.5f, 10f)] public float colorLerpSpeed = 1;


    [Header("Interaction Animation")]
    public float maxHandScale = 10;
    public float maxHandDistance = 20;
    [Range(0,1)]public float maxPokeOffset;
    [Range(0,1)]public float maxGrabOffset;
    [Range(0,10)] public float baseOffset;

    [Range(0.5f, 10f)] public float pokeLerpSpeed = 1;

    // Private Properties
    [HideInInspector] public  bool grabbing = false;
    [HideInInspector] public RaycastHit raycastHit;

    private HandCursorData leftHandData;
    private HandCursorData rightHandData;
    private LayerMask projectionMask;


    private const float grabReleaseLerpMax = 60f;
    private const float grabReleaseLerpIncremenet = 1f;
    private const float grabReleaseLerpMin = 0;


    protected override void OnEnable()
    {
        base.OnEnable();
        InteractionManager.HandleInputAction += OnHandlePrimaryAction;
        InteractionManager.HandleInputActionLeftHand += OnHandleLeftInputAction;
        InteractionManager.HandleInputActionRightHand += OnHandleRightInputAction;

        if (cursorCamera == null) cursorCamera = Camera.main;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        InteractionManager.HandleInputActionLeftHand -= OnHandleLeftInputAction;
        InteractionManager.HandleInputActionRightHand -= OnHandleRightInputAction;
        InteractionManager.HandleInputAction -= OnHandlePrimaryAction;
    }

    // Start is called before the first frame update
    void Start()
    {
        handProcessor.leftCursor = leftHandTransform;
        handProcessor.rightCursor = rightHandTransform;
        projectionMask = LayerMask.NameToLayer("HandCursor");
        currentColour = baseColour;
        targetColour = baseColour;

        leftHandData = InitHandData(HandChirality.LEFT);
        rightHandData = InitHandData(HandChirality.RIGHT);
    }


    protected override void Update()
    {
        base.Update();
        UpdateHandColours();
    }

    protected void OnHandlePrimaryAction(InputActionData _inputData)
    {
        if (_inputData.Type == InputType.MOVE)
        {
            handProcessor.leftHandActive = _inputData.Chirality == HandChirality.LEFT;
            handProcessor.rightHandActive = _inputData.Chirality == HandChirality.RIGHT;
        }
    }

    protected void OnHandleLeftInputAction(InputActionData _inputData)
    {
        leftHandData = UpdateHand(_inputData, leftHandData, leftHandTransform);
        handProcessor.rightHandActive = true;
    }

    protected void OnHandleRightInputAction(InputActionData _inputData)
    {
        rightHandData = UpdateHand(_inputData, rightHandData, rightHandTransform);
        handProcessor.leftHandActive = true;
    }


    private HandCursorData UpdateHand(InputActionData _inputData, HandCursorData handData, Transform handTransform)
    {
        switch(_inputData.Type)
        {
            case InputType.MOVE:
                // Animate the offset based on the interaction
                handData.colour = Color.Lerp(handData.colour, handData.targetColour, colorLerpSpeed * Time.deltaTime);

                PlaceHandTransform(_inputData.CursorPosition, handTransform, ref handData);
                OffsetHand(ref handData, _inputData.CursorPosition, _inputData.ProgressToClick);
                ScaleHandByDistance(handData.raycastHit);
                break;

            case InputType.DOWN:
                if (_inputData.Source == InteractionType.Grab)
                {
                    handData.grabbing = true;
                    handData.grabReleaseLerpSpeed = 0;
                    handData.targetColour = grabColour;
                }
                else if (_inputData.Source == InteractionType.Push && !handData.grabbing)
                {
                    handData.targetColour = pokeColour;
                }
                break;

            case InputType.UP:
                if (_inputData.Source == InteractionType.Grab)
                {
                    handData.grabbing = false;
                }
                handData.targetColour = baseColour;
                break;

            case InputType.CANCEL:
                if (_inputData.Source == InteractionType.Grab)
                {
                    handData.grabbing = false;
                }
                handData.targetColour = baseColour;
                break;
        }

        return handData;
    }


    private void PlaceHandTransform(Vector2 cursorPos, Transform hand, ref HandCursorData handData)
    {

        if (!handData.grabbing)
        {
            handData.grabReleaseLerpSpeed = Mathf.Clamp(handData.grabReleaseLerpSpeed + grabReleaseLerpIncremenet, grabReleaseLerpMin, grabReleaseLerpMax);
        }
        handData.raycastHit = ProjectedCursorHit(Vector2.Lerp(cursorCamera.WorldToScreenPoint(handData.raycastHit.point), cursorPos, handData.grabReleaseLerpSpeed * Time.deltaTime));

        hand.position = handData.raycastHit.point;
        hand.rotation = cursorCamera.transform.rotation;
    }

    private void OffsetHand(ref HandCursorData _handData, Vector2 _cursorPos, float _progressToClick)
    {
        // Work out the offset from the raycast hit to where we want the hand
        _handData.offsetDirection = cursorCamera.ScreenPointToRay(_cursorPos).direction;
        _handData.offsetDistance = Mathf.Lerp(baseOffset, maxPokeOffset, _progressToClick);

        if (_handData.chirality == HandChirality.LEFT)
        {
            handProcessor.leftOffsetVector = -_handData.offsetDirection * _handData.offsetDistance;
        }
        else
        {
            handProcessor.rightOffsetVector = -_handData.offsetDirection * _handData.offsetDistance;
        }
    }

    /// <Summary>
    /// Project from a position on the screen into the scene and return the coordinates of the first hit
    /// </Summary>
    private RaycastHit ProjectedCursorHit(Vector3 screenPos)
    {
        var hit = new RaycastHit();

        if (!Physics.Raycast(cursorCamera.ScreenPointToRay(screenPos), out hit, 200, projectionMask))
        {
            hit.point = cursorCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 1f));
        };

        return hit;
    }

    private Quaternion CursorRotation()
    {
        var hitA = new RaycastHit();
        var hitB = new RaycastHit();
        if (Physics.Raycast(cursorCamera.ViewportPointToRay(new Vector3(0.5f,0.5f,0)), out hitA, projectionMask) &&
            Physics.Raycast(cursorCamera.ViewportPointToRay(new Vector3(0.5f,0.8f,0)), out hitB, projectionMask))
        {
            return Quaternion.LookRotation(hitB.point - hitA.point, hitA.normal);
        }
        else
        {
            return cursorCamera.transform.rotation;
        }

    }

    public void ScaleHandByDistance(RaycastHit hit)
    {
        var distanceToHand = handProcessor.handScale = hit.distance;
        SetHandScale(Mathf.Lerp(0.5f, maxHandScale, distanceToHand / maxHandDistance));
    }

    public void SetHandScale(float value)
    {
        handProcessor.handScale = Mathf.Max(0.001f, value);
        rightHand.handScale = handProcessor.rightHandActive ? handProcessor.handScale : 0;
        leftHand.handScale = handProcessor.leftHandActive ? handProcessor.handScale : 0;
    }

    public void SetLeftHandColour(Color colour)
    {
        leftHand.SetBoneColour(colour);
        leftHand.SetJointColour(colour);
    }
    public void SetRightHandColour(Color colour)
    {
        rightHand.SetBoneColour(colour);
        rightHand.SetJointColour(colour);
    }

    public void UpdateHandColours()
    {
        if (handProcessor.leftHandActive) SetLeftHandColour(leftHandData.colour);
        if (handProcessor.rightHandActive) SetRightHandColour(rightHandData.colour);
    }

    private HandCursorData InitHandData(HandChirality handedness)
    {
        var newHand = new HandCursorData();
        newHand.chirality = handedness;
        newHand.position = Vector3.zero;
        newHand.rotation = Quaternion.identity;
        newHand.offsetDirection = Vector3.zero;
        newHand.offsetDistance = baseOffset;
        newHand.grabReleaseLerpSpeed = 60;
        newHand.colour = baseColour;
        newHand.targetColour = baseColour;
        newHand.grabbing = false;
        newHand.poking = false;

        return newHand;
    }
}
