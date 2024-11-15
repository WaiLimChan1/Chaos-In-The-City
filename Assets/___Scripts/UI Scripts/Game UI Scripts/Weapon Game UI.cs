using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponGameUI : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Components")]
    [SerializeField] private GameObject _content;
    [SerializeField] private Image _background;
    [SerializeField] private TextMeshProUGUI _keybind;
    [SerializeField] private Image _weaponImage;

    [Header("Components - Only Show When Chosen")]
    [SerializeField] private GameObject _chosenContent;
    [SerializeField] private TextMeshProUGUI _weaponName;
    [SerializeField] private TextMeshProUGUI _weaponClipSize;
    [SerializeField] private TextMeshProUGUI _weaponAmmo;

    [Header("Colors")]
    [SerializeField] private Color RegularBackgroundColor;
    [SerializeField] private Color ChosenBackgroundColor;
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------
    //Initialization
    public void Awake()
    {
        Inactivate();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------



    //------------------------------------------------------------------------------------------------------------------------------------------------       
    public void Inactivate()
    {
        _content.gameObject.SetActive(false);
    }
    
    public void UpdateUI(Weapon weapon, string keybind)
    {
        _content.gameObject.SetActive(false);
        _chosenContent.gameObject.SetActive(false);

        if (weapon == null) return;

        _content.gameObject.SetActive(true);

        _background.color = RegularBackgroundColor;
        _keybind.text = keybind;
        _weaponImage.sprite = weapon.WeaponImage;

        _weaponName.text = weapon.WeaponName;
        _weaponClipSize.text = weapon.ClipSizeString;
        _weaponAmmo.text = weapon.AmmoString;
    }

    public void UpdateChosenUI()
    {
        _chosenContent.gameObject.SetActive(true);
        
        _background.color = ChosenBackgroundColor;
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
}
