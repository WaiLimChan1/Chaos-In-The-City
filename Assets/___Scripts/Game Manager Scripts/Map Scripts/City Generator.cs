using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using static Tile;
using static CITC.GameManager.StreetTileConnections;
using System.Collections;
using UnityEngine.Tilemaps;

namespace CITC.GameManager
{
    public class CityGenerator : MonoBehaviour
    {
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Static Variables
        public static CityGenerator Instance { get; private set; }
        //------------------------------------------------------------------------------------------------------------------------------------------------

        //-------------------------------------------------------------------------------------------------------------------------------------
        [Header("City Seed")]
        private System.Random _cityRandom;
        [SerializeField] private int _citySeed;
        //-------------------------------------------------------------------------------------------------------------------------------------

        //-------------------------------------------------------------------------------------------------------------------------------------
        [Header("City Generation Progress")]
        private float _tileGameObjectCreationProgress;
        private bool _cityGenerationComplete;
        //-------------------------------------------------------------------------------------------------------------------------------------

        //-------------------------------------------------------------------------------------------------------------------------------------
        [Header("Tile Prefabs")]
        [SerializeField] private GameObject _tilePrefab;
        [SerializeField] private GameObject _pavementTilePrefab;

        [Header("Tile SOs")]
        [SerializeField] private Tile_SO _waterTile_SO;
        [SerializeField] private Tile_SO _sandTile_SO;
        [SerializeField] private Tile_SO _wildernessTile_SO;
        [SerializeField] private Tile_SO _suburbsPavementTile_SO;
        [SerializeField] private Tile_SO _residentialAreaPavementTile_SO;
        [SerializeField] private Tile_SO _cityCenterPavementTile_SO;
        //-------------------------------------------------------------------------------------------------------------------------------------

        //-------------------------------------------------------------------------------------------------------------------------------------
        [Serializable]
        public class WFC_SpawnInfo
        {
            public StreetTile_SO streetTile_SO;
            public int spawnChanceWeight;
        }

        [Header("Street Tile SOs")]
        [SerializeField] private StreetTile_SO _fourWayIntersection_SOs;
        [SerializeField] private List<WFC_SpawnInfo> _wfc_SpawnInfos;

        [Header("Tile Variables")]
        public float TileSize = 19.2f;

        [Header("Tile Anchors")]
        [SerializeField] private GameObject _tileAnchor;
        [SerializeField] private GameObject _streetTileAnchor;
        [SerializeField] private GameObject _pavementTileAnchor;
        //-------------------------------------------------------------------------------------------------------------------------------------

        //-------------------------------------------------------------------------------------------------------------------------------------
        [Header("Zone")]
        private ZoneGrid _zoneGrid;
        //-------------------------------------------------------------------------------------------------------------------------------------

        //-------------------------------------------------------------------------------------------------------------------------------------
        [Header("Grid Variables")]
        public int GridWidth;
        public int GridHeight;

        [Header("Grid Data Storage")]
        [SerializeField] private List<List<Tile_SO>> _tileGrid;
        [SerializeField] private List<Vector2> _emptyCells;
        [SerializeField] private GameObject[,] _tileGameObjectGrid;
        //-------------------------------------------------------------------------------------------------------------------------------------

        //-------------------------------------------------------------------------------------------------------------------------------------
        [Header("Tile Maps")]
        public Tilemap mapTileMap;
        //-------------------------------------------------------------------------------------------------------------------------------------



        //-------------------------------------------------------------------------------------------------------------------------------------
        private void SetUp()
        {
            _zoneGrid = GetComponent<ZoneGrid>();
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
        //-------------------------------------------------------------------------------------------------------------------------------------


        #region Helper Functions
        //-------------------------------------------------------------------------------------------------------------------------------------
        public bool OutOfBound(int x, int y)
        {
            if (x < 0 || x >= GridWidth) return true;
            if (y < 0 || y >= GridHeight) return true;
            return false;
        }

        public void ResetCityGenerationProgress()
        {
            _tileGameObjectCreationProgress = 0;
            _cityGenerationComplete = false;
        }

        public float CityGeneratonProgress
        {
            get
            {
                float tileGridCalculationProgress = 1f - ((float)_emptyCells.Count / (float)(GridWidth * GridHeight));
                float tileGameObjectCreationPogress = _tileGameObjectCreationProgress;
                return 1 / 2f * (tileGameObjectCreationPogress + tileGameObjectCreationPogress);
            }
        }

        public bool CityGenerationCompleted { get { return CityGeneratonProgress >= 1f || _cityGenerationComplete; } }
        //-------------------------------------------------------------------------------------------------------------------------------------
        #endregion


        //-------------------------------------------------------------------------------------------------------------------------------------
        private void Start()
        {
            ResetCityGenerationProgress();
            //GenerateCityAsync(UnityEngine.Random.Range(0, int.MaxValue));
        }
        //-------------------------------------------------------------------------------------------------------------------------------------



        #region Generate City
        //-------------------------------------------------------------------------------------------------------------------------------------
        private void CalculateTileGrid()
        {
            InitializeDataStorage();

            _zoneGrid.GenerateZoneGrid(_cityRandom);

            FillTileGridByZone(ZoneCell.ZoneCellType.WATER, _waterTile_SO);
            FillTileGridByZone(ZoneCell.ZoneCellType.SAND, _sandTile_SO);
            FillTileGridByZone(ZoneCell.ZoneCellType.WILDERNESS, _wildernessTile_SO);

            CalculateBorderStreetTiles();

            CalculateStreetTiles();
            RemoveSurroundedStreetTiles();
            ExtendUnconnectedStreets();

            CleanUpStreetTiles();
            CleanUpStreetTiles();
            CleanUpStreetTiles();

            FillTileGridByZone(ZoneCell.ZoneCellType.SUBURBS, _suburbsPavementTile_SO);
            FillTileGridByZone(ZoneCell.ZoneCellType.RESIDENTIAL_AREA, _residentialAreaPavementTile_SO);
            FillTileGridByZone(ZoneCell.ZoneCellType.CITY_CENTER, _cityCenterPavementTile_SO);
        }

        private IEnumerator CreateTileGameObjectsCoroutine(Stopwatch cityGenerationTimer)
        {
            Stopwatch currentFrameTime = new Stopwatch();
            currentFrameTime.Start();

            float maxFrames = 15f; // Target frame time for 60 FPS (you can adjust this target)
            float maxFrameTime = 1f / maxFrames;

            float progressPerIteration = 1f / (GridWidth * GridHeight * 2f);

            //Create Tile Game Objects
            yield return StartCoroutine(CreateTileGameObjects(currentFrameTime, maxFrameTime, progressPerIteration));

            //Complete Pavement Tile Game Objects
            yield return StartCoroutine(CompletePavementTileGameObjects(currentFrameTime, maxFrameTime, progressPerIteration));
        
            //Fill In Map Tile Map
            FillInMapTileMap();

            _cityGenerationComplete = true; //Ensure it is complete
            cityGenerationTimer.Stop();
            UnityEngine.Debug.Log($"City Generation - Seed: {_citySeed} - Time: {cityGenerationTimer.ElapsedMilliseconds} ms");
        }

        public async void GenerateCityAsync(int citySeed)
        {
            Stopwatch cityGenerationTimer = new Stopwatch();
            cityGenerationTimer.Start();

            ResetCityGenerationProgress();

            _citySeed = citySeed;
            _cityRandom = new System.Random(_citySeed);

            await Task.Run(() =>
            {
                CalculateTileGrid();
            });

            StartCoroutine(CreateTileGameObjectsCoroutine(cityGenerationTimer));
        }
        //-------------------------------------------------------------------------------------------------------------------------------------
        #endregion



        #region Initialize Data Storage
        //-------------------------------------------------------------------------------------------------------------------------------------
        private void InitializeDataStorage()
        {
            //Initialize Tile Grid
            _tileGrid = new List<List<Tile_SO>>();
            for (int x = 0; x < GridWidth; x++)
            {
                _tileGrid.Add(new List<Tile_SO>());
                for (int y = 0; y < GridHeight; y++)
                    _tileGrid[x].Add(null);
            }

            //Initialize Empty Cells
            for (int x = 0; x < GridWidth; x++)
                for (int y = 0; y < GridHeight; y++)
                    _emptyCells.Add(new Vector2(x, y));

            //Initialize Tile Game Object Grid
            _tileGameObjectGrid = new GameObject[GridWidth, GridHeight];
        }
        //-------------------------------------------------------------------------------------------------------------------------------------
        #endregion



        #region Fill Tile Grid By Zone
        //-------------------------------------------------------------------------------------------------------------------------------------
        private void FillTileGridByZone(ZoneCell.ZoneCellType zoneCellType, Tile_SO tile_SO)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    if (_zoneGrid.GetZoneCellTypeAt(x, y) == zoneCellType)
                    {
                        if (_tileGrid[x][y] != null) continue; //Skip, if the grid cell is already filled
                        _tileGrid[x][y] = tile_SO;
                        _emptyCells.Remove(new Vector2(x, y));
                    }
                }
            }
        }
        //-------------------------------------------------------------------------------------------------------------------------------------
        #endregion



        #region Calculate Border Street Tiles
        //-------------------------------------------------------------------------------------------------------------------------------------
        private void CalculateBorderStreetTiles()
        {
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    if (
                        //Sides
                        _zoneGrid.GetZoneCellTypeAt(x - 1, y) == ZoneCell.ZoneCellType.WILDERNESS ||
                        _zoneGrid.GetZoneCellTypeAt(x + 1, y) == ZoneCell.ZoneCellType.WILDERNESS ||
                        _zoneGrid.GetZoneCellTypeAt(x, y + 1) == ZoneCell.ZoneCellType.WILDERNESS ||
                        _zoneGrid.GetZoneCellTypeAt(x, y - 1) == ZoneCell.ZoneCellType.WILDERNESS ||

                        //Corners
                        _zoneGrid.GetZoneCellTypeAt(x - 1, y - 1) == ZoneCell.ZoneCellType.WILDERNESS ||
                        _zoneGrid.GetZoneCellTypeAt(x - 1, y + 1) == ZoneCell.ZoneCellType.WILDERNESS ||
                        _zoneGrid.GetZoneCellTypeAt(x + 1, y - 1) == ZoneCell.ZoneCellType.WILDERNESS ||
                        _zoneGrid.GetZoneCellTypeAt(x + 1, y + 1) == ZoneCell.ZoneCellType.WILDERNESS)
                    {
                        if (_tileGrid[x][y] != null) continue; //Skip, if the grid cell is already filled
                        _tileGrid[x][y] = _fourWayIntersection_SOs;
                        _emptyCells.Remove(new Vector2(x, y));
                    }
                }
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------
        #endregion



        #region Calculate Street Tiles
        //-------------------------------------------------------------------------------------------------------------------------------------
        private List<StreetTileConnectionType> CalculatePossibleTileConnectionTypeForOneDirection(int checkX, int checkY, TileDirection checkTileDirection)
        {
            if (OutOfBound(checkX, checkY)) return StreetTileConnections.Instance.CanConnectTo(StreetTileConnectionType.NONE);

            StreetTile_SO streetTile_SO = _tileGrid[checkX][checkY] as StreetTile_SO;
            if (streetTile_SO == null) return StreetTileConnections.Instance.CanConnectTo(StreetTileConnectionType.NONE);

            if (checkTileDirection == TileDirection.NORTH) return StreetTileConnections.Instance.CanConnectTo(streetTile_SO.NorthConnectionType);
            if (checkTileDirection == TileDirection.EAST) return StreetTileConnections.Instance.CanConnectTo(streetTile_SO.EastConnectionType);
            if (checkTileDirection == TileDirection.SOUTH) return StreetTileConnections.Instance.CanConnectTo(streetTile_SO.SouthConnectionType);
            if (checkTileDirection == TileDirection.WEST) return StreetTileConnections.Instance.CanConnectTo(streetTile_SO.WestConnectionType);

            return StreetTileConnections.Instance.CanConnectTo(StreetTileConnectionType.NONE);
        }

        private PossibleTileConnectionTypes CalculatePossibleTileConnectionTypesForAllDirection(int x, int y)
        {
            PossibleTileConnectionTypes possibleTileConnectionTypes = new PossibleTileConnectionTypes();
            possibleTileConnectionTypes.PossibleNorthTileConnectionTypes = CalculatePossibleTileConnectionTypeForOneDirection(x, y + 1, TileDirection.SOUTH);
            possibleTileConnectionTypes.PossibleEastTileConnectionTypes = CalculatePossibleTileConnectionTypeForOneDirection(x + 1, y, TileDirection.WEST);
            possibleTileConnectionTypes.PossibleSouthTileConnectionTypes = CalculatePossibleTileConnectionTypeForOneDirection(x, y - 1, TileDirection.NORTH);
            possibleTileConnectionTypes.PossibleWestTileConnectionTypes = CalculatePossibleTileConnectionTypeForOneDirection(x - 1, y, TileDirection.EAST);
            return possibleTileConnectionTypes;
        }

        private List<WFC_SpawnInfo> FindPossibleTile_SOs(PossibleTileConnectionTypes possibleTileConnectionTypes)
        {
            List<WFC_SpawnInfo> possibleTile_SOs = new List<WFC_SpawnInfo>();
            for (int i = 0; i < _wfc_SpawnInfos.Count; i++)
            {
                if (!possibleTileConnectionTypes.PossibleNorthTileConnectionTypes.Contains(_wfc_SpawnInfos[i].streetTile_SO.NorthConnectionType)) continue;
                if (!possibleTileConnectionTypes.PossibleEastTileConnectionTypes.Contains(_wfc_SpawnInfos[i].streetTile_SO.EastConnectionType)) continue;
                if (!possibleTileConnectionTypes.PossibleSouthTileConnectionTypes.Contains(_wfc_SpawnInfos[i].streetTile_SO.SouthConnectionType)) continue;
                if (!possibleTileConnectionTypes.PossibleWestTileConnectionTypes.Contains(_wfc_SpawnInfos[i].streetTile_SO.WestConnectionType)) continue;

                possibleTile_SOs.Add(_wfc_SpawnInfos[i]);
            }
            return possibleTile_SOs;
        }

        private int PossibleTile_SOsCount(Vector2 cell)
        {
            PossibleTileConnectionTypes possibleTileConnectionTypes = CalculatePossibleTileConnectionTypesForAllDirection((int)cell.x, (int)cell.y);
            List<WFC_SpawnInfo> possibleTile_SOs = FindPossibleTile_SOs(possibleTileConnectionTypes);
            return possibleTile_SOs.Count;
        }

        private Tile_SO GetRandomPossibleTile_SO(Vector2 cell)
        {
            PossibleTileConnectionTypes possibleTileConnectionTypes = CalculatePossibleTileConnectionTypesForAllDirection((int)cell.x, (int)cell.y);
            List<WFC_SpawnInfo> possibleTile_SOs = FindPossibleTile_SOs(possibleTileConnectionTypes);

            if (possibleTile_SOs.Count == 0) return null;

            //Ignore Weight
            //return possibleTile_SOs[_cityRandom.Next(0, possibleTile_SOs.Count)].streetTile_SO;

            int totalWeight = 0;
            foreach (WFC_SpawnInfo wfc_SpawnInfo in possibleTile_SOs) totalWeight += wfc_SpawnInfo.spawnChanceWeight;

            int randomChance = _cityRandom.Next(0, totalWeight);
            int weightCounter = 0;
            foreach (WFC_SpawnInfo wfc_SpawnInfo in possibleTile_SOs)
            {
                weightCounter += wfc_SpawnInfo.spawnChanceWeight;
                if (randomChance < weightCounter) return wfc_SpawnInfo.streetTile_SO;
            }
            return null;
        }

        private bool AssignRandomPossibleTile(Vector2 cell)
        {
            Tile_SO tile_SO = GetRandomPossibleTile_SO(cell);
            _tileGrid[(int)cell.x][(int)cell.y] = tile_SO;

            return tile_SO != null;
        }

        private bool CalculateOneStreetTile() //Old
        {
            if (_emptyCells.Count <= 0) return false;

            int lowestPossibleTiles = 100;
            int lowestEntropyIndex = 0;
            for (int i = 0; i < _emptyCells.Count; i++)
            {
                int currentCellPossibleTiles = PossibleTile_SOsCount(_emptyCells[i]);
                if (currentCellPossibleTiles < lowestPossibleTiles && currentCellPossibleTiles != 0)
                {
                    lowestPossibleTiles = currentCellPossibleTiles;
                    lowestEntropyIndex = i;
                }
            }

            if (AssignRandomPossibleTile(_emptyCells[lowestEntropyIndex]))
            {
                _emptyCells.Remove(_emptyCells[lowestEntropyIndex]);
                return true;
            }
            else
            {
                UnityEngine.Debug.Log("Cant Find A Possible Tile For " + _emptyCells[lowestEntropyIndex]);
                return false;
            }
        }

        private bool CalculateOneStreetTile(int[,] numPossible_Grid)
        {
            if (_emptyCells.Count <= 0) return false;

            int lowestPossibleTiles = 100;
            int lowestEntropyIndex = 0;
            for (int i = 0; i < _emptyCells.Count; i++)
            {
                //Use PossibleTile_SOsCount from Grid. Only calculate PossibleTile_SOsCount if grid doesn't have the value.
                if (numPossible_Grid[(int)_emptyCells[i].x, (int)_emptyCells[i].y] == -1)
                    numPossible_Grid[(int)_emptyCells[i].x, (int)_emptyCells[i].y] = PossibleTile_SOsCount(_emptyCells[i]);

                int currentCellPossibleTiles = numPossible_Grid[(int)_emptyCells[i].x, (int)_emptyCells[i].y];

                if (currentCellPossibleTiles < lowestPossibleTiles && currentCellPossibleTiles != 0)
                {
                    lowestPossibleTiles = currentCellPossibleTiles;
                    lowestEntropyIndex = i;
                }

                //Break early if there is already a cell with less than max entropy
                if (lowestPossibleTiles < _wfc_SpawnInfos.Count)
                    break;
            }

            if (AssignRandomPossibleTile(_emptyCells[lowestEntropyIndex]))
            {
                //Update PossibleTile_SOsCount For Adjacent Cells.
                List<Vector2> adjacentCellIndices = new List<Vector2>()
            {
                new Vector2(_emptyCells[lowestEntropyIndex].x + 1, _emptyCells[lowestEntropyIndex].y),
                new Vector2(_emptyCells[lowestEntropyIndex].x - 1, _emptyCells[lowestEntropyIndex].y),
                new Vector2(_emptyCells[lowestEntropyIndex].x, _emptyCells[lowestEntropyIndex].y + 1),
                new Vector2(_emptyCells[lowestEntropyIndex].x, _emptyCells[lowestEntropyIndex].y - 1)

            };
                foreach (Vector2 adjacentCellIndex in adjacentCellIndices)
                {
                    if (OutOfBound((int)adjacentCellIndex.x, (int)adjacentCellIndex.y)) continue;
                    numPossible_Grid[(int)adjacentCellIndex.x, (int)adjacentCellIndex.y] = PossibleTile_SOsCount(adjacentCellIndex);
                }

                //Remove filled cell
                _emptyCells.Remove(_emptyCells[lowestEntropyIndex]);
                return true;
            }
            else
            {
                //UnityEngine.Debug.Log("Cant Find A Possible Tile For " + _emptyCells[lowestEntropyIndex]);
                return false;
            }
        }

        private void CalculateStreetTiles()
        {
            int iterations = (int)(GridWidth * GridHeight);

            //Store the PossibleTile_SOsCount Calculations
            int[,] numPossible_Grid = new int[GridWidth, GridHeight];
            for (int x = 0; x < GridWidth; x++)
                for (int y = 0; y < GridHeight; y++)
                    numPossible_Grid[x, y] = -1;

            for (int i = 0; i < iterations; i++)
                if (!CalculateOneStreetTile(numPossible_Grid))
                    return;
        }
        //-------------------------------------------------------------------------------------------------------------------------------------
        #endregion



        #region Remove Surrounded Street Tiles
        //-------------------------------------------------------------------------------------------------------------------------------------
        private TileType GetTileType(int x, int y)
        {
            if (OutOfBound(x, y)) return TileType.NONE;
            if (_tileGrid[x][y] == null) return TileType.NONE;

            return _tileGrid[x][y].tileType;
        }

        //Replace streets tiles that are surrounded by pavement tiles on three sides or four sides.
        private void RemoveSurroundedStreetTiles()
        {
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    if (_tileGrid[x][y] is not StreetTile_SO) continue; //Only for Street Tiles

                    int pavementCount = 0;
                    if (GetTileType(x + 1, y) == TileType.NONE) pavementCount++; //Pavements have not been generated yet, so they have the tile type of NONE
                    if (GetTileType(x - 1, y) == TileType.NONE) pavementCount++;
                    if (GetTileType(x, y + 1) == TileType.NONE) pavementCount++;
                    if (GetTileType(x, y - 1) == TileType.NONE) pavementCount++;

                    if (pavementCount >= 3)
                    {
                        _tileGrid[x][y] = null;
                        _emptyCells.Add(new Vector2(x, y));
                    }
                }
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------
        #endregion



        #region Extend Unconnected Streets
        //-------------------------------------------------------------------------------------------------------------------------------------
        private bool IsStreetTileConnected(int x, int y)
        {
            int streetCount = 0;
            if (GetTileType(x + 1, y) == TileType.STREET) streetCount++;
            if (GetTileType(x - 1, y) == TileType.STREET) streetCount++;
            if (GetTileType(x, y + 1) == TileType.STREET) streetCount++;
            if (GetTileType(x, y - 1) == TileType.STREET) streetCount++;
            return streetCount >= 2;
        }

        private bool IsStreet(int x, int y)
        {
            if (OutOfBound(x, y)) return false;
            if (_tileGrid[x][y] == null) return false;
            if (_tileGrid[x][y].tileType == TileType.STREET) return true;

            return false;
        }

        private void TurnStreetToIntersection(int x, int y)
        {
            if (!OutOfBound(x, y) && IsStreet(x, y))
            {
                _tileGrid[x][y] = _fourWayIntersection_SOs;
            }
        }

        //For the street tiles surrounded by three pavement tiles, extend them until they are connected (touching two other street tiles)
        private void ExtendUnconnectedStreets()
        {
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    if (_tileGrid[x][y] is not StreetTile_SO) continue; //Only for Street Tiles

                    int pavementCount = 0;
                    if (GetTileType(x + 1, y) == TileType.NONE) pavementCount++; //Pavements have not been generated yet, so they have the tile type of NONE
                    if (GetTileType(x - 1, y) == TileType.NONE) pavementCount++;
                    if (GetTileType(x, y + 1) == TileType.NONE) pavementCount++;
                    if (GetTileType(x, y - 1) == TileType.NONE) pavementCount++;

                    if (pavementCount == 3)
                    {
                        //Go in the opposite direction of the original street
                        Vector2 direction = Vector2.zero;
                        if (GetTileType(x + 1, y) == TileType.STREET) direction.x -= 1;
                        if (GetTileType(x - 1, y) == TileType.STREET) direction.x += 1;
                        if (GetTileType(x, y + 1) == TileType.STREET) direction.y -= 1;
                        if (GetTileType(x, y - 1) == TileType.STREET) direction.y += 1;

                        //Extend until the street tile is connected
                        int currentX = x;
                        int currentY = y;
                        do
                        {
                            //Move
                            currentX += (int)direction.x;
                            currentY += (int)direction.y;

                            //Moving out of the map
                            if (OutOfBound(currentX, currentY)) break;

                            //Extend current street tile
                            _tileGrid[currentX][currentY] = _fourWayIntersection_SOs;
                            _emptyCells.Remove(new Vector2(currentX, currentY));

                            //Once the current path is connected, turn surrounding street tiles into intersection so that they are connectable street tiles.
                            //Note: a street can extend into street tiles in all four directions regardless of what direction it is moving in. 
                            if (IsStreetTileConnected(currentX, currentY))
                            {
                                TurnStreetToIntersection(currentX + 1, currentY);
                                TurnStreetToIntersection(currentX - 1, currentY);
                                TurnStreetToIntersection(currentX, currentY + 1);
                                TurnStreetToIntersection(currentX, currentY - 1);
                            }
                        }
                        while (!IsStreetTileConnected(currentX, currentY));
                    }
                }
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------
        #endregion



        #region Clean Up Street
        //-------------------------------------------------------------------------------------------------------------------------------------
        private void CleanUpStreetTiles()
        {
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    StreetTile_SO currentStreetTile_SO = _tileGrid[x][y] as StreetTile_SO;
                    if (currentStreetTile_SO == null) continue;

                    if (currentStreetTile_SO.NorthEnd != null && !IsStreet(x, y + 1)) _tileGrid[x][y] = currentStreetTile_SO.NorthEnd;
                    else if (currentStreetTile_SO.EastEnd != null && !IsStreet(x + 1, y)) _tileGrid[x][y] = currentStreetTile_SO.EastEnd;
                    else if (currentStreetTile_SO.SouthEnd != null && !IsStreet(x, y - 1)) _tileGrid[x][y] = currentStreetTile_SO.SouthEnd;
                    else if (currentStreetTile_SO.WestEnd != null && !IsStreet(x - 1, y)) _tileGrid[x][y] = currentStreetTile_SO.WestEnd;
                }
            }
        }
        //-------------------------------------------------------------------------------------------------------------------------------------
        #endregion



        #region Create Tile Game Objects
        //-------------------------------------------------------------------------------------------------------------------------------------
        public Vector3 TileOffset() { return new Vector3(GridWidth, GridHeight) * TileSize / 2f; }
        
        private void CreateTileGameObject(int x, int y, Tile_SO newTile_SO)
        {
            //Clear Current Tile Game Object
            if (newTile_SO == null)
            {
                if (_tileGameObjectGrid[x, y] != null)
                {
                    Destroy(_tileGameObjectGrid[x, y]);
                    _tileGameObjectGrid[x, y] = null;
                }
                return;
            }

            //Replace Current Tile_SO
            _tileGrid[x][y] = newTile_SO;

            //Destroy Old Tile Game Object
            if (_tileGameObjectGrid[x, y] != null) Destroy(_tileGameObjectGrid[x, y]);

            //Create Different Tiles
            GameObject gameObject = null;
            if (_tileGrid[x][y] is PavementTile_SO)
            {
                gameObject = Instantiate(_pavementTilePrefab);
                gameObject.GetComponent<PavementTile>().SetUp(newTile_SO, x, y);
                gameObject.transform.parent = _pavementTileAnchor.transform;
            }
            else if (_tileGrid[x][y] is StreetTile_SO)
            {
                gameObject = Instantiate(_tilePrefab);
                gameObject.GetComponent<Tile>().SetUp(newTile_SO, x, y);
                gameObject.transform.parent = _streetTileAnchor.transform;
            }
            else if (_tileGrid[x][y] is Tile_SO)
            {
                gameObject = Instantiate(_tilePrefab);
                gameObject.GetComponent<Tile>().SetUp(newTile_SO, x, y);
                gameObject.transform.parent = _tileAnchor.transform;
            }
            else return;

            gameObject.transform.position = new Vector3(x, y, 0) * TileSize;
            gameObject.name += $" ({x},{y})";
            _tileGameObjectGrid[x, y] = gameObject;

            //Offset Map
            gameObject.transform.position -= TileOffset();
        }

        private IEnumerator CreateTileGameObjects(Stopwatch currentFrameTime, float maxFrameTime, float progressPerIteration)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    _tileGameObjectCreationProgress += progressPerIteration;

                    CreateTileGameObject(x, y, _tileGrid[x][y]);

                    // If the frame time exceeds the target, yield and wait for the next frame
                    if (currentFrameTime.ElapsedMilliseconds > maxFrameTime * 1000)
                    {
                        currentFrameTime.Restart();
                        yield return null; // Yield control back to Unity until the next frame
                    }

                }
            }
        }
        //-------------------------------------------------------------------------------------------------------------------------------------
        #endregion



        #region Complete Pavement Tile Game Object
        //-------------------------------------------------------------------------------------------------------------------------------------
        private bool IsPavement(int x, int y)
        {
            if (OutOfBound(x, y)) return false;
            if (_tileGrid[x][y] == null) return false;
            if (_tileGrid[x][y].tileType == TileType.PAVEMENT) return true;
            return false;
        }

        private IEnumerator CompletePavementTileGameObjects(Stopwatch currentFrameTime, float maxFrameTime, float progressPerIteration)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    _tileGameObjectCreationProgress += progressPerIteration;

                    GameObject currentGameObject = _tileGameObjectGrid[x, y];
                    if (currentGameObject == null) continue;

                    PavementTile currentPavementTile = currentGameObject.GetComponent<PavementTile>();
                    if (currentPavementTile == null) continue;

                    currentPavementTile.SetTileSprite(!IsPavement(x, y + 1), !IsPavement(x + 1, y), !IsPavement(x, y - 1), !IsPavement(x - 1, y));
                    currentPavementTile.CreateBuildingGameObject(_cityRandom);

                    // If the frame time exceeds the target, yield and wait for the next frame
                    if (currentFrameTime.ElapsedMilliseconds > maxFrameTime * 1000)
                    {
                        currentFrameTime.Restart();
                        yield return null; // Yield control back to Unity until the next frame
                    }
                }
            }
        }
        //-------------------------------------------------------------------------------------------------------------------------------------
        #endregion



        #region Fill In Map Tile Map
        //-------------------------------------------------------------------------------------------------------------------------------------
        private void FillInMapTileMap()
        {
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    Tile_SO currentTile_SO = _tileGrid[x][y];
                    if (currentTile_SO == null) continue;

                    currentTile_SO.tileMapTile.color = currentTile_SO.tileSpriteColor;
                    mapTileMap.SetTile(new Vector3Int(x, y, 0), currentTile_SO.tileMapTile);
                }
            }
            mapTileMap.transform.position = Vector3.zero - TileOffset() - new Vector3(TileSize / 2f, TileSize / 2f, 0);
        }
        //-------------------------------------------------------------------------------------------------------------------------------------
        #endregion
    }
}