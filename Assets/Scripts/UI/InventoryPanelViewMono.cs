using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.WheelOfFortune.Economy;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.UI
{
    public sealed class InventoryPanelViewMono : MonoBehaviour
    {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _viewport;
        [SerializeField] private RectTransform _content;
        [SerializeField] private InventoryItemViewMono _itemTemplate;

        private readonly List<InventoryItemViewMono> _spawnedItems = new List<InventoryItemViewMono>();

        private Coroutine _rebuildCoroutine;

        private void Reset()
        {
            _scrollRect = ComponentFinder.FindChildByName<ScrollRect>(this, "ui_scroll_inventory_items");
            _viewport = ComponentFinder.FindChildByName<RectTransform>(this, "ui_viewport_inventory_items");
            _content = ComponentFinder.FindChildByName<RectTransform>(this, "ui_content_inventory_items");
            _itemTemplate = ComponentFinder.FindChildByName<InventoryItemViewMono>(this, "ui_item_inventory_reward_template");
        }

        private void OnDestroy()
        {
            StopRebuild();
        }

        public void Refresh(IReadOnlyList<RewardEntry> entries)
        {
            Clear();

            if (!_content || !_itemTemplate)
            {
                Debug.LogWarning("InventoryPanelViewMono is missing its content or template.", this);
                return;
            }

            _itemTemplate.gameObject.SetActive(false);

            if (entries != null)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    InventoryItemViewMono item = Instantiate(_itemTemplate, _content);
                    item.name = "ui_item_inventory_reward";
                    item.SetData(entries[i]);
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

            if (_content)
                LayoutRebuilder.ForceRebuildLayoutImmediate(_content);

            Canvas.ForceUpdateCanvases();

            if (_scrollRect)
            {
                _scrollRect.horizontal = false;
                _scrollRect.vertical = true;
                _scrollRect.verticalScrollbar = null;
                _scrollRect.horizontalScrollbar = null;

                _scrollRect.content = _content;
                _scrollRect.viewport = _viewport;

                _scrollRect.normalizedPosition = new Vector2(0f, 1f);
            }

            _rebuildCoroutine = null;
        }

        private void StopRebuild()
        {
            if (_rebuildCoroutine != null)
                StopCoroutine(_rebuildCoroutine);

            _rebuildCoroutine = null;
        }

        private void Clear()
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
