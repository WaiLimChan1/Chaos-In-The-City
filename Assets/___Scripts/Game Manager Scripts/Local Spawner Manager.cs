using UnityEngine;
using static BulletCasing;
using static CITC.GameManager.SingleAnimationClipManager;

namespace CITC.GameManager
{
    public class LocalSpawnerManager : MonoBehaviour
    {
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Static Variables
        public static LocalSpawnerManager Instance { get; private set; }
        //------------------------------------------------------------------------------------------------------------------------------------------------



        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Member Variables
        [Header("Local Prefabs")]
        [SerializeField] private GameObject _bulletCasingPrefab;
        [SerializeField] private GameObject _visualEffectPrefab;
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
        //Static Spawn Functions
        static public GameObject CreateBulletCasingInstance(BulletCasingType bulletCasingType, Vector3 position)
        {
            GameObject bulletCasing = Instantiate(Instance._bulletCasingPrefab);
            bulletCasing.GetComponent<BulletCasing>().SetUp(bulletCasingType, position);
            return bulletCasing;
        }

        static public GameObject CreateVisualEffectInstance(SingleAnimationType singleAnimationType, Vector3 position, float scale = 1, float rotationDegree = 0, bool flipY = false, int sortingOrder = 0, Transform parent = null)
        {
            if (singleAnimationType == SingleAnimationType.None) return null;

            GameObject visualEffect = Instantiate(Instance._visualEffectPrefab);
            visualEffect.GetComponent<MyVisualEffect>().SetUp(singleAnimationType, position, scale, rotationDegree, flipY, sortingOrder, parent);
            return visualEffect;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
    }
}