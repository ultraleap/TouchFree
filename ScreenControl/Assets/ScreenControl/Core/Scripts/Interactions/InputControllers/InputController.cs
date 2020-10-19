using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// A base Input Controller to derive from. This base class handles moving the standard cursor.
/// </summary>
public class InputController : BaseInput
{
    public static InputController Instance;

    public bool allowInteractions = true;

    protected override void Start()
    {
        base.Start();

        Instance = this;
        InteractionManager.HandleInputAction += HandleInputAction;
        ConfigurationSetupController.EnableInteractions += EnableInteraction;
        ConfigurationSetupController.DisableInteractions += DisableInteraction;
    }

    protected virtual void EnableInteraction()
    {
        allowInteractions = true;
    }

    protected virtual void DisableInteraction()
    {
        allowInteractions = false;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        InteractionManager.HandleInputAction -= HandleInputAction;
        ConfigurationSetupController.EnableInteractions -= EnableInteraction;
        ConfigurationSetupController.DisableInteractions -= DisableInteraction;
    }

    protected virtual void HandleInputAction(InputActionData _inputData)
    {
        switch (_inputData.Type)
        {
            case InputType.MOVE:
                break;
            case InputType.DOWN:
                break;
            case InputType.HOLD:
                break;
            case InputType.DRAG:
                break;
            case InputType.UP:
                break;
            case InputType.HOVER:
                break;
            case InputType.CANCEL:
                break;
        }
    }
}