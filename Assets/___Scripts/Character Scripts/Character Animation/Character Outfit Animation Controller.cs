using UnityEngine;
using CITC.GameManager;

public class CharacterOutfitAnimationController : MonoBehaviour
{
    [Header("Component Arrays")]
    public SpriteRenderer[] SpriteRenderersSortingLayerOrderList;

    [Header("Animation Controllers")]
    [SerializeField] private Animator _outfitTransformAnimator;
    [SerializeField] private IdleAndWalkAnimationController _bodyAC;

    [Header("Unanimated Outfit SpriteRenderers")]
    [SerializeField] private SpriteRenderer _backpackSR;
    [SerializeField] private SpriteRenderer _shirtSR;
    [SerializeField] private SpriteRenderer _hairSR;
    [SerializeField] private SpriteRenderer _mustacheSR;
    [SerializeField] private SpriteRenderer _maskSR;

    [Header("Variables")]
    [SerializeField] private int _startingSortingOrder;

    public int StartingSortingOrder { get { return _startingSortingOrder; } set { _startingSortingOrder = value; UpdateSortingLayer();  } }

    private void UpdateSortingLayer()
    {
        for (int i = 0; i < SpriteRenderersSortingLayerOrderList.Length; i++)
            SpriteRenderersSortingLayerOrderList[i].sortingOrder = _startingSortingOrder + i;
    }

    private void UpdateAnimationStatus(Character.Status status)
    {
        _outfitTransformAnimator.SetInteger("Status", (int)status);
        _bodyAC.UpdateStatus((int)status);
    }

    private void SyncAnimation()
    {
        if (_bodyAC.NormalizedTime != _outfitTransformAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime)
        {
            _outfitTransformAnimator.Play(_outfitTransformAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 0.0f);
            _bodyAC.NormalizedTime = 0.0f;
        }
    }

    private void UpdateAnimationFlipX(bool isFacingLeft)
    {
        for (int i = 0; i < SpriteRenderersSortingLayerOrderList.Length; i++)
            SpriteRenderersSortingLayerOrderList[i].flipX = isFacingLeft;
    }

    private void UpdateSprite(SpriteRenderer outfitPart, Sprite[] spriteList, int index)
    {
        if (index <= -1 || index > spriteList.Length)
        {
            outfitPart.gameObject.SetActive(false);
            return;
        }

        outfitPart.gameObject.SetActive(true);
        outfitPart.sprite = spriteList[index];
    }

    private void UpdateOutfit(OutfitManager.Outfit outfit)
    {
        OutfitManager outfitManager = OutfitManager.Instance;
        _bodyAC.ChangeAnimationClips(outfitManager.BodyOutfits, outfit.BodyIndex);

        UpdateSprite(_backpackSR, outfitManager.BackpackOutfits, outfit.BackpackIndex);
        UpdateSprite(_shirtSR, outfitManager.ShirtOutfits, outfit.ShirtIndex);
        UpdateSprite(_hairSR, outfitManager.HairOutfits, outfit.HairIndex);
        UpdateSprite(_mustacheSR, outfitManager.MustacheOutfits, outfit.MustacheIndex);
        UpdateSprite(_maskSR, outfitManager.MaskOutfits, outfit.MaskIndex);
    }

    public void UpdateAnimation(Character.Status status, bool isFacingLeft, OutfitManager.Outfit outfit)
    {
        UpdateSortingLayer();
        UpdateAnimationStatus(status);
        SyncAnimation();
        UpdateAnimationFlipX(isFacingLeft);
        UpdateOutfit(outfit);
    }
}
