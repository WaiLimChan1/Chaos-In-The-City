using CITC.GameManager;
using UnityEngine;
using static CITC.GameManager.SingleAnimationClipManager;

public class MyVisualEffect : MonoBehaviour
{
    private SingleAnimationController _singleAnimationController;

    private void Awake()
    {
        GameObjectListManager.Instance.Add(GameObjectListManager.Instance.AllVisualEffects, this.gameObject);

        _singleAnimationController = GetComponentInChildren<SingleAnimationController>();
    }

    public void SetUp(SingleAnimationType singleAnimationType, Vector3 position, float scale = 1, float rotationDegree = 0, bool flipY = false, int sortingOrder = 0, Transform parent = null)
    {
        transform.position = position;
        transform.localScale = new Vector3(scale, scale, 1);
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotationDegree);
        if (parent != null) this.transform.parent = parent;

        _singleAnimationController.ChangeAnimationClips(singleAnimationType);
        _singleAnimationController.FlipY = flipY;
        _singleAnimationController.SortingOrder = sortingOrder;
        _singleAnimationController.NormalizedTime = 0;
        _singleAnimationController.UpdateStatus(0);
    }

    private void Update()
    {
        if (_singleAnimationController.NormalizedTime >= 1f) HandleDestruction();
    }

    private void HandleDestruction()
    {
        Destroy(gameObject);
    }
}
