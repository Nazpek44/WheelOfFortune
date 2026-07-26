using System;
using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.UI
{
    public sealed class StartScreenView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Button startButton;

        public event Action StartClicked;

        private void Awake()
        {
            CacheReferences();
        }

        private void OnValidate()
        {
            CacheReferences();
        }

        private void OnEnable()
        {
            if (startButton != null)
                startButton.onClick.AddListener(OnStartClicked);
        }

        private void OnDisable()
        {
            if (startButton != null)
                startButton.onClick.RemoveListener(OnStartClicked);
        }

        private void CacheReferences()
        {
            if (root == null)
                root = gameObject;

            if (startButton == null)
                startButton = ComponentFinder.FindChildByName<Button>(this, "ui_button_start");
        }

        public void Show()
        {
            root.SetActive(true);
            root.transform.SetAsLastSibling();
        }

        public void Hide()
        {
            root.SetActive(false);
        }

        private void OnStartClicked()
        {
            StartClicked?.Invoke();
        }
    }
}