using UnityEngine;

public class AnimationController : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Animation Controller Components")]
    protected Animator animator;
    protected AnimatorOverrideController animationOverrideController;

    protected SpriteRenderer spriteRenderer;
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Helper Functions
    public int SortingOrder
    {
        get { return spriteRenderer.sortingOrder; }
        set { spriteRenderer.sortingOrder = value; }
    }

    public bool FlipX
    {
        get { return spriteRenderer.flipX; }
        set { spriteRenderer.flipX = value; }
    }

    public bool FlipY
    {
        get { return spriteRenderer.flipY; }
        set { spriteRenderer.flipY = value; }
    }

    public float NormalizedTime
    {
        get { return animator.GetCurrentAnimatorStateInfo(0).normalizedTime; }
        set { animator.Play(animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, value); }
    }

    public float AnimationSpeed
    {
        get { return animator.speed; }
        set { animator.speed = value; }
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Initialization
    protected void Awake()
    {
        animator = GetComponent<Animator>();
        animationOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animationOverrideController;

        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Functions
    public void UpdateStatus(int statusValue)
    {
        animator.SetInteger("Status", statusValue);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}
