using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.WheelOfFortune.Core;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.UI
{
    public sealed class InventoryPanelView : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform viewport;
        [SerializeField] private RectTransform content;
        [SerializeField] private InventoryItemView itemTemplate;

        private readonly List<InventoryItemView> spawnedItems = new List<InventoryItemView>();
        private Coroutine rebuildRoutine;

        private void Awake()
        {
            CacheReferences();
        }

        private void OnValidate()
        {
            CacheReferences();
        }

        private void CacheReferences()
        {
            if (scrollRect == null)
                scrollRect = ComponentFinder.FindChildByName<ScrollRect>(this, "ui_scroll_inventory_items");

            if (viewport == null)
                viewport = ComponentFinder.FindChildByName<RectTransform>(this, "ui_viewport_inventory_items");

            if (content == null)
                content = ComponentFinder.FindChildByName<RectTransform>(this, "ui_content_inventory_items");

            if (itemTemplate == null)
                itemTemplate = ComponentFinder.FindChildByName<InventoryItemView>(this, "ui_item_inventory_reward_template");
        }

        public void Refresh(IReadOnlyList<InventoryEntry> entries)
        {
            Clear();

            if (content == null || itemTemplate == null)
            {
                Debug.LogWarning("InventoryPanelView missing content or template.");
                return;
            }

            itemTemplate.gameObject.SetActive(false);

            if (entries != null)
            {
                foreach (InventoryEntry entry in entries)
                {
                    InventoryItemView item = Instantiate(itemTemplate, content);
                    item.name = "ui_item_inventory_reward";
                    item.SetData(entry);
                    spawnedItems.Add(item);
                }
            }

            if (rebuildRoutine != null)
                StopCoroutine(rebuildRoutine);

            rebuildRoutine = StartCoroutine(RebuildScrollNextFrame());
        }

        private IEnumerator RebuildScrollNextFrame()
        {
            yield return null;

            Canvas.ForceUpdateCanvases();

            if (content != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);

            Canvas.ForceUpdateCanvases();

            if (scrollRect != null)
            {
                scrollRect.horizontal = false;
                scrollRect.vertical = true;
                scrollRect.verticalScrollbar = null;
                scrollRect.horizontalScrollbar = null;

                scrollRect.content = content;
                scrollRect.viewport = viewport;

                scrollRect.normalizedPosition = new Vector2(0f, 1f);
            }
        }

        private void Clear()
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