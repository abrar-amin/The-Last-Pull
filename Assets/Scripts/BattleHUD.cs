using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour
{
    public Text nameText;
    public Text levelText;
    public Slider hpSlider;

    // Speed of the HP bar animation
    public float hpLerpSpeed = 3f;

    // Store the target HP value
    private float targetHP;
    private float currentDisplayedHP;

    // Flag to determine if animation is in progress
    private bool isAnimating = false;

    public void SetHUD(Unit unit)
    {
        nameText.text = unit.unitName;
        levelText.text = "Lvl " + unit.unitLevel;
        hpSlider.maxValue = unit.maxHP;

        // Set initial values without animation
        hpSlider.value = unit.currentHP;
        targetHP = unit.currentHP;
        currentDisplayedHP = unit.currentHP;
    }

    public void SetHP(int hp)
    {
        // Set the target HP
        targetHP = hp;

        // Start animation if it's not already running
        if (!isAnimating)
        {
            StartCoroutine(AnimateHPChange());
        }
    }

    private IEnumerator AnimateHPChange()
    {
        isAnimating = true;

        // Continue until we reach the target HP
        while (Mathf.Abs(currentDisplayedHP - targetHP) > 0.1f)
        {
            // Lerp towards the target
            currentDisplayedHP = Mathf.Lerp(currentDisplayedHP, targetHP, Time.deltaTime * hpLerpSpeed);

            // Update the slider
            hpSlider.value = currentDisplayedHP;

            yield return null;
        }

        // Ensure we reach exactly the target value
        currentDisplayedHP = targetHP;
        hpSlider.value = currentDisplayedHP;

        isAnimating = false;
    }
}