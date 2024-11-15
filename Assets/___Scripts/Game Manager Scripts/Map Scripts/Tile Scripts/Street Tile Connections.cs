using System.Collections.Generic;
using UnityEngine;

namespace CITC.GameManager
{
    public class StreetTileConnections : MonoBehaviour
    {
        public enum StreetTileConnectionType
        {
            NONE,
            ROAD,
            ROAD_EDGE,
            CROSS_WALK,
            CROSS_WALK_EDGE,
            INTERSECTION
        };

        public static StreetTileConnections Instance;

        public Dictionary<StreetTileConnectionType, List<StreetTileConnectionType>> connections;

        //-----------------------------------------------------------------------------------------------------------
        //Initialization
        private void SetUp()
        {
            // Initialize connections
            connections = new Dictionary<StreetTileConnectionType, List<StreetTileConnectionType>>
            {
                { StreetTileConnectionType.NONE, new List<StreetTileConnectionType>
                    {
                        StreetTileConnectionType.ROAD,
                        StreetTileConnectionType.ROAD_EDGE,
                        StreetTileConnectionType.CROSS_WALK,
                        StreetTileConnectionType.CROSS_WALK_EDGE,
                        StreetTileConnectionType.INTERSECTION,
                    }
                },
                { StreetTileConnectionType.ROAD, new List<StreetTileConnectionType> { StreetTileConnectionType.ROAD, StreetTileConnectionType.CROSS_WALK, StreetTileConnectionType.INTERSECTION } },
                { StreetTileConnectionType.ROAD_EDGE, new List<StreetTileConnectionType> { } },
                { StreetTileConnectionType.CROSS_WALK, new List<StreetTileConnectionType> { StreetTileConnectionType.ROAD, StreetTileConnectionType.INTERSECTION } },
                { StreetTileConnectionType.CROSS_WALK_EDGE, new List<StreetTileConnectionType> { } },
                { StreetTileConnectionType.INTERSECTION, new List<StreetTileConnectionType> { StreetTileConnectionType.ROAD, StreetTileConnectionType.CROSS_WALK } }
            };
        }

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                SetUp();
            }
            else Destroy(this.gameObject);
        }
        //-----------------------------------------------------------------------------------------------------------

        public List<StreetTileConnectionType> CanConnectTo(StreetTileConnectionType tileType)
        {
            if (connections.TryGetValue(tileType, out List<StreetTileConnectionType> tileTypes))
            {
                return tileTypes;
            }
            return new List<StreetTileConnectionType>(); // Return an empty list if tileType not found
        }
    }
}