using System.Collections.Generic;
using UnityEngine;

namespace CITC.GameManager
{
    public class AnimationCullingManager : MonoBehaviour
    {
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Static Variables
        public static AnimationCullingManager Instance { get; private set; }
        //------------------------------------------------------------------------------------------------------------------------------------------------



        //------------------------------------------------------------------------------------------------------------------------------------------------
        [Header("Components")]
        private GameObjectListManager _gameObjectListManager;

        [Header("Variables")]
        [SerializeField] private bool activeAnimationCulling;
        //------------------------------------------------------------------------------------------------------------------------------------------------



        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Initialization
        private void SetUp()
        {
            _gameObjectListManager = GameObjectListManager.Instance;
            //activeAnimationCulling = true;
            activeAnimationCulling = false; //For testing, just to see all of the zombies
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                SetUp();
            }
            else Destroy(this);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------



        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Logic Functions
        private void SetActivateAllCharacters(List<GameObject> list, bool activeValue)
        {
            Character character = null;
            foreach (GameObject gameObject in list)
            {
                character = gameObject.GetComponent<Character>();
                if (!Character.CanUse(character)) return;
                character.ActiveAnimation = activeValue;
            }
        }

        private void UpdateAllCharacterVisibility()
        {
            if (activeAnimationCulling)
            {
                SetActivateAllCharacters(_gameObjectListManager.AllVisibleCharacters, true);
                SetActivateAllCharacters(_gameObjectListManager.AllNonVisibleCharacters, false);
            }
            else
            {
                SetActivateAllCharacters(_gameObjectListManager.AllCharacters, true);
            }
        }

        private void LateUpdate()
        {
            UpdateAllCharacterVisibility();
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
