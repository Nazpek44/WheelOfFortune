using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("Wheel")]
    [SerializeField] private Image wheelBaseImage;
    [SerializeField] private Image indicatorImage;
    [SerializeField] private RectTransform wheelRotator;
    [SerializeField] private WheelSlotView[] slots;

    [Header("Texts")]
    [SerializeField] private TMP_Text zoneTitleText_value;
    [SerializeField] private TMP_Text totalRewardText_value;

    [Header("Buttons")]
    [SerializeField] private Button spinButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button bombRestartButton;
    [SerializeField] private Button bombReviveButton;
    [SerializeField] private Button collectButton;

    [Header("Popups")]
    [SerializeField] private GameObject bombPopup;
    [SerializeField] private GameObject winPopup;
    [SerializeField] private TMP_Text winTotalText_value;

    [Header("Single Reward Reveal")]
    [SerializeField] private SingleRewardRevealView singleRewardRevealView;

    [Header("Win Reward List")]
    [SerializeField] private Transform winRewardsContent;
    [SerializeField] private WinRewardItemView winRewardItemTemplate;

    private readonly List<WinRewardItemView> spawnedWinRewardItems = new List<WinRewardItemView>();

    public Button SpinButton => spinButton;
    public Button LeaveButton => leaveButton;
    public Button RestartButton => restartButton;
    public Button BombRestartButton => bombRestartButton;
    public Button BombReviveButton => bombReviveButton;
    public Button CollectButton => collectButton;
    public RectTransform WheelRotator => wheelRotator;

    private void Awake()
    {
        CacheReferences();
        ValidateReferences();
    }

    private void OnValidate()
    {
        CacheReferences();
    }

    private void CacheReferences()
    {
        wheelBaseImage = FindByName<Image>("ui_image_spin_base", wheelBaseImage);
        indicatorImage = FindByName<Image>("ui_image_spin_indicator", indicatorImage);
        wheelRotator = FindByName<RectTransform>("ui_transform_spin_rotator", wheelRotator);

        zoneTitleText_value = FindByName<TMP_Text>("ui_text_zone_title_value", zoneTitleText_value);
        totalRewardText_value = FindByName<TMP_Text>("ui_text_total_reward_value", totalRewardText_value);

        spinButton = FindByName<Button>("ui_button_spin", spinButton);
        leaveButton = FindByName<Button>("ui_button_leave", leaveButton);

        restartButton = FindByName<Button>("ui_button_restart", restartButton);
        bombRestartButton = FindByName<Button>("ui_button_bomb_restart", bombRestartButton);
        bombReviveButton = FindByName<Button>("ui_button_bomb_revive", bombReviveButton);

        collectButton = FindByName<Button>("ui_button_collect", collectButton);
        collectButton = FindByName<Button>("ui_button_win_collect", collectButton);

        bombPopup = FindGameObjectByName("ui_popup_bomb", bombPopup);
        winPopup = FindGameObjectByName("ui_popup_win", winPopup);

        winTotalText_value = FindByName<TMP_Text>("ui_text_win_total_value", winTotalText_value);

        singleRewardRevealView = FindByName<SingleRewardRevealView>(
            "ui_panel_single_reward_reveal",
            singleRewardRevealView
        );

        if (winRewardsContent == null)
        {
            RectTransform content = FindByName<RectTransform>("ui_content_win_rewards", null);

            if (content != null)
                winRewardsContent = content;
        }

        winRewardItemTemplate = FindByName<WinRewardItemView>(
            "ui_item_win_reward_template",
            winRewardItemTemplate
        );

        if (slots == null || slots.Length == 0)
            slots = GetComponentsInChildren<WheelSlotView>(true);

        if (slots != null)
            Array.Sort(slots, (a, b) => string.CompareOrdinal(a.name, b.name));
    }

    private void ValidateReferences()
    {
        if (wheelBaseImage == null)
            Debug.LogWarning("GameUI missing reference: ui_image_spin_base");

        if (indicatorImage == null)
            Debug.LogWarning("GameUI missing reference: ui_image_spin_indicator");

        if (wheelRotator == null)
            Debug.LogWarning("GameUI missing reference: ui_transform_spin_rotator");

        if (spinButton == null)
            Debug.LogWarning("GameUI missing reference: ui_button_spin");

        if (leaveButton == null)
            Debug.LogWarning("GameUI missing reference: ui_button_leave");

        if (restartButton == null)
            Debug.LogWarning("GameUI missing reference: ui_button_restart");

        if (collectButton == null)
            Debug.LogWarning("GameUI missing reference: ui_button_collect or ui_button_win_collect");

        if (singleRewardRevealView == null)
            Debug.LogWarning("GameUI missing reference: ui_panel_single_reward_reveal");

        if (winRewardsContent == null)
            Debug.LogWarning("GameUI missing reference: ui_content_win_rewards");

        if (winRewardItemTemplate == null)
            Debug.LogWarning("GameUI missing reference: ui_item_win_reward_template");
    }

    private T FindByName<T>(string targetName, T currentValue) where T : Component
    {
        if (currentValue != null)
            return currentValue;

        T[] components = GetComponentsInChildren<T>(true);

        foreach (T component in components)
        {
            if (component.name == targetName)
                return component;
        }

        return null;
    }

    private GameObject FindGameObjectByName(string targetName, GameObject currentValue)
    {
        if (currentValue != null)
            return currentValue;

        Transform[] transforms = GetComponentsInChildren<Transform>(true);

        foreach (Transform child in transforms)
        {
            if (child.name == targetName)
                return child.gameObject;
        }

        return null;
    }

    public void SetWheel(WheelConfig config)
    {
        if (config == null)
        {
            Debug.LogWarning("SetWheel failed because config is null.");
            return;
        }

        if (wheelBaseImage != null)
        {
            wheelBaseImage.sprite = config.wheelBaseSprite;
            wheelBaseImage.raycastTarget = false;
        }

        if (indicatorImage != null)
        {
            indicatorImage.sprite = config.indicatorSprite;
            indicatorImage.raycastTarget = false;
        }

        if (slots == null)
            return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
                continue;

            if (config.slices != null && i < config.slices.Length)
                slots[i].SetSlice(config.slices[i]);
            else
                slots[i].Clear();
        }
    }

    public void SetZone(int zone, ZoneType zoneType)
    {
        if (zoneTitleText_value == null)
            return;

        string zoneName = zoneType switch
        {
            ZoneType.Safe => "SAFE ZONE",
            ZoneType.Super => "SUPER ZONE",
            _ => "ZONE"
        };

        zoneTitleText_value.text = $"{zoneName} {zone}";
    }

    public void SetTotalReward(int totalReward, int collectedCount)
    {
        if (totalRewardText_value != null)
            totalRewardText_value.text = $"TOTAL: {totalReward} | ITEMS: {collectedCount}";
    }

    public void SetButtons(bool canSpin, bool canLeave)
    {
        if (spinButton != null)
            spinButton.interactable = canSpin;

        if (leaveButton != null)
        {
            leaveButton.gameObject.SetActive(canLeave);
            leaveButton.interactable = canLeave;
        }

        if (restartButton != null)
            restartButton.interactable = true;
    }

    public void ShowSingleRewardReveal(WheelSlice reward)
    {
        if (reward == null)
        {
            Debug.LogWarning("ShowSingleRewardReveal failed because reward is null.");
            return;
        }

        if (singleRewardRevealView == null)
        {
            Debug.LogWarning("SingleRewardRevealView is missing in GameUI.");
            return;
        }

        Debug.Log("Showing single reward reveal: " + reward.rewardName);
        singleRewardRevealView.Show(reward);
    }

    public void ShowBombPopup(bool show)
    {
        if (bombPopup != null)
            bombPopup.SetActive(show);
    }

    public void ShowWinPopup(bool show, int totalReward, IReadOnlyList<WheelSlice> collectedRewards)
    {
        if (winPopup != null)
        {
            winPopup.SetActive(show);

            if (show)
                winPopup.transform.SetAsLastSibling();
        }

        if (!show)
        {
            ClearWinRewardItems();
            return;
        }

        if (collectButton != null)
            collectButton.interactable = true;

        if (winTotalText_value != null)
        {
            int count = collectedRewards == null ? 0 : collectedRewards.Count;
            winTotalText_value.text = $"TOTAL REWARD: {totalReward}\nITEMS: {count}";
        }

        BuildWinRewardList(collectedRewards);
    }

    private void BuildWinRewardList(IReadOnlyList<WheelSlice> collectedRewards)
    {
        ClearWinRewardItems();

        if (winRewardsContent == null)
        {
            Debug.LogWarning("Cannot build win reward list. ui_content_win_rewards is missing.");
            return;
        }

        if (winRewardItemTemplate == null)
        {
            Debug.LogWarning("Cannot build win reward list. ui_item_win_reward_template is missing.");
            return;
        }

        if (collectedRewards == null || collectedRewards.Count == 0)
        {
            Debug.Log("No collected rewards to show.");
            return;
        }

        winRewardItemTemplate.gameObject.SetActive(false);

        foreach (WheelSlice reward in collectedRewards)
        {
            WinRewardItemView item = Instantiate(winRewardItemTemplate, winRewardsContent);
            item.name = "ui_item_win_reward";
            item.SetReward(reward);

            spawnedWinRewardItems.Add(item);
        }

        Debug.Log("Built win reward list. Count: " + spawnedWinRewardItems.Count);
    }

    public IEnumerator PlayCollectEffect(Action onComplete)
    {
        Debug.Log("Collect effect started. Item count: " + spawnedWinRewardItems.Count);

        if (collectButton != null)
            collectButton.interactable = false;

        if (spawnedWinRewardItems.Count == 0)
        {
            yield return new WaitForSeconds(0.2f);
            onComplete?.Invoke();
            yield break;
        }

        float delayBetweenItems = 0.06f;

        for (int i = 0; i < spawnedWinRewardItems.Count; i++)
        {
            if (spawnedWinRewardItems[i] != null)
                StartCoroutine(AnimateCollectItem(spawnedWinRewardItems[i], i * delayBetweenItems));
        }

        float totalWait = spawnedWinRewardItems.Count * delayBetweenItems + 0.45f;
        yield return new WaitForSeconds(totalWait);

        onComplete?.Invoke();
    }

    private IEnumerator AnimateCollectItem(WinRewardItemView item, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (item == null)
            yield break;

        CanvasGroup canvasGroup = item.CanvasGroup;
        RectTransform rectTransform = item.transform as RectTransform;

        Vector3 startScale = Vector3.one;
        Vector3 endScale = Vector3.one * 0.15f;

        float duration = 0.35f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = timer / duration;
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            item.transform.localScale = Vector3.Lerp(startScale, endScale, eased);

            if (canvasGroup != null)
                canvasGroup.alpha = 1f - eased;

            if (rectTransform != null)
                rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, 8f, eased));

            yield return null;
        }

        item.gameObject.SetActive(false);
    }

    private void ClearWinRewardItems()
    {
        for (int i = spawnedWinRewardItems.Count - 1; i >= 0; i--)
        {
            if (spawnedWinRewardItems[i] != null)
                Destroy(spawnedWinRewardItems[i].gameObject);
        }

        spawnedWinRewardItems.Clear();
    }
}