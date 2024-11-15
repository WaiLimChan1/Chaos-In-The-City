using System.Collections.Generic;
using UnityEngine;
using static CITC.GameManager.StreetTileConnections;
using static Tile;

namespace CITC.GameManager
{
    public class EntityColorManager : MonoBehaviour
    {
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Static Variables
        public static EntityColorManager Instance { get; private set; }
        //------------------------------------------------------------------------------------------------------------------------------------------------



        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Member Variables
        public Dictionary<int, Color> PlayerColorDictionary;
        //------------------------------------------------------------------------------------------------------------------------------------------------



        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Initialization
        public void SetUp()
        {
            PlayerColorDictionary = new Dictionary<int, Color>
            {
                {7, new Color(0, 1, 0)}, // Green
                {0, new Color(1, 0, 0)}, // Red
                {1, new Color(1, 0.647f, 0)}, // Orange (RGB: 255, 165, 0)
                {2, new Color(0, 0, 1)}, // Blue
                {3, new Color(0.5f, 0, 0.5f)}, // Purple (RGB: 128, 0, 128)
                {4, new Color(1, 1, 0)}, // Yellow (RGB: 255, 255, 0)
                {5, new Color(0, 1, 1)}, // Cyan
                {6, new Color(0.5f, 0.5f, 0.5f)}, // Gray (RGB: 128, 128, 128)
            };
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
        public Color GetPlayerColor(int playerId)
        {
            if (PlayerColorDictionary.TryGetValue(playerId, out Color playerColor))
            {
                return playerColor;
            }
            return Color.white;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
