using UnityEngine;

namespace CITC.GameManager
{
    public class SingleAnimationClipManager : MonoBehaviour
    {
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Enum
        public enum SingleAnimationType
        {
            None,
            GunMuzzleFx,
            PistolMuzzleFx,
            AssaultRifleMuzzleFx,
            ShotGunMuzzleFx,
            SniperMuzzleFx,

            ExplosionFx,

            PunchVFX,
            GetPunchedVFX,
            ZombieSlashVFX,

            HeadShotBlood,
            BodyShotBlood,
            DeathBlood
        }

        public enum MeleeAttackAnimationType
        {
            Slash
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------



        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Static Variables
        public static SingleAnimationClipManager Instance { get; private set; }
        //------------------------------------------------------------------------------------------------------------------------------------------------



        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Member Variables
        [Header("Gun Muzzle VFX Animation Clips")]
        [SerializeField] private AnimationClip _gunMuzzleFx;
        [SerializeField] private AnimationClip _pistolMuzzleFx;
        [SerializeField] private AnimationClip _assaultRifleMuzzleFx;
        [SerializeField] private AnimationClip _shotGunMuzzleFx;
        [SerializeField] private AnimationClip _sniperMuzzleFx;

        [Header("Punch VFX Animation Clips")]
        [SerializeField] private AnimationClip _punchVFX;
        [SerializeField] private AnimationClip _getPunchedVFX;
        [SerializeField] private AnimationClip _zombieSlashVFX;

        [Header("Throwable Explosion VFX Animation Clips")]
        [SerializeField] private AnimationClip _explosionFx;

        [Header("Blood VFX Animation Clips")]
        [SerializeField] private AnimationClip _headShotBlood;
        [SerializeField] private AnimationClip _bodyShotBlood;
        [SerializeField] private AnimationClip _deathBlood;

        [Header("Melee VFX Animation Clips")]
        [SerializeField] private AnimationClip _slash;
        //------------------------------------------------------------------------------------------------------------------------------------------------



        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Initialization
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------



        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Static Functions
        static public AnimationClip GetAnimationClip(SingleAnimationType singleAnimationType)
        {
            if (Instance == null) return null;

            //None
            if (singleAnimationType == SingleAnimationType.None) return null;

            //Gun Muzzle VFX
            else if (singleAnimationType == SingleAnimationType.GunMuzzleFx) return Instance._gunMuzzleFx;
            else if (singleAnimationType == SingleAnimationType.PistolMuzzleFx) return Instance._pistolMuzzleFx;
            else if (singleAnimationType == SingleAnimationType.AssaultRifleMuzzleFx) return Instance._assaultRifleMuzzleFx;
            else if (singleAnimationType == SingleAnimationType.ShotGunMuzzleFx) return Instance._shotGunMuzzleFx;
            else if (singleAnimationType == SingleAnimationType.SniperMuzzleFx) return Instance._sniperMuzzleFx;

            //Punch VFX
            else if (singleAnimationType == SingleAnimationType.PunchVFX) return Instance._punchVFX;
            else if (singleAnimationType == SingleAnimationType.GetPunchedVFX) return Instance._getPunchedVFX;
            else if (singleAnimationType == SingleAnimationType.ZombieSlashVFX) return Instance._zombieSlashVFX;

            //Throwable Explosion VFX
            else if (singleAnimationType == SingleAnimationType.ExplosionFx) return Instance._explosionFx;

            //Blood VFX
            else if (singleAnimationType == SingleAnimationType.HeadShotBlood) return Instance._headShotBlood;
            else if (singleAnimationType == SingleAnimationType.BodyShotBlood) return Instance._bodyShotBlood;
            else if (singleAnimationType == SingleAnimationType.DeathBlood) return Instance._deathBlood;

            return null;
        }

        static public AnimationClip GetAnimationClip(MeleeAttackAnimationType meleeAttackAnimationType)
        {
            if (Instance == null) return null;

            //Melee Attack Animations
            if (meleeAttackAnimationType == MeleeAttackAnimationType.Slash) return Instance._slash;

            return null;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
    }
}