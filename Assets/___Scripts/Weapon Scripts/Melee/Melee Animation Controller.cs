using CITC.GameManager;
using System.Collections.Generic;
using UnityEngine;
using static CITC.GameManager.SingleAnimationClipManager;

public class MeleeAnimationController : AnimationController
{
    [Header("Components")]
    private Melee _melee;

    protected new void Awake()
    {
        base.Awake();
        _melee = GetComponentInParent<Melee>();
    }

    public void ChangeAnimationClips(MeleeAttackAnimationType meleeAttackAnimationType)
    {
        AnimationClip animationClip = SingleAnimationClipManager.GetAnimationClip(meleeAttackAnimationType);

        // Create a list to hold the overrides
        List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();

        // Get the current overrides
        animationOverrideController.GetOverrides(overrides);

        // Override the animation clips
        for (int i = 0; i < overrides.Count; i++)
        {
            if (overrides[i].Key.name == "Attack") overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, animationClip);
        }

        // Apply the overrides
        animationOverrideController.ApplyOverrides(overrides);
    }

    public void TriggerAttack()
    {
        _melee.TriggerAttack();
    }
}
