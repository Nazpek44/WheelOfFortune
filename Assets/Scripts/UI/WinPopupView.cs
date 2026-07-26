using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.WheelOfFortune.Core;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.UI
{
    public sealed class WinPopupView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text totalText_value;
        [SerializeField] private ScrollRect rewardScrollRect;
        [SerializeField] private RectTransform rewardContent;
        [SerializeField] private CanvasGroup rewardContentCanvasGroup;
        [SerializeField] private WinRewardItemView rewardItemTemplate;
        [SerializeField] private Button collectButton;

        private readonly List<WinRewardItemView> spawnedItems = new List<WinRewardItemView>();
        private Coroutine rebuildRoutine;

        private void Awake()
        {
            CacheReferences();
            Hide();
        }

        private void OnValidate()
        {
            CacheReferences();
        }

        private void CacheReferences()
        {
            if (root == null)
                root = gameObject;

            if (totalText_value == null)
                totalText_value = ComponentFinder.FindChildByName<TMP_Text>(this, "ui_text_win_total_value");

            if (rewardScrollRect == null)
                rewardScrollRect = ComponentFinder.FindChildByName<ScrollRect>(this, "ui_scroll_win_rewards");

            if (rewardContent == null)
                rewardContent = ComponentFinder.FindChildByName<RectTransform>(this, "ui_content_win_rewards");

            if (rewardContent != null && rewardContentCanvasGroup == null)
            {
                rewardContentCanvasGroup = rewardContent.GetComponent<CanvasGroup>();

                if (rewardContentCanvasGroup == null)
                    rewardContentCanvasGroup = rewardContent.gameObject.AddComponent<CanvasGroup>();
            }

            if (rewardItemTemplate == null)
                rewardItemTemplate = ComponentFinder.FindChildByName<WinRewardItemView>(this, "ui_item_win_reward_template");

            if (collectButton == null)
                collectButton = ComponentFinder.FindChildByName<Button>(this, "ui_button_collect");
        }

        public void Show(int totalReward, int itemCount, IReadOnlyList<InventoryEntry> entries)
        {
            CacheReferences();

            root.SetActive(true);
            root.transform.SetAsLastSibling();

            if (totalText_value != null)
                totalText_value.text = RewardTextFormatter.FormatWinTotal(totalReward, itemCount);

            BuildRewardList(entries);

            if (collectButton != null)
                collectButton.interactable = true;

            if (rewardContentCanvasGroup != null)
                rewardContentCanvasGroup.alpha = 1f;

            if (rewardContent != null)
                rewardContent.localScale = Vector3.one;
        }

        public void Hide()
        {
            ClearRewardList();

            if (root != null)
                root.SetActive(false);
        }

        public IEnumerator PlayCollectEffect()
        {
            if (collectButton != null)
                collectButton.interactable = false;

            if (rewardContentCanvasGroup == null || rewardContent == null)
            {
                yield return new WaitForSeconds(0.2f);
                yield break;
            }

            float duration = 0.45f;
            float timer = 0f;

            Vector3 startScale = Vector3.one;
            Vector3 endScale = Vector3.one * 0.88f;

            while (timer < duration)
            {
                timer += Time.deltaTime;

                float t = Mathf.Clamp01(timer / duration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);

                rewardContent.localScale = Vector3.Lerp(startScale, endScale, eased);
                rewardContentCanvasGroup.alpha = 1f - eased;

                yield return null;
            }

            rewardContent.localScale = Vector3.one;
            rewardContentCanvasGroup.alpha = 0f;
        }

        private void BuildRewardList(IReadOnlyList<InventoryEntry> entries)
        {
            ClearRewardList();

            if (rewardContent == null || rewardItemTemplate == null)
            {
                Debug.LogWarning("WinPopupView cannot build rewards. Content or template missing.");
                return;
            }

            rewardItemTemplate.gameObject.SetActive(false);

            if (entries != null)
            {
                foreach (InventoryEntry entry in entries)
                {
                    WinRewardItemView item = Instantiate(rewardItemTemplate, rewardContent);
                    item.name = "ui_item_win_reward";
                    item.SetEntry(entry);
                    spawnedItems.Add(item);
                }
            }

            if (rebuildRoutine != null)
                StopCoroutine(rebuildRoutine);

            rebuildRoutine = StartCoroutine(RebuildScrollNextFrame());

            Debug.Log("WinPopupView built grouped reward list. Count: " + spawnedItems.Count);
        }

        private IEnumerator RebuildScrollNextFrame()
        {
            yield return null;

            Canvas.ForceUpdateCanvases();

            if (rewardContent != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(rewardContent);

            Canvas.ForceUpdateCanvases();

            if (rewardScrollRect != null)
            {
                rewardScrollRect.horizontal = true;
                rewardScrollRect.vertical = false;
                rewardScrollRect.horizontalScrollbar = null;
                rewardScrollRect.verticalScrollbar = null;
                rewardScrollRect.normalizedPosition = new Vector2(0f, 1f);
            }
        }

        private void ClearRewardList()
        {
            for (int i = spawnedItems.Count - 1; i >= 0; i--)
            {
                if (spawnedItems[i] != null)
                    Destroy(spawnedItems[i].gameObject);
            }

            spawnedItems.Clear();
        }
    }
}