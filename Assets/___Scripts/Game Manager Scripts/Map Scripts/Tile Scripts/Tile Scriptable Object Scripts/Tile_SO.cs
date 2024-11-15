using UnityEngine;
using UnityEngine.Tilemaps;
using static Tile;

[CreateAssetMenu(fileName = "New Tile", menuName = "Tiles/Tile")]
public class Tile_SO : ScriptableObject
{
    [Header("Tile Information")]
    public Sprite tileSprite;
    public UnityEngine.Tilemaps.Tile tileMapTile;
    public Color tileSpriteColor;
    public TileType tileType;
}
