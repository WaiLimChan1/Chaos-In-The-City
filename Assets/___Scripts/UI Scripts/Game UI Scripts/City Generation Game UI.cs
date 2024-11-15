using CITC.GameManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CityGenerationGameUI : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Static Variables
    public static CityGenerationGameUI Instance { get; private set; }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Components")]
    [SerializeField] private GameObject _content;
    [SerializeField] private Image _cityGenerationProgressBar;
    [SerializeField] private TextMeshProUGUI _cityGenerationProgressText;
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Initialization
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
        if (CityGenerator.Instance.CityGenerationCompleted) return;

        _content.gameObject.SetActive(true);
        _cityGenerationProgressBar.fillAmount = Mathf.Clamp01(CityGenerator.Instance.CityGeneratonProgress);
        _cityGenerationProgressText.text = (int) (Mathf.Clamp01(CityGenerator.Instance.CityGeneratonProgress) * 100) + "%";

    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}
