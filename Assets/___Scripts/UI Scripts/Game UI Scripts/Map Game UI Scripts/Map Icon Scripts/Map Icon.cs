using CITC.GameManager;
using UnityEngine;

public class MapIcon : MonoBehaviour
{
    [SerializeField] protected GameObject _content;
    [SerializeField] protected SpriteRenderer _mapIconBackground;
    [SerializeField] protected SpriteRenderer _mapIcon;

    protected void Awake()
    {
        GameObjectListManager.Instance.Add(GameObjectListManager.Instance.AllMapIcons, this.gameObject);
    }

    public void UpdateScale(float newScale)
    {
        _content.transform.localScale = new Vector3(newScale, newScale, 1);
    }
}
