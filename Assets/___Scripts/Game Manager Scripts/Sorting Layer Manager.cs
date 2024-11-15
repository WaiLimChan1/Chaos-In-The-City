using System.Collections.Generic;
using UnityEngine;

namespace CITC.GameManager
{
    public class SortingLayerManager : MonoBehaviour
    {
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Static Variables
        public static SortingLayerManager Instance { get; private set; }
        //------------------------------------------------------------------------------------------------------------------------------------------------



        //------------------------------------------------------------------------------------------------------------------------------------------------
        [Header("Components")]
        private GameObjectListManager gameObjectListManager;
        //------------------------------------------------------------------------------------------------------------------------------------------------



        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Initialization
        private void SetUp()
        {
            gameObjectListManager = GameObjectListManager.Instance;
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

        //Bigger position y value first
        private void SortGameObjectListByPosition(List<GameObject> list)
        {
            list.Sort((a, b) => b.transform.position.y.CompareTo(a.transform.position.y));
        }

        private void UpdateStartingSortingOrder(List<GameObject> list)
        {
            int currentSortingOrder = 0;

            for (int i = 0; i < list.Count; i++)
            {
                Character character = list[i].GetComponent<Character>();
                if (Character.CanUse(character))
                {
                    if (Vehicle.CanUse(character.CurrentVehicle)) continue; //Character In Vehicle

                    character._outfitAC.StartingSortingOrder = currentSortingOrder;
                    currentSortingOrder += Character.NUM_SORTING_ORDER + Weapon.NUM_OF_WEAPON_SORTING_ORDERS_BEHIND;
                    continue;
                }

                Vehicle vehicle = list[i].GetComponent<Vehicle>();
                if (Vehicle.CanUse(vehicle))
                {
                    vehicle.StartingSortingOrder = currentSortingOrder;
                    currentSortingOrder += Vehicle.NUM_SORTING_ORDER;
                    continue;
                }    

                Building building = list[i].GetComponent<Building>();
                if (building != null)
                {
                    building.StartingSortingOrder = currentSortingOrder;
                    currentSortingOrder += Building.NUM_SORTING_ORDER;
                    continue;
                }
            }
        }

        private void UpdateAllGamePlaySortingOrder()
        {
            SortGameObjectListByPosition(gameObjectListManager.AllVisibleGamePlayObjects);
            UpdateStartingSortingOrder(gameObjectListManager.AllVisibleGamePlayObjects);
        }

        private void LateUpdate()
        {
            UpdateAllGamePlaySortingOrder();
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
