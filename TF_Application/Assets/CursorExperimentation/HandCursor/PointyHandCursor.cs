using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ultraleap.TouchFree.Tooling;
using Ultraleap.TouchFree.Tooling.Connection;

[RequireComponent(typeof(Animator))]
public class PointyHandCursor : MonoBehaviour
{
    public SkinnedMeshRenderer handMesh;
    public HandChirality chirality;
    Animator animator;
    bool down = false;
    bool dragging = false;
    float startDistance = 0;

    public float handLostTimer = 3;

    void Start()
    {
        InputActionManager.TransmitInputAction += HandleInputAction;
        ConnectionManager.HandFound += HandFound;
        ConnectionManager.HandsLost += HandsLost;
        animator = GetComponent<Animator>();
    }

    public void HandleInputAction(InputAction inputAction)
    {
        animator.SetFloat("Progress", inputAction.ProgressToClick);
        animator.SetFloat("Distance", 1 - (startDistance - inputAction.DistanceFromScreen) * 10);
        handMesh.enabled = inputAction.Chirality == chirality;

        switch (inputAction.InputType)
        {
            case InputType.UP:
                animator.SetTrigger("Up");
                down = false;
                dragging = false;
                break;

            case InputType.DOWN:
                down = true;
                startDistance = inputAction.DistanceFromScreen;
                break;

            case InputType.MOVE:
                if (down && !dragging)
                {
                    dragging = true;
                    animator.ResetTrigger("Up");
                    animator.SetTrigger("Drag");
                }
                break;
        }
        if (inputAction.InputType == InputType.UP)
        {
            animator.ResetTrigger("Drag");
            animator.SetTrigger("Up");
            down = false;
        }
        if (inputAction.InputType == InputType.DOWN)
        {
            down = true;
        }
    }

    public void HandFound()
    {
        animator.SetFloat("Progress", 0);
        animator.ResetTrigger("HandsLost");
        animator.SetTrigger("HandFound");

        if (ctiChangeCoroutine != null)
        {
            StopCoroutine(ctiChangeCoroutine);
        }
    }

    public void HandsLost()
    {
        if (ctiChangeCoroutine != null)
        {
            StopCoroutine(ctiChangeCoroutine);
        }

        ctiChangeCoroutine = StartCoroutine(ChangeToCTIAfterWait());
    }


    Coroutine ctiChangeCoroutine;
    IEnumerator ChangeToCTIAfterWait()
    {
        animator.ResetTrigger("HandFound");

        yield return new WaitForSeconds(handLostTimer);

        ctiChangeCoroutine = null;
        animator.SetFloat("Progress", 0);
        animator.SetTrigger("HandsLost");
    }
}
