using CITC.GameManager;
using UnityEngine;
using static ZoneCell;

public class ZoneGrid : MonoBehaviour
{
    [Header("Grid")]
    private CityGenerator _cityGenerator;
    private ZoneCell[,] _zoneGrid;

    [Header("Perlin Noise")]
    [SerializeField] private float _scale = .07f;
    [SerializeField] private float _xOffset = 0;
    [SerializeField] private float _yOffset = 0;

    [Header("Perlin Noise Levels")]
    [SerializeField] private float _waterLevel = -0.1f;
    [SerializeField] private float _sandLevel = 0.025f;
    [SerializeField] private float _wildernessLevel = 0.2f;
    [SerializeField] private float _suburbsLevel = 0.45f;
    [SerializeField] private float _residentialAreaLevel = 0.55f;
    [SerializeField] private float _cityCenterLevel = 1f;

    [Header("Fall Off Map")]
    [SerializeField] private float _fallOffSteepness = 1f;
    [SerializeField] private float _fallOffPosition = 9.5f;

    private void Awake()
    {
        _cityGenerator = this.gameObject.GetComponent<CityGenerator>();
    }

    public ZoneCellType GetZoneCellTypeAt(int x, int y)
    {
        if (_cityGenerator.OutOfBound(x,y)) return ZoneCellType.None;
        return _zoneGrid[x, y].zoneCellType;
    }

    public void GenerateZoneGrid(System.Random cityRandom)
    {
        //Perlin Noise Offset
        int offsetRange = 10000;
        _xOffset = cityRandom.Next(-offsetRange, offsetRange);
        _yOffset = cityRandom.Next(-offsetRange, offsetRange);

        //Perlin Noise
        float[,] noiseMap = new float[_cityGenerator.GridWidth, _cityGenerator.GridHeight];
        for (int y = 0; y < _cityGenerator.GridHeight; y++)
        {
            for (int x = 0; x < _cityGenerator.GridWidth; x++)
            {
                float noiseValue = Mathf.PerlinNoise(x * _scale + _xOffset, y * _scale + _yOffset);
                noiseMap[x, y] = noiseValue;
            }
        }

        //Fall Off Map
        float[,] falloffMap = new float[_cityGenerator.GridWidth, _cityGenerator.GridHeight];
        for (int y = 0; y < _cityGenerator.GridHeight; y++)
        {
            for (int x = 0; x < _cityGenerator.GridWidth; x++)
            {
                float xv = x / (float)_cityGenerator.GridWidth * 2 - 1;
                float yv = y / (float)_cityGenerator.GridHeight * 2 - 1;
                float v = Mathf.Max(Mathf.Abs(xv), Mathf.Abs(yv));
                falloffMap[x, y] = Mathf.Pow(v, _fallOffSteepness) / (Mathf.Pow(v, _fallOffSteepness) + Mathf.Pow(_fallOffPosition - _fallOffPosition * v, _fallOffSteepness));
            }
        }

        //Grid
        _zoneGrid = new ZoneCell[_cityGenerator.GridWidth, _cityGenerator.GridHeight];
        for (int y = 0; y < _cityGenerator.GridHeight; y++)
        {
            for (int x = 0; x < _cityGenerator.GridWidth; x++)
            {
                ZoneCell zoneCell = new ZoneCell();

                float noiseValue = noiseMap[x, y];
                noiseValue -= falloffMap[x, y];

                if (noiseValue < _waterLevel) zoneCell.zoneCellType = ZoneCell.ZoneCellType.WATER;
                else if (noiseValue < _sandLevel) zoneCell.zoneCellType = ZoneCell.ZoneCellType.SAND;
                else if (noiseValue < _wildernessLevel) zoneCell.zoneCellType = ZoneCell.ZoneCellType.WILDERNESS;
                else if (noiseValue < _suburbsLevel) zoneCell.zoneCellType = ZoneCell.ZoneCellType.SUBURBS;
                else if (noiseValue < _residentialAreaLevel) zoneCell.zoneCellType = ZoneCell.ZoneCellType.RESIDENTIAL_AREA;
                else if (noiseValue < _cityCenterLevel) zoneCell.zoneCellType = ZoneCell.ZoneCellType.CITY_CENTER;
                else zoneCell.zoneCellType = ZoneCell.ZoneCellType.CITY_CENTER;


                _zoneGrid[x, y] = zoneCell;
            }
        }
    }

    private void OnDrawGizmosZone()
    {
        if (!Application.isPlaying) return;

        for (int y = 0; y < _cityGenerator.GridHeight; y++)
        {
            for (int x = 0; x < _cityGenerator.GridWidth; x++)
            {
                ZoneCell zoneCell = _zoneGrid[x, y];
                switch (zoneCell.zoneCellType)
                {
                    case ZoneCell.ZoneCellType.WATER:
                        Gizmos.color = Color.blue;
                        break;
                    case ZoneCell.ZoneCellType.SAND:
                        Gizmos.color = Color.yellow;
                        break;
                    case ZoneCell.ZoneCellType.WILDERNESS:
                        Gizmos.color = Color.green;
                        break;
                    case ZoneCell.ZoneCellType.SUBURBS:
                        Gizmos.color = Color.white;
                        break;
                    case ZoneCell.ZoneCellType.RESIDENTIAL_AREA:
                        Gizmos.color = Color.grey;
                        break;
                    case ZoneCell.ZoneCellType.CITY_CENTER:
                        Gizmos.color = Color.black;
                        break;
                }
                Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.75f);

                Vector3 pos = new Vector3(x, y) * _cityGenerator.TileSize;

                //Offset Position
                Vector3 offset = new Vector3(_cityGenerator.GridWidth, _cityGenerator.GridHeight) * _cityGenerator.TileSize / 2f;
                pos -= offset;

                Gizmos.DrawCube(pos, Vector3.one * _cityGenerator.TileSize);
            }
        }
    }

    private void OnDrawGizmos()
    {
        OnDrawGizmosZone();
    }
}
