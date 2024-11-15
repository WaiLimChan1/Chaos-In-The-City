using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum TileType
    {
        NONE,
        WATER,
        SAND,
        WILDERNESS,
        STREET,
        PAVEMENT
    }

    public enum TileDirection
    {
        NORTH,
        EAST, 
        SOUTH, 
        WEST
    }

    [Header("Components")]
    public SpriteRenderer tileSpriteRenderer;

    [Header("Tile_SO")]
    public Tile_SO Tile_SO;

    [Header("Variables")]
    public int xIndex;
    public int yIndex;

    public void SetUp(Tile_SO tile_SO, int xIndex, int yIndex)
    {
        Tile_SO = tile_SO;

        tileSpriteRenderer.sprite = Tile_SO.tileSprite;
        tileSpriteRenderer.color = tile_SO.tileSpriteColor;
        this.xIndex = xIndex;
        this.yIndex = yIndex;
    }
}
