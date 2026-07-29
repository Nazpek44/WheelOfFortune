using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.WheelOfFortune.Economy;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.UI
{
    public sealed class WinPopupViewMono : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _summaryText;
        [SerializeField] private ScrollRect _rewardScrollRect;
        [SerializeField] private RectTransform _rewardContent;
        [SerializeField] private CanvasGroup _rewardContentCanvasGroup;
        [SerializeField] private WinRewardItemViewMono _rewardItemTemplate;
        [SerializeField] private Button _collectButton;

        [Header("Collect Effect")]
        [Tooltip("Where the rewards fly to. Normally the wallet HUD.")]
        [SerializeField] private RectTransform _collectTarget;

        [Tooltip("Rewards are reparented here so the scroll mask cannot clip them.")]
        [SerializeField] private RectTransform _flightLayer;

        [Range(0.05f, 0.6f)]
        [SerializeField] private float _popDuration = 0.16f;

        [Range(0f, 0.3f)]
        [SerializeField] private float _stagger = 0.06f;

        [Range(0.15f, 1.5f)]
        [SerializeField] private float _flightDuration = 0.45f;

        [Tooltip("How high the rewards arc on their way to the wallet.")]
        [Range(0f, 400f)]
        [SerializeField] private float _arcHeight = 140f;

        private readonly List<WinRewardItemViewMono> _spawnedItems = new List<WinRewardItemViewMono>();
        private readonly List<RewardFlight> _flights = new List<RewardFlight>();
        private readonly StringBuilder _summaryBuilder = new StringBuilder();

        private Coroutine _rebuildCoroutine;

        private void Reset()
        {
            _root = gameObject;
            _summaryText = ComponentFinder.FindChildByName<TMP_Text>(this, "ui_text_win_subtitle_value");
            _rewardScrollRect = ComponentFinder.FindChildByName<ScrollRect>(this, "ui_scroll_win_rewards");
            _rewardContent = ComponentFinder.FindChildByName<RectTransform>(this, "ui_content_win_rewards");
            _rewardItemTemplate = ComponentFinder.FindChildByName<WinRewardItemViewMono>(this, "ui_item_win_reward_template");
            _collectButton = ComponentFinder.FindChildByName<Button>(this, "ui_button_collect");
            _flightLayer = transform as RectTransform;
        }

        private void Awake()
        {
            if (!_root)
                _root = gameObject;

            if (_rewardContent && !_rewardContentCanvasGroup)
            {
                _rewardContentCanvasGroup = _rewardContent.GetComponent<CanvasGroup>();

                if (!_rewardContentCanvasGroup)
                    _rewardContentCanvasGroup = _rewardContent.gameObject.AddComponent<CanvasGroup>();
            }

            Hide();
        }

        private void OnDestroy()
        {
            StopRebuild();
        }

        /// <summary>
        /// Shows what the run earned, broken down per currency. There is no
        /// single "total reward" figure: cash and gold are different things.
        /// </summary>
        public void Show(IRunRewardInventory runInventory, IPlayerWallet wallet)
        {
            if (runInventory == null)
                return;

            if (!_root)
                _root = gameObject;

            _root.SetActive(true);
            _root.transform.SetAsLastSibling();

            if (_summaryText)
                _summaryText.text = BuildSummary(runInventory, wallet);

            BuildRewardList(runInventory.Entries);

            if (_collectButton)
                _collectButton.interactable = true;

            if (_rewardContentCanvasGroup)
                _rewardContentCanvasGroup.alpha = 1f;

            if (_rewardContent)
                _rewardContent.localScale = Vector3.one;
        }

        public void Hide()
        {
            ClearRewardList();

            if (!_root)
                _root = gameObject;

            _root.SetActive(false);
        }

        private string BuildSummary(IRunRewardInventory runInventory, IPlayerWallet wallet)
        {
            _summaryBuilder.Length = 0;

            long cash = runInventory.GetCurrencyTotal(CurrencyType.Cash);
            long gold = runInventory.GetCurrencyTotal(CurrencyType.Gold);

            _summaryBuilder.Append("CASH ").Append(RewardTextFormatter.FormatBalance(cash));
            _summaryBuilder.Append("   GOLD ").Append(RewardTextFormatter.FormatBalance(gold));
            _summaryBuilder.Append("   ITEMS ").Append(runInventory.CollectedCount);

            if (wallet != null)
            {
                _summaryBuilder.AppendLine();
                _summaryBuilder.Append("WALLET AFTER COLLECT: ");
                _summaryBuilder.Append(RewardTextFormatter.FormatBalance(wallet.GetBalance(CurrencyType.Cash) + cash));
                _summaryBuilder.Append(" CASH / ");
                _summaryBuilder.Append(RewardTextFormatter.FormatBalance(wallet.GetBalance(CurrencyType.Gold) + gold));
                _summaryBuilder.Append(" GOLD");
            }

            return _summaryBuilder.ToString();
        }

        /// <summary>
        /// Each reward pops, then arcs across to the wallet on a staggered
        /// delay, shrinking and fading as it lands. Items are reparented to the
        /// flight layer first: the scroll viewport has a Mask and the content a
        /// HorizontalLayoutGroup, either of which would otherwise clip them or
        /// drag them back into place mid animation.
        /// </summary>
        public IEnumerator PlayCollectEffectCoroutine()
        {
            if (_collectButton)
                _collectButton.interactable = false;

            PrepareFlights();

            if (_flights.Count == 0)
            {
                yield return FadeContentCoroutine();
                yield break;
            }

            float lastStart = _popDuration + _stagger * (_flights.Count - 1);
            float totalDuration = lastStart + _flightDuration;
            float elapsed = 0f;

            while (elapsed < totalDuration)
            {
                elapsed += Time.deltaTime;

                for (int i = 0; i < _flights.Count; i++)
                    _flights[i].Evaluate(elapsed, _popDuration, _flightDuration);

                yield return null;
            }

            for (int i = 0; i < _flights.Count; i++)
                _flights[i].Finish();

            _flights.Clear();

            yield return FadeContentCoroutine();
        }

        private void PrepareFlights()
        {
            _flights.Clear();

            RectTransform layer = _flightLayer ? _flightLayer : transform as RectTransform;

            if (!layer)
                return;

            Vector3 targetLocal = _collectTarget
                ? layer.InverseTransformPoint(_collectTarget.position)
                : Vector3.zero;

            for (int i = 0; i < _spawnedItems.Count; i++)
            {
                WinRewardItemViewMono item = _spawnedItems[i];

                if (!item)
                    continue;

                RectTransform rect = item.transform as RectTransform;

                if (!rect)
                    continue;

                Vector3 worldPosition = rect.position;

                rect.SetParent(layer, true);
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.localPosition = layer.InverseTransformPoint(worldPosition);

                _flights.Add(new RewardFlight(
                    rect,
                    item.CanvasGroup,
                    rect.localPosition,
                    targetLocal,
                    _arcHeight,
                    _stagger * _flights.Count
                ));
            }
        }

        private IEnumerator FadeContentCoroutine()
        {
            if (!_rewardContentCanvasGroup || !_rewardContent)
            {
                yield return new WaitForSeconds(0.1f);
                yield break;
            }

            float duration = 0.2f;
            float timer = 0f;
            float startAlpha = _rewardContentCanvasGroup.alpha;

            while (timer < duration)
            {
                timer += Time.deltaTime;

                float progress = Mathf.Clamp01(timer / duration);

                _rewardContentCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, progress);

                yield return null;
            }

            _rewardContent.localScale = Vector3.one;
            _rewardContentCanvasGroup.alpha = 0f;
        }

        private void BuildRewardList(IReadOnlyList<RewardEntry> entries)
        {
            ClearRewardList();

            if (!_rewardContent || !_rewardItemTemplate)
            {
                Debug.LogWarning("WinPopupViewMono cannot build rewards. Content or template missing.", this);
                return;
            }

            _rewardItemTemplate.gameObject.SetActive(false);

            if (entries != null)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    WinRewardItemViewMono item = Instantiate(_rewardItemTemplate, _rewardContent);
                    item.name = "ui_item_win_reward";
                    item.SetEntry(entries[i]);
                    _spawnedItems.Add(item);
                }
            }

            StopRebuild();

            if (isActiveAndEnabled)
                _rebuildCoroutine = StartCoroutine(RebuildScrollCoroutine());
        }

        private IEnumerator RebuildScrollCoroutine()
        {
            yield return null;

            Canvas.ForceUpdateCanvases();

            if (_rewardContent)
                LayoutRebuilder.ForceRebuildLayoutImmediate(_rewardContent);

            Canvas.ForceUpdateCanvases();

            if (_rewardScrollRect)
            {
                _rewardScrollRect.horizontal = true;
                _rewardScrollRect.vertical = false;
                _rewardScrollRect.horizontalScrollbar = null;
                _rewardScrollRect.verticalScrollbar = null;
                _rewardScrollRect.normalizedPosition = new Vector2(0f, 1f);
            }

            _rebuildCoroutine = null;
        }

        private void StopRebuild()
        {
            if (_rebuildCoroutine != null)
                StopCoroutine(_rebuildCoroutine);

            _rebuildCoroutine = null;
        }

        private void ClearRewardList()
        {
            for (int i = _spawnedItems.Count - 1; i >= 0; i--)
            {
                if (_spawnedItems[i])
                    Destroy(_spawnedItems[i].gameObject);
            }

            _spawnedItems.Clear();
        }
    }
}
