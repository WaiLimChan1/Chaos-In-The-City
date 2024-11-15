using CITC.GameManager;
using System.Collections.Generic;
using UnityEngine;
using static CITC.GameManager.SingleAnimationClipManager;

public class SingleAnimationController : AnimationController
{
    public void ChangeAnimationClips(SingleAnimationType singleAnimationType)
    {
        AnimationClip animationClip = SingleAnimationClipManager.GetAnimationClip(singleAnimationType);

        // Create a list to hold the overrides
        List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();

        // Get the current overrides
        animationOverrideController.GetOverrides(overrides);

        // Override the animation clips
        for (int i = 0; i < overrides.Count; i++)
        {
            if (overrides[i].Key.name == "Single Animation") overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, animationClip);
        }

        // Apply the overrides
        animationOverrideController.ApplyOverrides(overrides);
    }
}
