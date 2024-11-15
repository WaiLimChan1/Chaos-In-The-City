using System;
using UnityEngine;
using static CITC.GameManager.GameObjectListManager;

namespace CITC.GameManager
{
    public class OutfitManager : MonoBehaviour
    {
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Serializable Classes
        [Serializable]
        public class IdleWalkAnimationCombo
        {
            public string Name;
            public AnimationClip Idle;
            public AnimationClip Walk;
        }

        [Serializable]
        public class Outfit
        {
            public int BackpackIndex;
            public int BodyIndex;
            public int ShirtIndex;
            public int HairIndex;
            public int MustacheIndex;
            public int MaskIndex;

            public void CopyFrom(OutfitNetworked other)
            {
                BackpackIndex = other.BackpackIndex;
                BodyIndex = other.BodyIndex;
                ShirtIndex = other.ShirtIndex;
                HairIndex = other.HairIndex;
                MustacheIndex = other.MustacheIndex;
                MaskIndex = other.MaskIndex;
            }

            public void RandomizeOutfit(int bodyIndex)
            {
                BodyIndex = bodyIndex;

                OutfitManager outfitManager = OutfitManager.Instance;

                BackpackIndex = UnityEngine.Random.Range(-1, outfitManager.BackpackOutfits.Length);
                ShirtIndex = UnityEngine.Random.Range(-1, outfitManager.ShirtOutfits.Length);
                HairIndex = UnityEngine.Random.Range(-1, outfitManager.HairOutfits.Length);
                MustacheIndex = UnityEngine.Random.Range(-1, outfitManager.MustacheOutfits.Length);
                MaskIndex = UnityEngine.Random.Range(-1, outfitManager.MaskOutfits.Length);
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------



        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Static Variables
        public const int NUM_OF_OUTFIT_TYPES = 6;
        public static OutfitManager Instance { get; private set; }
        //------------------------------------------------------------------------------------------------------------------------------------------------



        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Member Variables
        [Header("Outfit Animation Combos")]
        [SerializeField] public IdleWalkAnimationCombo[] BodyOutfits;

        [Header("Outfit Sprites")]
        [SerializeField] public Sprite[] BackpackOutfits;
        [SerializeField] public Sprite[] ShirtOutfits;
        [SerializeField] public Sprite[] HairOutfits;
        [SerializeField] public Sprite[] MustacheOutfits;
        [SerializeField] public Sprite[] MaskOutfits;
        //------------------------------------------------------------------------------------------------------------------------------------------------



        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Initialization
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
