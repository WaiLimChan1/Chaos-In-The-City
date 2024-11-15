using CITC.GameManager;
using UnityEngine;

public class CharacterHitbox : MonoBehaviour
{
    protected Character _character;

    private void Awake()
    {
        _character = GetComponentInParent<Character>();
    }

    protected virtual void GotHitLocalEffects(SingleAnimationClipManager.SingleAnimationType singleAnimationType, Vector3 position, Transform visualEffectParent = null) 
    {
        LocalSpawnerManager.CreateVisualEffectInstance(singleAnimationType, position, parent : visualEffectParent);
    }
    protected virtual void BulletCollisionLogic(Bullet bullet) {}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!Character.CanUse(_character)) return;
        if (_character.Runner.IsResimulation) return;
        if (_character.IsDead) return;

        Bullet bullet = collision.gameObject.GetComponent<Bullet>();
        if (bullet != null && !bullet.Finished)
        {
            if (bullet.IsOwner(_character)) return;
            BulletCollisionLogic(bullet);
        }
    }

    public void TakeHit(SingleAnimationClipManager.SingleAnimationType singleAnimationType, Vector3 localEffectsPosition, float damage, Vector2 knockback, Transform visualEffectParent = null)
    {
        if (!Character.CanUse(_character)) return;
        if (_character.Runner.IsResimulation) return;
        if (_character.IsDead) return;

        GotHitLocalEffects(singleAnimationType, localEffectsPosition, visualEffectParent);

        if (!_character.Runner.IsServer) return;
        _character.TakeDamage(damage, knockback);
    }
}
