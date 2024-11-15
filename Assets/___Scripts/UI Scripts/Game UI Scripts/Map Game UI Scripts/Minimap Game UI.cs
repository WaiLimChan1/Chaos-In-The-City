using CITC.GameManager;
using UnityEngine;
using System.Collections.Generic;

public class MinimapGameUI : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Static Variables
    public static MinimapGameUI Instance { get; private set; }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Variable")]
    private NetworkedPlayer _networkedPlayer;

    [Header("Components")]
    [SerializeField] private GameObject _content;
    [SerializeField] private Camera _minimapCamera;
    [SerializeField] private RectTransform _minimapRectTransform;

    [Header("Minimap Camera Stats")]
    private Vector2 _cameraSizeRange = new Vector2(20, 200);
    private float _cameraSizeRangeRatio = 1;
    private float _cameraTargetSize;

    private float _cameraSizeChangeSpeed = 500f;
    private float _cameraSizeChangeLerp = 5f;

    private float _mapIconMinScale = 1.5f;
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Initialization
    public void SetUp(NetworkedPlayer networkedPlayer)
    {
        _networkedPlayer = networkedPlayer;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Helper Functions
    public bool IsMouseOverMinimap()
    {
        if (!_content.activeInHierarchy) return false;

        Vector2 minimapCenter = _minimapRectTransform.position;
        Vector2 mousePosition = Input.mousePosition;
        float distance = Vector2.Distance(mousePosition, minimapCenter);
        return distance <= _minimapRectTransform.rect.width / 2f;
    }

    public float MapIconScale { get { return _minimapCamera.orthographicSize / _cameraSizeRange.y * _mapIconMinScale; } }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Logic Functions
    private void MoveCameraToNetworkedPlayerOwnedCharacter()
    {
        _minimapCamera.transform.position = _networkedPlayer.OwnedCharacter.GetComponent<Character>().InterpolationTargetPosition;
        _minimapCamera.transform.position = new Vector3(_minimapCamera.transform.position.x, _minimapCamera.transform.position.y, -10);
    }

    private void TakeCameraSizeInput()
    {
        if (IsMouseOverMinimap()) _cameraSizeRangeRatio += -1 * _cameraSizeChangeSpeed * Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime;
        _cameraSizeRangeRatio = Mathf.Clamp01(_cameraSizeRangeRatio);
        _cameraTargetSize = Mathf.Lerp(_cameraSizeRange.x, _cameraSizeRange.y, _cameraSizeRangeRatio);
        _minimapCamera.orthographicSize = Mathf.Lerp(_minimapCamera.orthographicSize, _cameraTargetSize, _cameraSizeChangeLerp * Time.deltaTime);
    }

    //Keep the map icons' scale constant while zooming in & out
    private void UpdateScaleOfMapIcons()
    {
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
        _content.gameObject.SetActive(false);
        if (WorldMapGameUI.Instance.Active) return; //World Map Is Active
        if (!NetworkedPlayer.CanUseNetworkedPlayerOwnedCharacter(_networkedPlayer)) return;
        _content.gameObject.SetActive(true);

        MoveCameraToNetworkedPlayerOwnedCharacter();
        TakeCameraSizeInput();
        UpdateScaleOfMapIcons();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}
