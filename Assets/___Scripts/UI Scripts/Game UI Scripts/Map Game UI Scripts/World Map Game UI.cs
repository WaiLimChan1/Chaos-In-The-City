using CITC.GameManager;
using ExitGames.Client.Photon;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorldMapGameUI : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Static Variables
    public static WorldMapGameUI Instance { get; private set; }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Components")]
    [SerializeField] private GameObject _content;
    [SerializeField] private Camera _worldMapCamera;
    [SerializeField] private RectTransform _worldMapRectTransform;

    [Header("Variables")]
    [SerializeField] public bool _activateWorldMap;
    private Vector3 _lastMousePosition;

    [Header("World Map Camera Stats")]
    private Vector2 _cameraSizeRange = new Vector2(100, 1500);
    private float _cameraSizeRangeRatio = 1;
    private float _cameraTargetSize;

    private float _cameraSizeChangeSpeed = 500f;
    private float _cameraSizeChangeLerp = 5f;

    private float _mapIconMinScale = 5f;
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Initialization
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _activateWorldMap = false;
        }
        else Destroy(this);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Helper Functions
    public bool Active { get { return _activateWorldMap; } }

    public bool IsMouseOverWorldMap()
    {
        if (!_content.activeInHierarchy) return false;

        Vector2 mousePosition = Input.mousePosition;
        return RectTransformUtility.RectangleContainsScreenPoint(_worldMapRectTransform, mousePosition, null);
    }

    public float MapIconScale { get { return _worldMapCamera.orthographicSize / _cameraSizeRange.y * _mapIconMinScale; } }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Logic Functions
    private void TakeActivateMapInput()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) _activateWorldMap = !_activateWorldMap;
        _content.gameObject.SetActive(_activateWorldMap);
    }

    private void TakeCameraSizeInput()
    {
        if (!Active) return;

        if (IsMouseOverWorldMap()) _cameraSizeRangeRatio += -1 * _cameraSizeChangeSpeed * Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime;
        _cameraSizeRangeRatio = Mathf.Clamp01(_cameraSizeRangeRatio);
        _cameraTargetSize = Mathf.Lerp(_cameraSizeRange.x, _cameraSizeRange.y, _cameraSizeRangeRatio);
        _worldMapCamera.orthographicSize = Mathf.Lerp(_worldMapCamera.orthographicSize, _cameraTargetSize, _cameraSizeChangeLerp * Time.deltaTime);
    }

    private void TakeCameraMovementInput()
    {
        if (!Active) return;

        if (Input.GetKeyDown(KeyCode.Mouse0)) _lastMousePosition = Input.mousePosition; //Begin Drag
        if (Input.GetKey(KeyCode.Mouse0)) //On Drag
        {
            Vector3 changeInPosition = Input.mousePosition - _lastMousePosition;
            _worldMapCamera.transform.position -= changeInPosition * _worldMapCamera.orthographicSize * Time.deltaTime;
            _worldMapCamera.transform.position = new Vector3(_worldMapCamera.transform.position.x, _worldMapCamera.transform.position.y, -10);
            _lastMousePosition = Input.mousePosition;
        }
    }

    private void TakeCenterAndMaximizeCameraInput()
    {
        if (!Active) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            _worldMapCamera.transform.position = new Vector3(0, 0, -10);
            _cameraSizeRangeRatio = 1;
        }
    }

    //Keep the map icons' scale constant while zooming in & out
    private void UpdateScaleOfMapIcons()
    {
        if (!Active) return;

        List<GameObject> allMapIconGameObjectss = GameObjectListManager.Instance.AllMapIcons.List;
        foreach (GameObject mapIconGameObject in allMapIconGameObjectss)
        {
            MapIcon mapIcon = mapIconGameObject.GetComponent<MapIcon>();
            if (mapIcon == null) continue;
            mapIcon.UpdateScale(MapIconScale);
        }
    }

    private void LateUpdate()
    {
        TakeActivateMapInput();
        TakeCameraSizeInput();
        TakeCameraMovementInput();
        TakeCenterAndMaximizeCameraInput();
        UpdateScaleOfMapIcons();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}
