using System.Collections;
using TMPro;
using UnityEngine;
using VertigoDemo.WheelOfFortune.Economy;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.UI
{
    /// <summary>
    /// Persistent display of the player's real wallet balances, so the cost of
    /// a continue and the payout of a collect are both visible on screen.
    /// </summary>
    public sealed class WalletHudViewMono : MonoBehaviour
    {
        [SerializeField] private TMP_Text _cashText;
        [SerializeField] private TMP_Text _goldText;

        [Header("Punch")]
        [Range(1f, 1.6f)]
        [SerializeField] private float _punchScale = 1.18f;

        [Range(0.05f, 0.6f)]
        [SerializeField] private float _punchDuration = 0.22f;

        private Coroutine _punchCoroutine;

        private void Reset()
        {
            _cashText = ComponentFinder.FindChildByName<TMP_Text>(this, "ui_text_wallet_cash_value");
            _goldText = ComponentFinder.FindChildByName<TMP_Text>(this, "ui_text_wallet_gold_value");
        }

        private void OnDestroy()
        {
            if (_punchCoroutine != null)
                StopCoroutine(_punchCoroutine);

            _punchCoroutine = null;
        }

        /// <summary>Small scale punch played as collected rewards land.</summary>
        public IEnumerator PlayPunchCoroutine()
        {
            if (!isActiveAndEnabled)
                yield break;

            float half = _punchDuration * 0.5f;
            float timer = 0f;

            while (timer < half)
            {
                timer += Time.deltaTime;

                float progress = Mathf.Clamp01(timer / half);

                transform.localScale = Vector3.one * Mathf.Lerp(1f, _punchScale, EaseOutCubic(progress));

                yield return null;
            }

            timer = 0f;

            while (timer < half)
            {
                timer += Time.deltaTime;

                float progress = Mathf.Clamp01(timer / half);

                transform.localScale = Vector3.one * Mathf.Lerp(_punchScale, 1f, EaseOutCubic(progress));

                yield return null;
            }

            transform.localScale = Vector3.one;
        }

        private static float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }

        public void Refresh(IPlayerWallet wallet)
        {
            if (wallet == null)
                return;

            if (_cashText)
                _cashText.text = "CASH  " + RewardTextFormatter.FormatBalance(wallet.GetBalance(CurrencyType.Cash));

            if (_goldText)
                _goldText.text = "GOLD  " + RewardTextFormatter.FormatBalance(wallet.GetBalance(CurrencyType.Gold));
        }
    }
}
