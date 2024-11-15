using CITC.GameManager;
using UnityEngine;

[CreateAssetMenu(fileName = "New Street Tile", menuName = "Tiles/Street Tile")]
public class StreetTile_SO : Tile_SO
{
    [Header("Street Tile Connection Types")]
    public StreetTileConnections.StreetTileConnectionType NorthConnectionType;
    public StreetTileConnections.StreetTileConnectionType EastConnectionType;
    public StreetTileConnections.StreetTileConnectionType SouthConnectionType;
    public StreetTileConnections.StreetTileConnectionType WestConnectionType;

    [Header("Street Tile Clean Up")]
    public StreetTile_SO NorthEnd;
    public StreetTile_SO EastEnd;
    public StreetTile_SO SouthEnd;
    public StreetTile_SO WestEnd;
}
