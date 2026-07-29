using System;
using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.UI
{
    public sealed class StartScreenViewMono : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Button _startButton;

        public event Action StartClicked;

        private void Reset()
        {
            _root = gameObject;
            _startButton = ComponentFinder.FindChildByName<Button>(this, "ui_button_start");
        }

        private void Awake()
        {
            if (!_root)
                _root = gameObject;
        }

        private void OnEnable()
        {
            if (_startButton)
                _startButton.onClick.AddListener(OnStartClicked);
        }

        private void OnDisable()
        {
            if (_startButton)
                _startButton.onClick.RemoveListener(OnStartClicked);
        }

        public void Show()
        {
            if (!_root)
                return;

            _root.SetActive(true);
            _root.transform.SetAsLastSibling();
        }

        public void Hide()
        {
            if (_root)
                _root.SetActive(false);
        }

        private void OnStartClicked()
        {
            StartClicked?.Invoke();
        }
    }
}
