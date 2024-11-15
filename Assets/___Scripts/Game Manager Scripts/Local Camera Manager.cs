using Fusion;
using UnityEngine;

namespace CITC.GameManager
{
    public class LocalCameraManager : MonoBehaviour
    {
        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Static Variables
        public static LocalCameraManager Instance { get; private set; }
        //------------------------------------------------------------------------------------------------------------------------------------------------



        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Member Variables
        [Header("Components")]
        private Camera _camera;

        [Header("Variable")]
        private NetworkedPlayer _networkedPlayer;

        [Header("Camera Stats")]
        private Vector2 _cameraSizeRange = new Vector2(5, 20);
        //private Vector2 _cameraSizeRange = new Vector2(5, 1100); //Bigger Range For Testing
        private float _cameraSizeRangeRatio = 1;
        private float _cameraTargetSize;

        private float _cameraSizeChangeSpeed = 500f;
        private float _cameraSizeChangeLerp = 5f;

        private float visibilityBuffer = 2;
        private float _cameraSpeed = 25f;
        //private float _cameraSpeed = 250f; //Faster Speed For Testing

        [Header("Camera Vehicle Variables")]
        private float cameraSizeVehicleMultiplier = 1.25f;
        private float cameraSizeVehicleForwardVelocityMultiplier = 1 / 3f;

        [Header("Camera Building Variables")]
        private float cameraSizeInsideBuildingMultiplier = 0.25f;
        //------------------------------------------------------------------------------------------------------------------------------------------------



        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Initialization
        public void SetUp(NetworkedPlayer networkedPlayer)
        {
            _networkedPlayer = networkedPlayer;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                _camera = Camera.main;
                _cameraTargetSize = _camera.orthographicSize;
            }
            else Destroy(this);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------



        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Helper Functions
        public Vector2 CameraPoint { get { return _camera.transform.position; } }
        public Vector2 CameraBoundsSize
        {
            get 
            {
                float height = 2f * _camera.orthographicSize + visibilityBuffer;
                float width = height * _camera.aspect + visibilityBuffer;
                return new Vector2(width, height);
            }
        }
        public Bounds CameraBounds { get { return new Bounds(CameraPoint, CameraBoundsSize); } }
        //------------------------------------------------------------------------------------------------------------------------------------------------



        //------------------------------------------------------------------------------------------------------------------------------------------------
        //Logic Functions
        private void TakeCameraSizeInput()
        {
            if (!MinimapGameUI.Instance.IsMouseOverMinimap() && !WorldMapGameUI.Instance.IsMouseOverWorldMap())
                _cameraSizeRangeRatio += -1 * _cameraSizeChangeSpeed * Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime;
            _cameraSizeRangeRatio = Mathf.Clamp01(_cameraSizeRangeRatio);

            _cameraTargetSize = Mathf.Lerp(_cameraSizeRange.x, _cameraSizeRange.y, _cameraSizeRangeRatio);
            _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _cameraTargetSize, _cameraSizeChangeLerp * Time.deltaTime);
        }

        private void MoveCameraToNetworkedPlayerOwnedCharacter()
        {
            if (!NetworkedPlayer.CanUseNetworkedPlayerOwnedCharacter(_networkedPlayer)) return;
            
            Vector3 cameraPosition = _networkedPlayer.OwnedCharacter.GetComponent<Character>().CameraPosition;
            cameraPosition = new Vector3(cameraPosition.x, cameraPosition.y, -10);
            _camera.transform.position = cameraPosition;
        }

        private void TakeCameraMovementInput()
        {
            if (NetworkedPlayer.CanUseNetworkedPlayerOwnedCharacter(_networkedPlayer)) return;

            Vector3 direction = new Vector3(0, 0, 0);
            if (Input.GetKey(KeyCode.W)) direction.y += 1;
            if (Input.GetKey(KeyCode.S)) direction.y -= 1;
            if (Input.GetKey(KeyCode.D)) direction.x += 1;
            if (Input.GetKey(KeyCode.A)) direction.x -= 1;
            direction = direction.normalized;

            _camera.transform.position += direction * _cameraSpeed * Time.deltaTime;
        }

        //If character is in a vehicle, double the camera size. And increase camera size depending on Vehicle's forward velocity
        private void UpdateTargetCameraSizeForVehicle()
        {
            if (!NetworkedPlayer.CanUseNetworkedPlayerOwnedCharacter(_networkedPlayer)) return;
            if (!Vehicle.CanUse(_networkedPlayer.OwnedCharacter.GetComponent<Character>().CurrentVehicle)) return;

            _cameraTargetSize *= cameraSizeVehicleMultiplier;
            _cameraTargetSize += Mathf.Abs(_networkedPlayer.OwnedCharacter.GetComponent<Character>().CurrentVehicle.ForwardVelocity) * cameraSizeVehicleForwardVelocityMultiplier;
        }

        private void UpdateTargetCameraSizeForInsideBuilding()
        {
            if (!NetworkedPlayer.CanUseNetworkedPlayerOwnedCharacter(_networkedPlayer)) return;
            if (!_networkedPlayer.OwnedCharacter.GetComponent<PlayerSurvivor>().InsideBuilding) return;

            _cameraTargetSize *= cameraSizeInsideBuildingMultiplier;
        }

        private void UpdateCameraSizeBasedOnCameraTargetSize()
        {
            _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _cameraTargetSize, _cameraSizeChangeLerp * Time.deltaTime);
        }

        private void LateUpdate()
        {
            TakeCameraSizeInput();
            MoveCameraToNetworkedPlayerOwnedCharacter();
            TakeCameraMovementInput();
            UpdateTargetCameraSizeForVehicle();
            UpdateTargetCameraSizeForInsideBuilding();
            UpdateCameraSizeBasedOnCameraTargetSize();
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------
    }
}