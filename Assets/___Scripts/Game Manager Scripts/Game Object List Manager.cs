using System;
using System.Collections.Generic;
using UnityEngine;

namespace CITC.GameManager
{
    public class GameObjectListManager : MonoBehaviour
    {
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Serializable Classes
        [Serializable]
        public class GameObjectList
        {
            public GameObject Anchor;
            public List<GameObject> List;

            public void SetUp(string anchorName, Transform parentTransform)
            {
                Anchor = new GameObject(anchorName);
                Anchor.transform.parent = parentTransform;
                List = new List<GameObject>();
            }

            public void UpdateList()
            {
                List.Clear();
                foreach (Transform child in Anchor.transform) List.Add(child.gameObject);
            }

            public void RemoveNullOrDefault()
            {
                List.RemoveAll(item => item == null || item == default);
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------



        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Static Variables
        public static GameObjectListManager Instance { get; private set; }
        //------------------------------------------------------------------------------------------------------------------------------------------------



        //------------------------------------------------------------------------------------------------------------------------------------------------
        [Header("Components")]
        private LocalCameraManager _localCameraManager;
        //------------------------------------------------------------------------------------------------------------------------------------------------

        //------------------------------------------------------------------------------------------------------------------------------------------------
        [Header("Game Object Lists")]
        [SerializeField] public GameObjectList AllNetworkedPlayers;
        [SerializeField] public GameObjectList AllPlayerSurvivors;
        [SerializeField] public GameObjectList AllHostileSurvivors;
        [SerializeField] public GameObjectList AllZombies;

        [SerializeField] public GameObjectList AllWeapons;
        [SerializeField] public GameObjectList AllBullets;
        [SerializeField] public GameObjectList AllBulletCasings;
        [SerializeField] public GameObjectList AllThrowableProjectiles;

        [SerializeField] public GameObjectList AllVehicles;

        [SerializeField] public GameObjectList AllVisualEffects;

        [SerializeField] public GameObjectList AllMapIcons;

        [SerializeField] public GameObjectList AllBuildings;
        //------------------------------------------------------------------------------------------------------------------------------------------------

        //------------------------------------------------------------------------------------------------------------------------------------------------
        [Header("Calculated Lists")]
        [SerializeField] public List<GameObject> AllCharacters;
        [SerializeField] public List<GameObject> AllVisibleCharacters;
        [SerializeField] public List<GameObject> AllNonVisibleCharacters;

        [SerializeField] public List<GameObject> AllVisibleVehicles;

        [SerializeField] public List<GameObject> AllStructures;
        [SerializeField] public List<GameObject> AllVisibleStructures;

        [SerializeField] public List<GameObject> AllVisibleGamePlayObjects;
        //------------------------------------------------------------------------------------------------------------------------------------------------



        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Initialization
        private void SetUpComponents()
        {
            _localCameraManager = LocalCameraManager.Instance;
        }

        private void SetUpGameObjectLists()
        {
            AllNetworkedPlayers.SetUp("Networked Player Anchor", this.transform);
            AllPlayerSurvivors.SetUp("Player Survivor Anchor", this.transform);
            AllHostileSurvivors.SetUp("Hostile Survivor Anchor", this.transform);
            AllZombies.SetUp("Zombie Anchor", this.transform);

            AllWeapons.SetUp("Weapon Anchor", this.transform);
            AllBullets.SetUp("Bullet Anchor", this.transform);
            AllBulletCasings.SetUp("Bullet Casing Anchor", this.transform);
            AllThrowableProjectiles.SetUp("Throwable Projectiles Anchor", this.transform);

            AllVehicles.SetUp("Vehicle Anchor", this.transform);

            AllVisualEffects.SetUp("Visual Effect Anchor", this.transform);

            AllMapIcons.SetUp("Map Icon Anchor", this.transform);

            AllBuildings.SetUp("Buildings Anchor", this.transform);
        }

        private void SetUp()
        {
            SetUpComponents();
            SetUpGameObjectLists();
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
        //Helper Functions
        public void Anchor(GameObjectList gameObjectList, GameObject newGameObject, GameObject parent = null)
        {
            if (parent == null) newGameObject.transform.parent = gameObjectList.Anchor.transform;
            else newGameObject.transform.parent = parent.transform;
        }
        public void RecordInList(GameObjectList gameObjectList, GameObject newGameObject)
        {
            gameObjectList.List.Add(newGameObject);
        }

        public void Add(GameObjectList gameObjectList, GameObject newGameObject, GameObject parent = null)
        {
            Anchor(gameObjectList, newGameObject, parent);
            RecordInList(gameObjectList, newGameObject);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------



        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Calculated List
        private void CalculateAllCharactersList()
        {
            AllCharacters.Clear();
            AllCharacters.AddRange(AllPlayerSurvivors.List);
            AllCharacters.AddRange(AllHostileSurvivors.List);
            AllCharacters.AddRange(AllZombies.List);
        }

        private void CalculateAllVisibleAndNonVisibleCharactersList()
        {
            AllVisibleCharacters.Clear();
            AllNonVisibleCharacters.Clear();

            Bounds cameraBounds = LocalCameraManager.Instance.CameraBounds;
            for (int i = 0; i < AllCharacters.Count; i++)
            {
                if (cameraBounds.Contains(AllCharacters[i].transform.position)) AllVisibleCharacters.Add(AllCharacters[i]);
                else AllNonVisibleCharacters.Add(AllCharacters[i]);
            }
        }

        private void CalculateAllVisibleVehicles()
        {
            AllVisibleVehicles.Clear();

            Bounds cameraBounds = LocalCameraManager.Instance.CameraBounds;
            Collider2D[] nearbyVehicles = Physics2D.OverlapBoxAll(cameraBounds.center, cameraBounds.size, 0, LayerMask.GetMask("Vehicle"));

            for (int i = 0; i < nearbyVehicles.Length; i++)
            {
                Vehicle vehicle = nearbyVehicles[i].gameObject.GetComponent<Vehicle>();
                if (!Vehicle.CanUse(vehicle)) continue;
                AllVisibleVehicles.Add(vehicle.gameObject);
            }
        }

        private void CalculateAllStructuresList()
        {
            AllStructures.Clear();
            AllStructures.AddRange(AllBuildings.List);
        }

        private void CalculateAllVisibleAndNonVisibleStructuresList()
        {
            AllVisibleStructures.Clear();

            Bounds cameraBounds = LocalCameraManager.Instance.CameraBounds;
            Collider2D[] nearbyStructures = Physics2D.OverlapBoxAll(cameraBounds.center, cameraBounds.size, 0, LayerMask.GetMask("Structure Finder"));

            for (int i = 0; i < nearbyStructures.Length; i++)
            {
                Structure structure = nearbyStructures[i].gameObject.GetComponentInParent<Structure>();
                if (structure == null) continue;
                AllVisibleStructures.Add(structure.gameObject);
            }
        }

        private void CalculateAllVisibleGamePlayObjectsList()
        {
            AllVisibleGamePlayObjects.Clear();
            AllVisibleGamePlayObjects.AddRange(AllVisibleCharacters);
            AllVisibleGamePlayObjects.AddRange(AllVisibleVehicles);
            AllVisibleGamePlayObjects.AddRange(AllVisibleStructures);
        }

        private void UpdateCalculatedLists()
        {
            CalculateAllCharactersList();
            CalculateAllVisibleAndNonVisibleCharactersList();

            CalculateAllVisibleVehicles();

            CalculateAllStructuresList();
            CalculateAllVisibleAndNonVisibleStructuresList();

            CalculateAllVisibleGamePlayObjectsList();
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------



        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Logic 
        private void UpdateGameObjectLists()
        {
            AllNetworkedPlayers.RemoveNullOrDefault();
            AllPlayerSurvivors.RemoveNullOrDefault();
            AllHostileSurvivors.RemoveNullOrDefault();
            AllZombies.RemoveNullOrDefault();

            AllWeapons.RemoveNullOrDefault();
            AllBullets.RemoveNullOrDefault();
            AllBulletCasings.RemoveNullOrDefault();
            AllThrowableProjectiles.RemoveNullOrDefault();

            AllVehicles.RemoveNullOrDefault();

            AllVisualEffects.RemoveNullOrDefault();

            AllMapIcons.RemoveNullOrDefault();

            AllBuildings.RemoveNullOrDefault();
        }

        private void LateUpdate()
        {
            UpdateGameObjectLists();
            UpdateCalculatedLists();
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
    }
}