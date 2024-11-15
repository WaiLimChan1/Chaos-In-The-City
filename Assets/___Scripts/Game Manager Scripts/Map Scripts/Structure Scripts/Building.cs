using UnityEngine;
using CITC.GameManager;

public class Building : Structure
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    public static int NUM_SORTING_ORDER = 1;
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Components")]
    [SerializeField] private SpriteRenderer buildingSpriteRenderer;
    [SerializeField] private SpriteRenderer floorSpriteRenderer;
    private int _startingSortingOrder;

    [SerializeField] private BoxCollider2D buildingInteriorHitBox;
    //------------------------------------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Initialization
    private void Awake()
    {
        GameObjectListManager.Instance.Add(GameObjectListManager.Instance.AllBuildings, this.gameObject);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Helper Function
    public void RevealInterior() { buildingSpriteRenderer.color = new Color(buildingSpriteRenderer.color.r, buildingSpriteRenderer.color.g, buildingSpriteRenderer.color.b, 0.25f); }
    public void HideInterior() { buildingSpriteRenderer.color = new Color(buildingSpriteRenderer.color.r, buildingSpriteRenderer.color.g, buildingSpriteRenderer.color.b, 1f); }
    public int StartingSortingOrder
    {
        get { return _startingSortingOrder; }
        set
        {
            if (_startingSortingOrder == value) return;
            _startingSortingOrder = value;
            buildingSpriteRenderer.sortingOrder = value;
        }
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}
