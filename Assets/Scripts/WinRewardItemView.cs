using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinRewardItemView : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image rewardIcon;
    [SerializeField] private TMP_Text rewardNameText_value;
    [SerializeField] private TMP_Text rewardAmountText_value;

    public CanvasGroup CanvasGroup => canvasGroup;

    private void OnValidate()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (rewardIcon == null)
            rewardIcon = FindChild<Image>("ui_image_reward_icon");

        if (rewardNameText_value == null)
            rewardNameText_value = FindChild<TMP_Text>("ui_text_reward_name_value");

        if (rewardAmountText_value == null)
            rewardAmountText_value = FindChild<TMP_Text>("ui_text_reward_amount_value");
    }

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void SetReward(WheelSlice reward)
    {
        gameObject.SetActive(true);
        transform.localScale = Vector3.one;

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        if (rewardIcon != null)
        {
            rewardIcon.sprite = reward.icon;
            rewardIcon.enabled = reward.icon != null;
            rewardIcon.raycastTarget = false;
        }

        if (rewardNameText_value != null)
            rewardNameText_value.text = reward.rewardName;

        if (rewardAmountText_value != null)
            rewardAmountText_value.text = reward.GetDisplayText();
    }

    private T FindChild<T>(string childName) where T : Component
    {
        T[] children = GetComponentsInChildren<T>(true);

        foreach (T child in children)
        {
            if (child.name == childName)
                return child;
        }

        return null;
    }
}