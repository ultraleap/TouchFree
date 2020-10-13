using System.Linq;
using UnityEngine;

using System.Collections.Generic;

/// <summary>
/// Add InteractionSetup GameObjects as children of this GameObject.
/// Stores them in an array for comparing against when calling SetInteraction(InteractionType) for switching between interaction modules.
/// </summary>
public class InteractionSelector : MonoBehaviour
{
	public InteractionSelection CurrentInteraction
	{
		get
		{
			if (_currentInteraction == InteractionSelection.Null) SetInteraction(SettingsConfig.Config.InteractionSelection);
			return _currentInteraction;
		}
	}

	public InteractionSetup pushInteraction;
	public InteractionSetup pokeInteraction;
	public InteractionSetup grabInteraction;
	public InteractionSetup hoverInteraction;

	private Dictionary<InteractionSelection, InteractionSetup> InteractionSetups;
	private InteractionSelection _currentInteraction = InteractionSelection.Null;


	void Awake()
	{
		InteractionSetups = new Dictionary<InteractionSelection, InteractionSetup>()
		{
			{InteractionSelection.Push, pushInteraction},
			{InteractionSelection.Poke, pokeInteraction},
			{InteractionSelection.Grab, grabInteraction},
			{InteractionSelection.Hover, hoverInteraction},
		};
		
		DisableAll();

		SettingsConfig.OnConfigUpdated += OnConfigUpdated;
		OnConfigUpdated();
	}

	private void OnDestroy()
	{
		SettingsConfig.OnConfigUpdated -= OnConfigUpdated;
	}

	public void SetInteraction(InteractionSelection interactionType)
	{
		var setup = InteractionSetups[interactionType];
		if (setup == null)
		{
			Debug.LogError($"Could not find an InteractionSetup for type {interactionType}. Ensure it is added to the list.");
			if (_currentInteraction == InteractionSelection.Null)
			{
				if (InteractionSetups.Count > 0)
				{
					var toSetUp = InteractionSetups.First();
					Debug.LogWarning($"Reverting to default interaction type: {toSetUp.Value}.");
					SettingsConfig.Config.InteractionSelection = toSetUp.Key;
					SettingsConfig.UpdateConfig(SettingsConfig.Config);
					SettingsConfig.SaveConfig();
					return;
				}
				else
				{
					Debug.LogError("No InteractionSetups could be found to enable! Interaction module state unknown.");
					return;
				}
			}
			else
			{
				Debug.LogWarning($"Keeping current active setup {_currentInteraction}.");
				SettingsConfig.Config.InteractionSelection = _currentInteraction;
				SettingsConfig.UpdateConfig(SettingsConfig.Config);
				SettingsConfig.SaveConfig();
				return;
			}
		}

		if (_currentInteraction != InteractionSelection.Null)
		{
			if (_currentInteraction == interactionType)
			{
				// The specified interaction is already set. Don't need to do anything.
				return;
			}

			InteractionSetups[_currentInteraction].TearDown();
		}

		setup.Initialize();
		_currentInteraction = interactionType;
	}

	private void DisableAll()
	{
		foreach (var element in InteractionSetups)
		{
			element.Value.TearDown();
		}
	}

	private void OnConfigUpdated()
	{
		SetInteraction(SettingsConfig.Config.InteractionSelection);
	}
}