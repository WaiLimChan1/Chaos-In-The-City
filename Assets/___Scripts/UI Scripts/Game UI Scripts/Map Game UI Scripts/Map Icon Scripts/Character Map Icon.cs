using CITC.GameManager;

public class CharacterMapIcon : MapIcon
{
    private PlayerSurvivor _playerSurvivor;

    protected new void Awake()
    {
        GameObjectListManager.Instance.Add(GameObjectListManager.Instance.AllMapIcons, this.gameObject, this.gameObject);
        _playerSurvivor = this.gameObject.GetComponentInParent<PlayerSurvivor>();
    }

    private void Update()
    {
        _content.SetActive(false);

        if (!PlayerSurvivor.CanUse(_playerSurvivor)) return;
        if (!NetworkedPlayer.CanUseNetworkedPlayer(_playerSurvivor.NetworkedPlayer)) return;

        _content.SetActive(true);

        //Calculate Sorting Orders
        int numOfIndexTaken = 2;
        int startingSortingOrder = _playerSurvivor.PlayerId * numOfIndexTaken + 1;
        _mapIconBackground.sortingOrder = startingSortingOrder;
        _mapIcon.sortingOrder = startingSortingOrder + 1;

        _mapIcon.color = _playerSurvivor.CharacterColor;
    }
}
