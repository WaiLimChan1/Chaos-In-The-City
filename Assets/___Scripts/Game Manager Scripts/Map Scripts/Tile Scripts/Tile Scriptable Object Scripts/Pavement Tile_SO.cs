using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Pavement Tile", menuName = "Tiles/Pavement Tile")]
public class PavementTile_SO : Tile_SO
{
    //-------------------------------------------------------------------------------------------------------------------------------------
    [Serializable]
    public class PavementSprite
    {
        public Sprite TileSprite;
        public bool NorthEnd;
        public bool EastEnd;
        public bool SouthEnd;
        public bool WestEnd;
    }

    [Header("Pavement Tiles")]
    public List<PavementSprite> pavementSprites;
    //-------------------------------------------------------------------------------------------------------------------------------------



    //-------------------------------------------------------------------------------------------------------------------------------------
    [Serializable]
    public class BuildingSpawnInfo
    {
        public GameObject BuildingPrefab;
        public int spawnChanceWeight = 1;
    }

    [Header("Buildings")]
    public List<BuildingSpawnInfo> buildingSpawnInfos;

    public int GetTotalWeight()
    {
        int totalWeight = 0;
        foreach (BuildingSpawnInfo buildingSpawnInfo in buildingSpawnInfos)
        {
            totalWeight += buildingSpawnInfo.spawnChanceWeight;
        }
        return totalWeight;
    }

    public GameObject GetRandomBuildingPrefab(System.Random cityRandom)
    {
        if (buildingSpawnInfos.Count == 0) return null;

        int randomChance = cityRandom.Next(0, GetTotalWeight());
        int weightCounter = 0;
        foreach (BuildingSpawnInfo buildingSpawnInfo in buildingSpawnInfos)
        {
            weightCounter += buildingSpawnInfo.spawnChanceWeight;
            if (randomChance < weightCounter) return buildingSpawnInfo.BuildingPrefab;
        }

        return null;
    }
    //-------------------------------------------------------------------------------------------------------------------------------------
}
