using UnityEngine;


public enum InteractionType
{
    Undefined,
    Push,
    Grab,
    Hover,
}

public class InteractionModule : MonoBehaviour
{
    public virtual InteractionType InteractionType {get;} = InteractionType.Undefined;
    public bool ignoreDragging;
    public PositioningModule positioningModule;
    public bool allowHover;

    public delegate void InputAction(InputActionData _inputData);
    public static event InputAction HandleInputAction;

    protected Positions positions;

    protected void SendInputAction(InputType _type, Vector2 _cursorPosition, Vector2 _clickPosition, float _distanceFromScreen)
    {
        InputActionData actionData = new InputActionData(InteractionType, _type, _cursorPosition, _clickPosition, _distanceFromScreen);
        HandleInputAction?.Invoke(actionData);
    }

    protected virtual void OnEnable()
    {
        SettingsConfig.OnConfigUpdated += OnSettingsUpdated;
        OnSettingsUpdated();
        PhysicalConfigurable.CreateVirtualScreen(PhysicalConfigurable.Config);
        positioningModule.Stabiliser.ResetValues();
    }

    protected virtual void OnDisable()
    {
        SettingsConfig.OnConfigUpdated -= OnSettingsUpdated;
    }

    protected virtual void OnSettingsUpdated()
    {
        ignoreDragging = !SettingsConfig.Config.UseScrollingOrDragging;
        allowHover = SettingsConfig.Config.SendHoverEvents;
        positioningModule.Stabiliser.defaultDeadzoneRadius = SettingsConfig.Config.DeadzoneRadius;
    }
}