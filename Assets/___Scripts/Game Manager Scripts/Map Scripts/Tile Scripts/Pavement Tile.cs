using UnityEngine;

public class PavementTile : Tile
{
    public void SetTileSprite(bool NorthEnd, bool EastEnd, bool SouthEnd, bool WestEnd)
    {
        PavementTile_SO pavementTile_SO = Tile_SO as PavementTile_SO;
        if (pavementTile_SO == null) return;

        for (int i = 0; i < pavementTile_SO.pavementSprites.Count; i++)
        {
            if (pavementTile_SO.pavementSprites[i].NorthEnd == NorthEnd &&
                pavementTile_SO.pavementSprites[i].EastEnd == EastEnd &&
                pavementTile_SO.pavementSprites[i].SouthEnd == SouthEnd &&
                pavementTile_SO.pavementSprites[i].WestEnd == WestEnd)
            {
                tileSpriteRenderer.sprite = pavementTile_SO.pavementSprites[i].TileSprite;
            }
        }
    }

    public void CreateBuildingGameObject(System.Random cityRandom)
    {
        PavementTile_SO pavementTile_SO = Tile_SO as PavementTile_SO;
        if (pavementTile_SO == null) return;
        if (pavementTile_SO.buildingSpawnInfos.Count == 0) return; //No Buildings To Spawn

        GameObject buildingPrefab = pavementTile_SO.GetRandomBuildingPrefab(cityRandom);
        if (buildingPrefab == null) return;

        GameObject buildingGameObject = Instantiate(pavementTile_SO.GetRandomBuildingPrefab(cityRandom));
        buildingGameObject.transform.position = this.transform.position;
    }
}
