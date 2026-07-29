using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VertigoDemo.WheelOfFortune.Animation;
using VertigoDemo.WheelOfFortune.Audio;
using VertigoDemo.WheelOfFortune.Data;
using VertigoDemo.WheelOfFortune.Economy;
using VertigoDemo.WheelOfFortune.Events;
using VertigoDemo.WheelOfFortune.UI;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.Core
{
    /// <summary>
    /// Drives the run: spin, resolve, progress, lose or bank.
    ///
    /// Bomb rule (per brief): a bomb destroys every reward collected during the
    /// run and sends the player back to zone 1. Continuing is a separate,
    /// optional bonus paid for out of the player's persistent wallet.
    /// </summary>
    public sealed class WheelGameControllerMono : MonoBehaviour
    {
        [Header("Views")]
        [SerializeField] private StartScreenViewMono _startScreenView;
        [SerializeField] private GameInputViewMono _inputView;
        [SerializeField] private WheelViewMono _wheelView;
        [SerializeField] private ProgressBarViewMono _progressBarView;
        [SerializeField] private InventoryPanelViewMono _inventoryPanelView;
        [SerializeField] private ZoneInfoPanelViewMono _zoneInfoPanelView;
        [SerializeField] private SingleRewardRevealViewMono _singleRewardRevealView;
        [SerializeField] private WinPopupViewMono _winPopupView;
        [SerializeField] private BombPopupViewMono _bombPopupView;
        [SerializeField] private WalletHudViewMono _walletHudView;

        [Header("Services")]
        [SerializeField] private WheelConfigProviderMono _wheelConfigProvider;
        [SerializeField] private CoroutineWheelSpinnerMono _wheelSpinner;
        [SerializeField] private AudioServiceMono _audioService;
        [SerializeField] private GameEventLoggerMono _eventLogger;

        [Header("Settings")]
        [Tooltip("Economy tuning asset. Falls back to code defaults when empty.")]
        [SerializeField] private GameEconomySettings _economySettings;

        [Header("Spin Settings")]
        [Range(0.2f, 10f)]
        [SerializeField] private float _spinDuration = GameConstants.DEFAULT_SPIN_DURATION;

        [Range(1, 20)]
        [SerializeField] private int _fullRotations = GameConstants.DEFAULT_FULL_ROTATIONS;

        [Range(0.1f, 3f)]
        [SerializeField] private float _rewardRevealSeconds = 1.15f;

        // Dependencies are consumed through interfaces. The serialized fields
        // above exist only because Unity cannot serialize an interface; no logic
        // below this line refers to a concrete implementation.
        private IWheelSpinner _spinner;
        private IAudioService _audio;
        private IWheelConfigProvider _configProvider;
        private IZoneService _zoneService;
        private IRunRewardInventory _runInventory;
        private IPlayerWallet _wallet;
        private IWheelRewardResolver _rewardResolver;
        private IContinueCostPolicy _continueCostPolicy;
        private GameEventBus _eventBus;

        private readonly List<RewardDraw> _resolvedSlices = new List<RewardDraw>();

        private GameState _currentState = GameState.Menu;
        private int _currentZone = GameConstants.FIRST_ZONE;
        private int _continuesUsedThisRun;

        private Coroutine _spinCoroutine;
        private Coroutine _rewardCoroutine;
        private Coroutine _collectCoroutine;

        /// <summary>
        /// Optional injection point. Call before Start to supply test doubles;
        /// otherwise the default service set is built in Awake.
        /// </summary>
        public void Initialize(GameServices services)
        {
            if (services == null)
                return;

            _zoneService = services.ZoneService;
            _runInventory = services.RunInventory;
            _wallet = services.Wallet;
            _rewardResolver = services.RewardResolver;
            _continueCostPolicy = services.ContinueCostPolicy;
            _eventBus = services.EventBus;
        }

        private void Awake()
        {
            if (_zoneService == null)
                Initialize(GameServices.CreateDefault(_economySettings));

            ResolveComponentDependencies();

            if (_eventLogger)
                _eventLogger.Initialize(_eventBus);

            if (_wallet != null)
                _wallet.Changed += OnWalletChanged;
        }

        private void OnEnable()
        {
            RegisterInputEvents();
        }

        private void OnDisable()
        {
            UnregisterInputEvents();
        }

        private void OnDestroy()
        {
            StopAllCoroutines();

            _spinCoroutine = null;
            _rewardCoroutine = null;
            _collectCoroutine = null;

            if (_wallet != null)
                _wallet.Changed -= OnWalletChanged;
        }

        private void Start()
        {
            PrepareGame();
            ShowStartScreen();
        }

        /// <summary>
        /// Binds serialized components to their interfaces once, at startup.
        /// There is no scene scanning here and none in OnValidate: every
        /// reference is assigned in the inspector, and a missing one is a loud
        /// error rather than a silent FindObjectOfType.
        /// </summary>
        private void ResolveComponentDependencies()
        {
            if (_wheelSpinner)
                _spinner = _wheelSpinner;
            else
                Debug.LogError("Wheel spinner reference is missing.", this);

            if (_wheelConfigProvider)
                _configProvider = _wheelConfigProvider;
            else
                Debug.LogError("Wheel config provider reference is missing.", this);

            if (_audioService)
                _audio = _audioService;

            if (!_wheelView)
                Debug.LogError("Wheel view reference is missing.", this);

            if (!_inputView)
                Debug.LogError("Input view reference is missing.", this);
        }

        private void RegisterInputEvents()
        {
            if (_startScreenView)
                _startScreenView.StartClicked += StartGameFromMenu;

            if (_inputView)
            {
                _inputView.SpinClicked += Spin;
                _inputView.ExitClicked += Leave;
                _inputView.RestartClicked += RestartRun;
                _inputView.CollectClicked += CollectRewards;
            }

            if (_bombPopupView)
            {
                _bombPopupView.ContinueClicked += ContinueAfterBomb;
                _bombPopupView.GiveUpClicked += GiveUpAfterBomb;
            }
        }

        private void UnregisterInputEvents()
        {
            if (_startScreenView)
                _startScreenView.StartClicked -= StartGameFromMenu;

            if (_inputView)
            {
                _inputView.SpinClicked -= Spin;
                _inputView.ExitClicked -= Leave;
                _inputView.RestartClicked -= RestartRun;
                _inputView.CollectClicked -= CollectRewards;
            }

            if (_bombPopupView)
            {
                _bombPopupView.ContinueClicked -= ContinueAfterBomb;
                _bombPopupView.GiveUpClicked -= GiveUpAfterBomb;
            }
        }

        private void PrepareGame()
        {
            _currentState = GameState.Menu;

            ResetRunState();

            if (_inputView)
                _inputView.SetGameplayButtons(false, false);
        }

        private void ShowStartScreen()
        {
            if (_startScreenView)
                _startScreenView.Show();

            if (_inputView)
                _inputView.SetGameplayButtons(false, false);
        }

        private void StartGameFromMenu()
        {
            if (_startScreenView)
                _startScreenView.Hide();

            _eventBus?.Raise(new GameStartedEvent());

            RestartRun();
        }

        // ------------------------------------------------------------------
        // Spinning
        // ------------------------------------------------------------------

        private void Spin()
        {
            if (_currentState != GameState.Idle)
            {
                Debug.Log($"Spin ignored. Current state: {_currentState}");
                return;
            }

            if (_configProvider == null || _spinner == null || !_wheelView)
            {
                Debug.LogError("Spin failed. Missing required references.", this);
                return;
            }

            ZoneType zoneType = _zoneService.GetZoneType(_currentZone);
            WheelConfig config = _configProvider.GetConfig(zoneType);

            if (config == null || !config.HasSlices)
            {
                Debug.LogError("Spin failed. Wheel config has no slices.", this);
                return;
            }

            int resultIndex = Random.Range(0, config.SliceCount);

            _spinCoroutine = StartCoroutine(SpinCoroutine(config, zoneType, resultIndex));
        }

        private IEnumerator SpinCoroutine(WheelConfig config, ZoneType zoneType, int resultIndex)
        {
            _currentState = GameState.Spinning;

            if (_inputView)
                _inputView.SetGameplayButtons(false, false);

            if (_bombPopupView)
                _bombPopupView.Hide();

            if (_winPopupView)
                _winPopupView.Hide();

            _audio?.PlaySpin();
            _eventBus?.Raise(new SpinStartedEvent());

            yield return _spinner.SpinCoroutine(
                _wheelView.WheelRotator,
                config.SliceCount,
                resultIndex,
                _spinDuration,
                _fullRotations
            );

            _audio?.StopSpin();
            _eventBus?.Raise(new SpinCompletedEvent(resultIndex));

            _spinCoroutine = null;

            if (resultIndex < 0 || resultIndex >= _resolvedSlices.Count)
            {
                Debug.LogError("Spin result index is outside the resolved wheel.", this);
                _currentState = GameState.Idle;
                LoadCurrentZone(false);
                yield break;
            }

            // Award exactly what the wheel showed, rather than resolving a
            // second time and risking a mismatch.
            RewardDraw draw = _resolvedSlices[resultIndex];

            if (draw.IsBomb)
            {
                HandleBombResult();
                yield break;
            }

            _rewardCoroutine = StartCoroutine(RewardResolveCoroutine(draw));
        }

        private IEnumerator RewardResolveCoroutine(RewardDraw draw)
        {
            _currentState = GameState.Spinning;

            _audio?.PlayReward();

            if (_singleRewardRevealView)
            {
                _singleRewardRevealView.Show(draw);
                yield return new WaitForSeconds(_rewardRevealSeconds);
            }

            _runInventory.Add(draw);

            _eventBus?.Raise(new RewardCollectedEvent(draw));

            _currentZone++;
            _currentState = GameState.Idle;

            LoadCurrentZone(true);

            _rewardCoroutine = null;

            Debug.Log($"Reward collected: {draw.DisplayName} x{draw.Amount}.");
        }

        // ------------------------------------------------------------------
        // Bomb: lose the run, or pay to continue
        // ------------------------------------------------------------------

        private void HandleBombResult()
        {
            _currentState = GameState.BombDecision;

            _audio?.PlayBomb();
            _eventBus?.Raise(new BombHitEvent(_currentZone));

            if (_inputView)
                _inputView.SetGameplayButtons(false, false);

            ShowBombPopup();

            Debug.Log($"Bomb hit at zone {_currentZone}. Rewards are lost unless the player pays to continue.");
        }

        private void ShowBombPopup()
        {
            if (!_bombPopupView)
            {
                // Without the popup there is no way to offer the paid continue,
                // so the brief's default applies: the run is lost.
                GiveUpAfterBomb();
                return;
            }

            CurrencyType currency = _continueCostPolicy.Currency;
            long cost = _continueCostPolicy.GetCost(_currentZone, _continuesUsedThisRun);
            long balance = _wallet.GetBalance(currency);

            _bombPopupView.Show(currency, cost, balance, _runInventory.CollectedCount);
        }

        private void ContinueAfterBomb()
        {
            if (_currentState != GameState.BombDecision)
            {
                Debug.Log("Continue ignored. There is no active bomb decision.");
                return;
            }

            CurrencyType currency = _continueCostPolicy.Currency;
            long cost = _continueCostPolicy.GetCost(_currentZone, _continuesUsedThisRun);

            if (!_wallet.TrySpend(currency, cost))
            {
                long balance = _wallet.GetBalance(currency);

                _eventBus?.Raise(new ContinueRejectedEvent(currency, cost, balance));

                if (_bombPopupView)
                    _bombPopupView.Show(currency, cost, balance, _runInventory.CollectedCount);

                Debug.Log($"Continue refused. Cost {cost} {currency}, balance {balance}.");
                return;
            }

            _continuesUsedThisRun++;

            if (_bombPopupView)
                _bombPopupView.Hide();

            _currentState = GameState.Idle;

            // The player bought their way out of the bomb: rewards survive and
            // the zone is replayed. Continuing never advances progress by itself.
            LoadCurrentZone(false);

            _audio?.PlayReward();
            _eventBus?.Raise(new ContinuePurchasedEvent(_currentZone, currency, cost));

            Debug.Log($"Continue purchased for {cost} {currency}. Rewards kept, zone {_currentZone} replayed.");
        }

        private void GiveUpAfterBomb()
        {
            if (_currentState != GameState.BombDecision)
            {
                Debug.Log("Give up ignored. There is no active bomb decision.");
                return;
            }

            int rewardsLost = _runInventory.CollectedCount;

            _eventBus?.Raise(new RunLostEvent(_currentZone, rewardsLost));

            Debug.Log($"Bomb resolved without payment. {rewardsLost} collected rewards destroyed.");

            RestartRun();
        }

        // ------------------------------------------------------------------
        // Leaving and banking
        // ------------------------------------------------------------------

        private void Leave()
        {
            if (_currentState != GameState.Idle)
            {
                Debug.Log($"Leave ignored. Current state: {_currentState}");
                return;
            }

            if (!_zoneService.CanLeave(_currentZone))
            {
                Debug.Log("Leave ignored. Current zone is not safe or super.");
                return;
            }

            _currentState = GameState.WinPopup;

            if (_inputView)
                _inputView.SetGameplayButtons(false, false);

            if (_winPopupView)
                _winPopupView.Show(_runInventory, _wallet);

            Debug.Log("Win popup opened with collected rewards.");
        }

        private void CollectRewards()
        {
            if (_currentState != GameState.WinPopup)
            {
                Debug.Log("Collect ignored. Player is not in win popup state.");
                return;
            }

            _collectCoroutine = StartCoroutine(CollectRewardsCoroutine());
        }

        private IEnumerator CollectRewardsCoroutine()
        {
            _currentState = GameState.Collecting;

            _audio?.PlayCollect();

            // This is the only path that moves rewards into the player's real
            // inventory. Everything collected before this point is at risk.
            int bankedEntries = _runInventory.EntryCount;

            if (_winPopupView)
                yield return _winPopupView.PlayCollectEffectCoroutine();

            // Credited once the rewards have visually landed, so the balance
            // ticks up at the moment the player sees them arrive.
            _wallet.Deposit(_runInventory.Entries);
            _eventBus?.Raise(new RewardsBankedEvent(bankedEntries));

            if (_walletHudView)
                yield return _walletHudView.PlayPunchCoroutine();

            _collectCoroutine = null;

            Debug.Log($"Banked {bankedEntries} reward entries into the player wallet.");

            RestartRun();
        }

        // ------------------------------------------------------------------
        // Run lifecycle
        // ------------------------------------------------------------------

        private void RestartRun()
        {
            _currentState = GameState.Idle;

            ResetRunState();

            _eventBus?.Raise(new GameRestartedEvent());

            Debug.Log("Run restarted from zone 1.");
        }

        private void ResetRunState()
        {
            _currentZone = GameConstants.FIRST_ZONE;
            _continuesUsedThisRun = 0;

            _runInventory.Clear();

            _audio?.StopSpin();

            if (_bombPopupView)
                _bombPopupView.Hide();

            if (_winPopupView)
                _winPopupView.Hide();

            if (_wheelView)
                _wheelView.ResetRotation();

            LoadCurrentZone(false);
            RefreshWalletHud();
        }

        private void LoadCurrentZone(bool animate)
        {
            ZoneType zoneType = _zoneService.GetZoneType(_currentZone);

            WheelConfig config = _configProvider == null ? null : _configProvider.GetConfig(zoneType);

            ResolveSlicesForZone(config, zoneType);

            if (_wheelView)
                _wheelView.SetWheel(config, _resolvedSlices);

            if (_progressBarView)
                _progressBarView.Refresh(_currentZone, zoneType, animate);

            if (_inventoryPanelView)
                _inventoryPanelView.Refresh(_runInventory.Entries);

            if (_zoneInfoPanelView)
                _zoneInfoPanelView.Refresh(_zoneService, _currentZone, zoneType);

            if (_inputView)
                _inputView.SetGameplayButtons(true, _zoneService.CanLeave(_currentZone));

            _eventBus?.Raise(new ZoneChangedEvent(_currentZone, zoneType));

            Debug.Log($"Loaded zone {_currentZone} ({zoneType}).");
        }

        /// <summary>
        /// Resolves the whole wheel for this zone: currencies scaled by the zone
        /// curve, item gifts swapped for their current tier.
        /// </summary>
        private void ResolveSlicesForZone(WheelConfig config, ZoneType zoneType)
        {
            _rewardResolver.ResolveWheel(config, _currentZone, zoneType, _resolvedSlices);
        }

        private void OnWalletChanged()
        {
            RefreshWalletHud();
        }

        private void RefreshWalletHud()
        {
            if (_walletHudView)
                _walletHudView.Refresh(_wallet);
        }
    }
}
