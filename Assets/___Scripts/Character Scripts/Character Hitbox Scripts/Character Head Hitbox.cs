using CITC.GameManager;
using UnityEngine;

public class CharacterHeadHitbox : CharacterHitbox
{
    protected override void BulletCollisionLogic(Bullet bullet)
    {
        GotHitLocalEffects(SingleAnimationClipManager.SingleAnimationType.HeadShotBlood, bullet.transform.position);

        if (!_character.Runner.IsServer) return;
        _character.TakeDamage(bullet.HeadShotDamage, bullet.KnockBackForce);
        bullet.HitSomething();
    }
}
