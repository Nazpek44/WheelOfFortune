using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelGameController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameUI ui;

    [Header("Wheel Configs")]
    [SerializeField] private WheelConfig normalWheel;
    [SerializeField] private WheelConfig safeWheel;
    [SerializeField] private WheelConfig superWheel;

    [Header("Spin Settings")]
    [SerializeField] private float spinDuration = 3f;
    [SerializeField] private int fullRotations = 6;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip spinAudioClip;
    [SerializeField] private AudioClip rewardAudioClip;
    [SerializeField] private AudioClip collectAudioClip;
    [SerializeField] private AudioClip bombAudioClip;

    private int currentZone = 1;
    private int totalReward = 0;
    private bool isSpinning = false;
    private bool hasLeftGame = false;
    private bool isWaitingForBombDecision = false;

    private readonly List<WheelSlice> collectedRewards = new List<WheelSlice>();

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
        if (ui == null)
            ui = FindObjectOfType<GameUI>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        StartCoroutine(RegisterButtonsNextFrame());
    }

    private IEnumerator RegisterButtonsNextFrame()
    {
        yield return null;

        CacheReferences();

        if (ui == null)
        {
            Debug.LogError("WheelGameController cannot register buttons because GameUI is missing.");
            yield break;
        }

        if (ui.SpinButton != null)
        {
            ui.SpinButton.onClick.RemoveListener(Spin);
            ui.SpinButton.onClick.AddListener(Spin);
        }
        else
        {
            Debug.LogWarning("Spin button is missing.");
        }

        if (ui.LeaveButton != null)
        {
            ui.LeaveButton.onClick.RemoveListener(Leave);
            ui.LeaveButton.onClick.AddListener(Leave);
        }
        else
        {
            Debug.LogWarning("Leave button is missing.");
        }

        if (ui.RestartButton != null)
        {
            ui.RestartButton.onClick.RemoveListener(RestartGame);
            ui.RestartButton.onClick.AddListener(RestartGame);
        }
        else
        {
            Debug.LogWarning("Restart button is missing.");
        }

        if (ui.BombRestartButton != null)
        {
            ui.BombRestartButton.onClick.RemoveListener(RestartGame);
            ui.BombRestartButton.onClick.AddListener(RestartGame);
        }
        else
        {
            Debug.LogWarning("Bomb restart button is missing. Check ui_button_bomb_restart.");
        }

        if (ui.BombReviveButton != null)
        {
            ui.BombReviveButton.onClick.RemoveListener(ReviveAfterBomb);
            ui.BombReviveButton.onClick.AddListener(ReviveAfterBomb);
        }
        else
        {
            Debug.LogWarning("Bomb revive button is missing. Check ui_button_bomb_revive.");
        }

        if (ui.CollectButton != null)
        {
            ui.CollectButton.onClick.RemoveListener(CollectRewards);
            ui.CollectButton.onClick.AddListener(CollectRewards);
        }
        else
        {
            Debug.LogWarning("Collect button is missing. Check ui_button_collect or ui_button_win_collect.");
        }
    }

    private void OnDisable()
    {
        if (ui == null)
            return;

        if (ui.SpinButton != null)
            ui.SpinButton.onClick.RemoveListener(Spin);

        if (ui.LeaveButton != null)
            ui.LeaveButton.onClick.RemoveListener(Leave);

        if (ui.RestartButton != null)
            ui.RestartButton.onClick.RemoveListener(RestartGame);

        if (ui.BombRestartButton != null)
            ui.BombRestartButton.onClick.RemoveListener(RestartGame);

        if (ui.BombReviveButton != null)
            ui.BombReviveButton.onClick.RemoveListener(ReviveAfterBomb);

        if (ui.CollectButton != null)
            ui.CollectButton.onClick.RemoveListener(CollectRewards);
    }

    private void Start()
    {
        RestartGame();
    }

    private void Spin()
    {
        Debug.Log("Spin button pressed.");

        if (isSpinning || isWaitingForBombDecision)
        {
            Debug.Log("Spin ignored because the game is busy.");
            return;
        }

        if (hasLeftGame)
        {
            Debug.Log("Spin ignored because player already left the game.");
            return;
        }

        WheelConfig config = GetCurrentWheelConfig();

        if (config == null)
        {
            Debug.LogError("Current wheel config is null.");
            return;
        }

        if (config.slices == null || config.slices.Length == 0)
        {
            Debug.LogError("Current wheel config has no slices.");
            return;
        }

        int resultIndex = Random.Range(0, config.slices.Length);

        StartCoroutine(SpinRoutine(config, resultIndex));
    }

    private IEnumerator SpinRoutine(WheelConfig config, int resultIndex)
    {
        isSpinning = true;

        ui.SetButtons(false, false);
        ui.ShowBombPopup(false);
        ui.ShowWinPopup(false, totalReward, collectedRewards);

        PlaySpinAudio();

        RectTransform rotator = ui.WheelRotator;

        if (rotator == null)
        {
            Debug.LogError("Wheel rotator is missing.");
            StopSpinAudio();
            isSpinning = false;
            yield break;
        }

        float sliceAngle = 360f / config.slices.Length;

        float startZ = NormalizeAngle(rotator.localEulerAngles.z);

        /*
         * This assumes your slots are ordered like this:
         *
         * Slice 0 = top
         * Slice 1 = top right
         * Slice 2 = right
         * Slice 3 = bottom right
         * Slice 4 = bottom
         * Slice 5 = bottom left
         * Slice 6 = left
         * Slice 7 = top left
         */
        float targetSliceZ = resultIndex * sliceAngle;

        float deltaToTarget = Mathf.Repeat(targetSliceZ - startZ, 360f);
        float endZ = startZ + 360f * fullRotations + deltaToTarget;

        float timer = 0f;

        while (timer < spinDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / spinDuration);
            float easedT = EaseOutCubic(t);

            float currentZ = Mathf.Lerp(startZ, endZ, easedT);
            rotator.localRotation = Quaternion.Euler(0f, 0f, currentZ);

            yield return null;
        }

        rotator.localRotation = Quaternion.Euler(0f, 0f, targetSliceZ);

        StopSpinAudio();

        isSpinning = false;

        WheelSlice result = config.slices[resultIndex];

        Debug.Log("Spin stopped. Result: " + result.rewardName);

        if (!result.isBomb)
        {
            PlayOneShot(rewardAudioClip);

            ui.ShowSingleRewardReveal(result);

            /*
             * Wait while the reward reveal animation plays.
             * If your reveal animation is longer/shorter, adjust this number.
             */
            yield return new WaitForSeconds(1.15f);
        }

        ResolveSlice(result);
    }

    private void ResolveSlice(WheelSlice result)
    {
        if (result == null)
        {
            Debug.LogError("Cannot resolve slice because result is null.");
            return;
        }

        if (result.isBomb)
        {
            PlayOneShot(bombAudioClip);

            isWaitingForBombDecision = true;

            ui.SetButtons(false, false);
            ui.ShowBombPopup(true);

            Debug.Log("Bomb hit. Waiting for revive or restart.");

            return;
        }

        collectedRewards.Add(result);
        totalReward += Mathf.Max(0, result.amount);

        Debug.Log($"Reward gained: {result.rewardName} / Amount: {result.amount}");

        currentZone++;

        LoadCurrentZone();
    }

    private void Leave()
    {
        Debug.Log("Leave button pressed.");

        if (!ZoneRules.CanLeave(currentZone, isSpinning))
        {
            Debug.Log("Leave ignored. Current zone is not safe/super or wheel is spinning.");
            return;
        }

        hasLeftGame = true;

        ui.SetButtons(false, false);
        ui.ShowWinPopup(true, totalReward, collectedRewards);

        Debug.Log($"Player left with total reward: {totalReward}");
    }

    private void CollectRewards()
    {
        Debug.Log("Collect button pressed.");

        if (!hasLeftGame)
        {
            Debug.LogWarning("Collect ignored because player has not left the game yet.");
            return;
        }

        PlayOneShot(collectAudioClip);

        StartCoroutine(ui.PlayCollectEffect(RestartGame));
    }

    private void RestartGame()
    {
        Debug.Log("Game restarted.");

        hasLeftGame = false;
        isWaitingForBombDecision = false;
        currentZone = 1;
        totalReward = 0;
        isSpinning = false;
        collectedRewards.Clear();

        StopSpinAudio();

        ui.ShowBombPopup(false);
        ui.ShowWinPopup(false, totalReward, collectedRewards);

        if (ui.WheelRotator != null)
            ui.WheelRotator.localRotation = Quaternion.identity;

        LoadCurrentZone();
    }

    private void ReviveAfterBomb()
    {
        Debug.Log("Revive button pressed.");

        if (!isWaitingForBombDecision)
        {
            Debug.LogWarning("Revive ignored because there is no active bomb decision.");
            return;
        }

        isWaitingForBombDecision = false;

        ui.ShowBombPopup(false);

        currentZone++;

        LoadCurrentZone();

        Debug.Log("Player revived. Rewards kept. Continuing to next zone.");
    }

    private void LoadCurrentZone()
    {
        WheelConfig config = GetCurrentWheelConfig();

        if (config == null)
        {
            Debug.LogError("Cannot load zone because wheel config is null.");
            return;
        }

        ZoneType zoneType = ZoneRules.GetZoneType(currentZone);

        ui.SetWheel(config);
        ui.SetZone(currentZone, zoneType);
        ui.SetTotalReward(totalReward, collectedRewards.Count);
        ui.SetButtons(true, ZoneRules.CanLeave(currentZone, isSpinning));

        Debug.Log($"Loaded zone {currentZone}. Zone type: {zoneType}");
    }

    private WheelConfig GetCurrentWheelConfig()
    {
        ZoneType zoneType = ZoneRules.GetZoneType(currentZone);

        return zoneType switch
        {
            ZoneType.Super => superWheel,
            ZoneType.Safe => safeWheel,
            _ => normalWheel
        };
    }

    private void PlaySpinAudio()
    {
        if (audioSource == null || spinAudioClip == null)
            return;

        audioSource.Stop();
        audioSource.clip = spinAudioClip;
        audioSource.loop = true;
        audioSource.Play();
    }

    private void StopSpinAudio()
    {
        if (audioSource == null)
            return;

        audioSource.Stop();
        audioSource.loop = false;
        audioSource.clip = null;
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.PlayOneShot(clip);
    }

    private float NormalizeAngle(float angle)
    {
        return Mathf.Repeat(angle, 360f);
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}