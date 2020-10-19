using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoHandInteractionSetup : InteractionSetup
{
    /**
    Use the same InteractionModule on both hands.
    Assumes that the GameObject containing the positioning module's PositionStabiliser
    also contains the positioning module's ColliderSnapper.
    Assumes the InteractionModule by default tracks the PRIMARY hand
    */
    public InteractionModule OneHandInteractionModule;
    private InteractionModule InteractionModuleClone;

    public override void Initialize()
    {
        // Set the main interaction module as the Left hand
        OneHandInteractionModule.trackedHand = TrackedHand.LEFT;

        // Clone the interactionModule
        GameObject secondHandObjectClone = Instantiate(OneHandInteractionModule.gameObject);

        // Clone the position stabiliser
        GameObject secondHandStabiliser = Instantiate(OneHandInteractionModule.positioningModule.positioningUtils);

        // Set up appropriately
        InteractionModuleClone = secondHandObjectClone.GetComponent<InteractionModule>();
        
        InteractionModuleClone.positioningModule.positioningUtils = secondHandStabiliser;
        InteractionModuleClone.positioningModule.Stabiliser = secondHandStabiliser.GetComponent<PositionStabiliser>();
        InteractionModuleClone.positioningModule.cursorSnapper = secondHandStabiliser.GetComponent<CursorSnapper>();

        // Set the clone to track the right hand
        InteractionModuleClone.trackedHand = TrackedHand.RIGHT;

        // Set both to be active.
        OneHandInteractionModule.gameObject.SetActive(true);
        InteractionModuleClone.gameObject.SetActive(true);
    }

    public override void TearDown()
    {
        OneHandInteractionModule.trackedHand = TrackedHand.PRIMARY;
        OneHandInteractionModule.gameObject.SetActive(false);
        
        if (InteractionModuleClone != null)
        {
            Destroy(InteractionModuleClone.positioningModule.Stabiliser.gameObject);
            Destroy(InteractionModuleClone.gameObject);
        }
    }
}
