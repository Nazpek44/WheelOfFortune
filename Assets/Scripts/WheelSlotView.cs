using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WheelSlotView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text amountText_value;

    private void OnValidate()
    {
        if (iconImage == null)
            iconImage = GetComponentInChildren<Image>(true);

        if (amountText_value == null)
            amountText_value = GetComponentInChildren<TMP_Text>(true);
    }

    public void SetSlice(WheelSlice slice)
    {
        gameObject.SetActive(true);

        if (iconImage != null)
        {
            iconImage.sprite = slice.icon;
            iconImage.enabled = slice.icon != null;
            iconImage.raycastTarget = false;
        }

        if (amountText_value != null)
        {
            amountText_value.text = slice.GetDisplayText();
        }
    }

    public void Clear()
    {
        gameObject.SetActive(false);
    }
}