using System;
using System.Collections.Generic;
using UnityEngine;

namespace CITC.GameManager
{
    public class CharacterCollisionManager : MonoBehaviour
    {
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Static Variables
        public static CharacterCollisionManager Instance { get; private set; }
        //------------------------------------------------------------------------------------------------------------------------------------------------



        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Initialization
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else Destroy(this);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------

        //------------------------------------------------------------------------------------------------------------------------------------------------
        private void LateUpdate()
        {
            return;
            int numChecks = 1000;
            List<GameObject> allCharacters = GameObjectListManager.Instance.AllCharacters;
            for (int i = 0; i < allCharacters.Count; i++)
            {
                for (int j = 0; j < allCharacters.Count; j++)
                {
                    if (i == j) continue;
                    float distance = Vector3.Distance(allCharacters[i].transform.position, allCharacters[j].transform.position);
                    if (distance < 0.7)
                    {
                        Vector3 direction = allCharacters[i].transform.position - allCharacters[j].transform.position;
                        allCharacters[i].transform.position += direction * distance / 2;
                        allCharacters[j].transform.position -= direction * distance / 2;
                    }
                    numChecks--;
                    if (numChecks == 0) return;
                }
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
    }
}