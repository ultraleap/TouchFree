using UnityEngine;
using System.IO;

public class StandardInteractionSetup : InteractionSetup
{
	public InteractionModule InteractionModule;
	[Tooltip("The Canvas transform to parent the spawned cursor prefab to. Only used if CursorPrefab is set.")]
	public Transform CursorCanvas;
	[Header("Set one of the following, or leave empty:")]
	[Tooltip("The cursor prefab to spawn into the scene under the CursorCanvas. Only set this or CursorGameObject, not both. CursorGameObject overrides the prefab set here.")]
	public GameObject CursorPrefab;
	[Tooltip("The cursor gameobject to enable in the scene. Only set this or CursorPrefab, not both. This property will override the CursorPrefab.")]
	public GameObject CursorSceneObject;
	private GameObject _cursorInstance;

	public override void Initialize()
	{
		InteractionModule.gameObject.SetActive(true);
		if (CursorSceneObject != null)
		{
			CursorSceneObject.SetActive(true);
		}
		else if (CursorPrefab != null)
		{
			_cursorInstance = Instantiate(CursorPrefab, CursorCanvas);
		}
	}

	public override void TearDown()
	{
		InteractionModule.gameObject.SetActive(false);
		if (CursorSceneObject != null)
		{
			CursorSceneObject.SetActive(false);
		}
		else
		{
			if (_cursorInstance != null)
			{
				Destroy(_cursorInstance);
				_cursorInstance = null;
			}
		}
	}
}