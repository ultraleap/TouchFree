using UnityEngine;

public class CallToInteractValueSetter : MonoBehaviour
{
	public CallToInteractController controller;

	void OnEnable()
	{
		PhysicalConfigurable.OnConfigUpdated += OnPhysicalConfigUpdated;
		CallToInteractConfig.OnConfigUpdated += OnCTIConfigUpdated;
        InteractionManager.HandleInputAction += HandleInteractionModuleInputAction;
        OnCTIConfigUpdated();
	}

	void OnDisable()
	{
		CallToInteractConfig.OnConfigUpdated -= OnCTIConfigUpdated;
        InteractionManager.HandleInputAction -= HandleInteractionModuleInputAction;
    }

	void OnCTIConfigUpdated()
	{
        controller.UpdateCTISettings(CallToInteractConfig.Config.Enabled, CallToInteractConfig.Config.ShowTimeAfterNoHandPresent, 
                                        CallToInteractConfig.Config.HideTimeAfterHandPresent, CallToInteractConfig.Config.CurrentFileName, CallToInteractConfig.Config.hideType);
	}

	void OnPhysicalConfigUpdated()
	{
		controller.RecreateVideoTexture();
	}

    private void HandleInteractionModuleInputAction(InputActionData _inputData)
    {
        if(_inputData.Type == InputType.UP)
        {
            controller.InteractionHappened();
        }
    }
}