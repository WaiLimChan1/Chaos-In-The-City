using System.Collections.Generic;
using UnityEngine;
using CITC.GameManager;

public class IdleAndWalkAnimationController : AnimationController
{
    private int oldAnimationComboIndex = -2;

    public void ChangeAnimationClips(OutfitManager.IdleWalkAnimationCombo[] animationCombos, int animationComboIndex)
    {
        if (oldAnimationComboIndex == animationComboIndex) return;
        else oldAnimationComboIndex = animationComboIndex;

        if (animationComboIndex == -1)
        {
            this.gameObject.SetActive(false);
            return;
        }
        this.gameObject.SetActive(true);

        animationComboIndex = Mathf.Clamp(animationComboIndex, 0, animationCombos.Length - 1);

        // Create a list to hold the overrides
        List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();

        // Get the current overrides
        animationOverrideController.GetOverrides(overrides);

        // Override the animation clips
        for (int i = 0; i < overrides.Count; i++)
        {
            if (overrides[i].Key.name == "Idle") overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, animationCombos[animationComboIndex].Idle);
            else if (overrides[i].Key.name == "Walk") overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, animationCombos[animationComboIndex].Walk);
        }

        // Apply the overrides
        animationOverrideController.ApplyOverrides(overrides);
    }
}
