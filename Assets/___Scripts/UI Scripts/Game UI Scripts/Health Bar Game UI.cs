using UnityEngine;
using UnityEngine.UI;

public class HealthBarGameUI : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Static Variables
    public static HealthBarGameUI Instance { get; private set; }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Variable")]
    private NetworkedPlayer _networkedPlayer;

    [Header("Components")]
    [SerializeField] private GameObject _content;
    [SerializeField] private Image _healthBar;

    private float lerpRatio = 0.3f; 
    private float targetFillAmount;

    [Header("Colors")]
    [SerializeField] private Color _fullHealthColor;
    [SerializeField] private Color _zeroHealthColor;
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
    private void LateUpdate()
    {
        _content.gameObject.SetActive(false);
        if (!NetworkedPlayer.CanUseNetworkedPlayerOwnedCharacter(_networkedPlayer)) return;

        _content.gameObject.SetActive(true);
        targetFillAmount = _networkedPlayer.OwnedCharacter.GetComponent<Character>().HealthRatio;
        _healthBar.fillAmount = Mathf.Lerp(_healthBar.fillAmount, targetFillAmount, lerpRatio);
        _healthBar.color = Color.Lerp(_zeroHealthColor, _fullHealthColor, _healthBar.fillAmount);

    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}
