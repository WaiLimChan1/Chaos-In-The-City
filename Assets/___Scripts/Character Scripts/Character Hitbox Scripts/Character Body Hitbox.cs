using CITC.GameManager;

public class CharacterBodyHitbox : CharacterHitbox
{
    protected override void BulletCollisionLogic(Bullet bullet)
    {
        GotHitLocalEffects(SingleAnimationClipManager.SingleAnimationType.BodyShotBlood, bullet.transform.position);

        if (!_character.Runner.IsServer) return;
        _character.TakeDamage(bullet.Damage, bullet.KnockBackForce);
        bullet.HitSomething();
    }
}
