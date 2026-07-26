using System.Collections;
using UnityEngine;
using VertigoDemo.WheelOfFortune.Animation;
using VertigoDemo.WheelOfFortune.Audio;
using VertigoDemo.WheelOfFortune.Data;
using VertigoDemo.WheelOfFortune.Events;
using VertigoDemo.WheelOfFortune.UI;
using VertigoDemo.WheelOfFortune.Utilities;

namespace VertigoDemo.WheelOfFortune.Core
{
    public sealed class WheelGameController : MonoBehaviour
    {
        [Header("Views")]
        [SerializeField] private StartScreenView startScreenView;
        [SerializeField] private GameInputView inputView;
        [SerializeField] private WheelView wheelView;
        [SerializeField] private ProgressBarView progressBarView;
        [SerializeField] private InventoryPanelView inventoryPanelView;
        [SerializeField] private ZoneInfoPanelView zoneInfoPanelView;
        [SerializeField] private SingleRewardRevealView singleRewardRevealView;
        [SerializeField] private WinPopupView winPopupView;

        [Header("Popups")]
        [SerializeField] private GameObject bombPopup;

        [Header("Services")]
        [SerializeField] private WheelConfigProvider wheelConfigProvider;
        [SerializeField] private CoroutineWheelSpinner wheelSpinner;
        [SerializeField] private AudioService audioService;
        [SerializeField] private VertigoDemo.WheelOfFortune.Events.GameEventLogger eventLogger;

        [Header("Spin Settings")]
        [SerializeField] private float spinDuration = GameConstants.DefaultSpinDuration;
        [SerializeField] private int fullRotations = GameConstants.DefaultFullRotations;

        private readonly IZoneService zoneService = new ZoneService();
        private readonly IRewardInventory rewardInventory = new RewardInventory();
        private readonly GameEventBus eventBus = new GameEventBus();

        private GameState currentState = GameState.Idle;
        private int currentZone = 1;

        private void Awake()
        {
            CacheReferences();
            if (eventLogger != null)
                eventLogger.Initialize(eventBus);
        }

        private void OnValidate()
        {
            CacheReferences();
        }

        private void OnEnable()
        {
            RegisterInputEvents();
        }

        private void OnDisable()
        {
            UnregisterInputEvents();
        }

        private void Start()
        {
            PrepareGame();
            ShowStartScreen();
        }

        private void CacheReferences()
        {
            if (startScreenView == null)
                startScreenView = FindObjectOfType<StartScreenView>(true);

            if (inputView == null)
                inputView = FindObjectOfType<GameInputView>(true);

            if (wheelView == null)
                wheelView = FindObjectOfType<WheelView>(true);

            if (progressBarView == null)
                progressBarView = FindObjectOfType<ProgressBarView>(true);

            if (inventoryPanelView == null)
                inventoryPanelView = FindObjectOfType<InventoryPanelView>(true);

            if (zoneInfoPanelView == null)
                zoneInfoPanelView = FindObjectOfType<ZoneInfoPanelView>(true);

            if (singleRewardRevealView == null)
                singleRewardRevealView = FindObjectOfType<SingleRewardRevealView>(true);

            if (winPopupView == null)
                winPopupView = FindObjectOfType<WinPopupView>(true);

            if (wheelConfigProvider == null)
                wheelConfigProvider = GetComponent<WheelConfigProvider>();

            if (wheelSpinner == null)
                wheelSpinner = GetComponent<CoroutineWheelSpinner>();

            if (audioService == null)
                audioService = GetComponent<AudioService>();

            if (eventLogger == null)
                eventLogger = GetComponent<VertigoDemo.WheelOfFortune.Events.GameEventLogger>();

            if (bombPopup == null)
                bombPopup = GameObject.Find("ui_popup_bomb");
        }

        private void RegisterInputEvents()
        {
            CacheReferences();

            if (startScreenView != null)
                startScreenView.StartClicked += StartGameFromMenu;

            if (inputView == null)
                return;

            inputView.SpinClicked += Spin;
            inputView.ExitClicked += Leave;
            inputView.RestartClicked += RestartGame;
            inputView.BombRestartClicked += RestartGame;
            inputView.BombReviveClicked += ReviveAfterBomb;
            inputView.CollectClicked += CollectRewards;
        }

        private void UnregisterInputEvents()
        {
            if (startScreenView != null)
                startScreenView.StartClicked -= StartGameFromMenu;

            if (inputView == null)
                return;

            inputView.SpinClicked -= Spin;
            inputView.ExitClicked -= Leave;
            inputView.RestartClicked -= RestartGame;
            inputView.BombRestartClicked -= RestartGame;
            inputView.BombReviveClicked -= ReviveAfterBomb;
            inputView.CollectClicked -= CollectRewards;
        }

        private void PrepareGame()
        {
            currentState = GameState.Idle;
            currentZone = 1;

            rewardInventory.Clear();

            audioService?.StopSpin();

            ShowBombPopup(false);
            winPopupView?.Hide();

            if (wheelView != null && wheelView.WheelRotator != null)
                wheelView.WheelRotator.localRotation = Quaternion.identity;

            inventoryPanelView?.Refresh(rewardInventory.Entries);
            LoadCurrentZone(false);

            inputView?.SetGameplayButtons(false, false);
        }

        private void ShowStartScreen()
        {
            startScreenView?.Show();
            inputView?.SetGameplayButtons(false, false);
        }

        private void StartGameFromMenu()
        {
            startScreenView?.Hide();
            RestartGame();
        }

        private void Spin()
        {
            if (currentState != GameState.Idle)
            {
                Debug.Log($"Spin ignored. Current state: {currentState}");
                return;
            }

            if (wheelConfigProvider == null || wheelView == null || wheelSpinner == null)
            {
                Debug.LogError("Spin failed. Missing required references.");
                return;
            }

            ZoneType zoneType = zoneService.GetZoneType(currentZone);
            WheelConfig config = wheelConfigProvider.GetConfig(zoneType);

            if (config == null || config.slices == null || config.slices.Length == 0)
            {
                Debug.LogError("Spin failed. Wheel config missing slices.");
                return;
            }

            int resultIndex = Random.Range(0, config.slices.Length);
            StartCoroutine(SpinRoutine(config, resultIndex));
        }

        private IEnumerator SpinRoutine(WheelConfig config, int resultIndex)
        {
            currentState = GameState.Spinning;

            inputView?.SetGameplayButtons(false, false);
            ShowBombPopup(false);
            winPopupView?.Hide();

            audioService?.PlaySpin();
            eventBus.Raise(new SpinStartedEvent());

            yield return wheelSpinner.Spin(
                wheelView.WheelRotator,
                config.slices.Length,
                resultIndex,
                spinDuration,
                fullRotations
            );

            audioService?.StopSpin();
            eventBus.Raise(new SpinCompletedEvent(resultIndex));

            WheelSlice result = config.slices[resultIndex];
            ResolveSlice(result);
        }

        private void ResolveSlice(WheelSlice result)
        {
            if (result == null)
            {
                Debug.LogError("ResolveSlice failed. Result null.");
                currentState = GameState.Idle;
                LoadCurrentZone(false);
                return;
            }

            if (result.isBomb)
            {
                HandleBombResult();
                return;
            }

            StartCoroutine(RewardResolveRoutine(result));
        }

        private IEnumerator RewardResolveRoutine(WheelSlice result)
        {
            currentState = GameState.Spinning;

            audioService?.PlayReward();

            if (singleRewardRevealView != null)
            {
                singleRewardRevealView.Show(result);
                yield return new WaitForSeconds(1.15f);
            }

            rewardInventory.Add(result);
            inventoryPanelView?.Refresh(rewardInventory.Entries);

            eventBus.Raise(new RewardCollectedEvent(result));

            currentZone++;

            currentState = GameState.Idle;
            LoadCurrentZone(true);

            Debug.Log($"Reward collected: {result.rewardName}");
        }

        private void HandleBombResult()
        {
            currentState = GameState.BombDecision;

            audioService?.PlayBomb();
            eventBus.Raise(new BombHitEvent());

            inputView?.SetGameplayButtons(false, false);
            ShowBombPopup(true);

            Debug.Log("Bomb hit. Waiting for revive or restart.");
        }

        private void Leave()
        {
            Debug.Log("Exit requested.");

            if (currentState != GameState.Idle)
            {
                Debug.Log($"Leave ignored. Current state: {currentState}");
                return;
            }

            if (!zoneService.CanLeave(currentZone))
            {
                Debug.Log("Leave ignored. Current zone is not safe or super.");
                return;
            }

            currentState = GameState.WinPopup;

            inputView?.SetGameplayButtons(false, false);

            if (winPopupView != null)
            {
                winPopupView.Show(
                    rewardInventory.TotalReward,
                    rewardInventory.ItemCount,
                    rewardInventory.Entries
                );
            }

            Debug.Log("Win popup opened with collected rewards.");
        }

        private void ReviveAfterBomb()
        {
            if (currentState != GameState.BombDecision)
            {
                Debug.Log("Revive ignored. There is no active bomb decision.");
                return;
            }

            currentState = GameState.Idle;

            ShowBombPopup(false);

            currentZone++;

            LoadCurrentZone(true);

            audioService?.PlayReward();
            eventBus.Raise(new RevivedEvent());

            Debug.Log("Player revived. Rewards kept.");
        }

        private void CollectRewards()
        {
            if (currentState != GameState.WinPopup)
            {
                Debug.Log("Collect ignored. Player is not in win popup state.");
                return;
            }

            StartCoroutine(CollectRewardsRoutine());
        }

        private IEnumerator CollectRewardsRoutine()
        {
            currentState = GameState.Collecting;

            audioService?.PlayCollect();

            if (winPopupView != null)
                yield return winPopupView.PlayCollectEffect();

            RestartGame();
        }

        private void RestartGame()
        {
            currentState = GameState.Idle;
            currentZone = 1;

            rewardInventory.Clear();

            audioService?.StopSpin();

            ShowBombPopup(false);
            winPopupView?.Hide();

            if (wheelView != null && wheelView.WheelRotator != null)
                wheelView.WheelRotator.localRotation = Quaternion.identity;

            inventoryPanelView?.Refresh(rewardInventory.Entries);

            LoadCurrentZone(false);

            eventBus.Raise(new GameRestartedEvent());

            Debug.Log("Game restarted.");
        }

        private void LoadCurrentZone(bool animate)
        {
            ZoneType zoneType = zoneService.GetZoneType(currentZone);

            WheelConfig config = null;

            if (wheelConfigProvider != null)
                config = wheelConfigProvider.GetConfig(zoneType);

            wheelView?.SetWheel(config);
            progressBarView?.Refresh(currentZone, zoneType, animate);
            inventoryPanelView?.Refresh(rewardInventory.Entries);
            zoneInfoPanelView?.Refresh(currentZone, zoneType);

            inputView?.SetGameplayButtons(true, zoneService.CanLeave(currentZone));

            eventBus.Raise(new ZoneChangedEvent(currentZone, zoneType));

            Debug.Log($"Loaded Zone {currentZone}. Zone Type: {zoneType}");
        }

        private void ShowBombPopup(bool show)
        {
            if (bombPopup == null)
                return;

            bombPopup.SetActive(show);

            if (show)
                bombPopup.transform.SetAsLastSibling();
        }
    }
}