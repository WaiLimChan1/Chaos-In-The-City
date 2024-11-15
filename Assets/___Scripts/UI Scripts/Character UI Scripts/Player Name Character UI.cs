using TMPro;
using UnityEngine;

public class PlayerNameCharacterUI : MonoBehaviour
{
    private PlayerSurvivor _playerSurvivor;
    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private RectTransform _backgroundRectTransform;
    private float _padding = 1f;

    private void Awake()
    {
        _playerSurvivor = this.gameObject.GetComponentInParent<PlayerSurvivor>();
    }

    private void UpdateName(string name)
    {
        _playerNameText.text  = name;
        float textWidth = _playerNameText.preferredWidth;
        _backgroundRectTransform.sizeDelta = new Vector2(textWidth + _padding, _backgroundRectTransform.sizeDelta.y);
    }

    private void Update()
    {
        _playerNameText.color = Color.white; //Default Text Colors
        UpdateName("Player"); //Default

        if (!PlayerSurvivor.CanUse(_playerSurvivor)) return;
        if (!NetworkedPlayer.CanUseNetworkedPlayer(_playerSurvivor.NetworkedPlayer)) return;

        _playerNameText.color = _playerSurvivor.CharacterColor; //Text Color

        if (_playerSurvivor.Name == "") //Empty Name
        {
            UpdateName("Player " + _playerSurvivor.PlayerId);
            return;
        }
        UpdateName(_playerSurvivor.Name); //Name
    }
}
