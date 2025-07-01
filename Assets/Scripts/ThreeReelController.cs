using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ThreeReelController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public SlotMachineReel leftReel;
    public SlotMachineReel centerReel;
    public SlotMachineReel rightReel;
    public RectTransform leverHandle;  // Reference to the lever handle's RectTransform
    public float delayBetweenReels = 0.2f;
    public int leftReelIndex = -1;
    public int middleReelIndex = -1;
    public int rightReelIndex = -1;
    public BattleSystem battleScript;
    [Header("Lever Settings")]
    public float maxPullAngle = 45f;
    public float returnSpeed = 5f;
    public float spinThreshold = 30f;
    public float returnEasingStrength = 2f; // Higher values = stronger easing effect
    public bool canSpin = true; // Controls whether the lever can trigger a spin

    private bool isSpinning = false;
    private bool isGrabbed = false;
    private bool hasTriggeredSpin = false;
    private float currentRotation = 0f;
    private Vector2 initialGrabPosition;
    private Vector2 leverPivotPosition;
    private float returnStartRotation = 0f; // Starting point for the return animation

     

    void Start()
    {
        // Keep the lever's initial orientation, just get its position
        if (leverHandle != null)
        {
            leverPivotPosition = leverHandle.position;
            currentRotation = 0; // Start with no added rotation
        }
    }

    void Update()
    {
        // If the lever isn't being grabbed, return it to the starting position with quadratic easing
        if (!isGrabbed && currentRotation < 0)
        {
            // Calculate how far through the return animation we are (0 = just released, 1 = back to start)
            float rotationToGo = Mathf.Abs(currentRotation);
            float totalRotationToReturn = Mathf.Abs(returnStartRotation);

            // If we have a valid starting point
            if (totalRotationToReturn > 0.01f)
            {
                // Calculate progress (0 to 1)
                float progress = 1f - (rotationToGo / totalRotationToReturn);

                // Apply quadratic easing - faster at the start, slowing down at the end
                float easedSpeed = returnSpeed * (1f + (returnEasingStrength * (1f - progress)));

                // Apply the eased return speed
                currentRotation += easedSpeed * Time.deltaTime;
            }
            else
            {
                // Fallback if we don't have a valid starting point
                currentRotation += returnSpeed * Time.deltaTime;
            }

            // Clamp to make sure we don't overshoot
            currentRotation = Mathf.Min(currentRotation, 0);

            // Apply rotation
            if (leverHandle != null)
            {
                leverHandle.localRotation = Quaternion.Euler(0, 0, currentRotation);
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isSpinning || !canSpin) return;

        // Store the initial grab position
        initialGrabPosition = eventData.position;
        leverPivotPosition = leverHandle.position;

        isGrabbed = true;
        hasTriggeredSpin = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isGrabbed || isSpinning || !canSpin) return;

        // Get vector from pivot to current position
        Vector2 currentDirection = eventData.position - leverPivotPosition;

        // Calculate distance from pivot along the vertical axis (normalized to 0-1 range)
        float pullAmount = Mathf.Clamp01(Vector2.Distance(initialGrabPosition, eventData.position) / 100f);

        // Calculate rotation based on pull amount (negative to rotate the other way)
        float dragAngle = -pullAmount * maxPullAngle;

        // Clamp to allowed range (negative values for opposite rotation)
        currentRotation = Mathf.Clamp(dragAngle, -maxPullAngle, 0);

        // Apply rotation
        if (leverHandle != null)
        {
            leverHandle.localRotation = Quaternion.Euler(0, 0, currentRotation);
        }

        // Check if we've pulled the lever far enough to trigger spin
        if (!hasTriggeredSpin && currentRotation <= -spinThreshold)
        {
            hasTriggeredSpin = true;
            SpinAllReels();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isGrabbed)
        {
            isGrabbed = false;
            // Store the rotation at the moment of release for smooth easing
            returnStartRotation = currentRotation;
        }
    }

    public void SpinAllReels()
    {
        if (isSpinning || !canSpin) return;
        battleScript.onSpin();
        isSpinning = true;
        StartCoroutine(SpinReelsWithDelay());
    }

    private IEnumerator SpinReelsWithDelay()
    {
        // Spin reels with a delay between each
        leftReel.SpinWithRandomResult();
        yield return new WaitForSeconds(delayBetweenReels);
        centerReel.SpinWithRandomResult();
        yield return new WaitForSeconds(delayBetweenReels);
        rightReel.SpinWithRandomResult();

        // Wait for spinning to complete
        float spinDuration = 7.2f;
        yield return new WaitForSeconds(spinDuration);

        // Re-enable lever and check results
        isSpinning = false;
        hasTriggeredSpin = false;
        EvaluateResults();
    }

    private void EvaluateResults()
    {
        // Get the center symbol from each reel
        leftReelIndex = leftReel.GetCenterSymbolIndex();
        middleReelIndex = centerReel.GetCenterSymbolIndex();
        rightReelIndex = rightReel.GetCenterSymbolIndex();

        Debug.Log($"Results: {leftReelIndex} | {middleReelIndex} | {rightReelIndex}");

        // Check for winning combinations
        if (leftReelIndex == middleReelIndex && middleReelIndex == rightReelIndex)
        {
            Debug.Log("Winner! All symbols match!");
        }
    }

    // Method to spin with specific results (for testing or rigging the game)
    public void SpinWithSpecificResults(int leftResult, int centerResult, int rightResult)
    {
        if (isSpinning || !canSpin) return;
        isSpinning = true;
        StartCoroutine(SpinWithResultsRoutine(leftResult, centerResult, rightResult));
    }

    private IEnumerator SpinWithResultsRoutine(int leftResult, int centerResult, int rightResult)
    {
        // Spin reels with a delay between each
        leftReel.SpinWithResult(leftResult);
        yield return new WaitForSeconds(delayBetweenReels);
        centerReel.SpinWithResult(centerResult);
        yield return new WaitForSeconds(delayBetweenReels);
        rightReel.SpinWithResult(rightResult);

        // Wait for spinning to complete
        float spinDuration = 7.2f;
        yield return new WaitForSeconds(spinDuration);

        // Re-enable lever and check results
        isSpinning = false;
        hasTriggeredSpin = false;
        EvaluateResults();
    }
}