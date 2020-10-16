using UnityEngine;
using System.IO;

public class MultiCursorInteractionSetup : InteractionSetup
{
	public InteractionModule InteractionModule;
	[Tooltip("The Canvas transform to parent the spawned cursor prefab to. Only used if CursorPrefab is set.")]
	public Transform CursorCanvas;
	[Header("Set one of the following, or leave empty:")]
	[Tooltip("The cursor prefab to spawn into the scene under the CursorCanvas. Only set this or CursorGameObject, not both. CursorGameObject overrides the prefab set here.")]
	public GameObject CursorPrefab1;
	public GameObject CursorPrefab2;
	private GameObject _cursorInstance1;
	private GameObject _cursorInstance2;

	public override void Initialize()
	{
		InteractionModule.gameObject.SetActive(true);

		if ((CursorPrefab1 != null) && (CursorPrefab2 != null))
		{
			_cursorInstance1 = Instantiate(CursorPrefab1, CursorCanvas);
			_cursorInstance2 = Instantiate(CursorPrefab2, CursorCanvas);
		}
	}

	public override void TearDown()
	{
		InteractionModule.gameObject.SetActive(false);
        if (_cursorInstance1 != null)
        {
            Destroy(_cursorInstance1);
            _cursorInstance1 = null;
        }
        if (_cursorInstance2 != null)
        {
            Destroy(_cursorInstance2);
            _cursorInstance2 = null;
        }
	}
}