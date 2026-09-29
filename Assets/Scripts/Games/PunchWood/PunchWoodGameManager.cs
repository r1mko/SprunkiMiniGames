using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class PunchWoodGameManager : MonoBehaviour, IMiniGame
{
    public MiniGameType Type => MiniGameType.PunchWood;

    [SerializeField, Required] private PunchFist fist;
    [SerializeField] private BreakableWood[] woods;
    [SerializeField, Required] private Transform perfectHeight;
    [SerializeField, Required] private Image powerBarImage;

    [SerializeField] private float chargeDuration = 1f;
    [SerializeField, MinMaxSlider(0f, 1f)] private Vector2 perfectZone = new Vector2(0.45f, 0.5f);
    [SerializeField] private float minPowerMultiplier = 0.3f;
    [SerializeField] private float maxPowerMultiplier = 2f;
    [SerializeField, Range(0f, 0.9f)] private float missGap = 0.3f;

    private bool _hasFinished;
    private bool _won;

    private void Awake()
    {
        fist.HitObstacle += OnFistHitObstacle;
        fist.Landed += OnFistLanded;
    }

    private void OnDestroy()
    {
        fist.HitObstacle -= OnFistHitObstacle;
        fist.Landed -= OnFistLanded;
    }

    private void OnDisable()
    {
        ResetGame();
    }

    public void StartGame()
    {
        StartCoroutine(ChargeAndLaunchRoutine());
    }

    [Button("Reset", EButtonEnableMode.Playmode)]
    public void ResetGame()
    {
        StopAllCoroutines();
        fist.ResetFist();

        foreach (BreakableWood wood in woods)
        {
            wood.ResetWood();
        }

        _hasFinished = false;
        _won = false;

        UpdatePowerBar(0f);
    }

    public void CheckResult()
    {
        if (_won)
        {
            UIManager.Instance.ShowNextScreen();
        }
        else
        {
            UIManager.Instance.ShowRetryScreen();
        }
    }

    private IEnumerator ChargeAndLaunchRoutine()
    {
        while (!PointerInput.WasPressedThisFrame())
        {
            yield return null;
        }

        float elapsed = 0f;

        while (elapsed < chargeDuration)
        {
            if (PointerInput.WasReleasedThisFrame())
            {
                break;
            }

            elapsed += Time.deltaTime;
            UpdatePowerBar(elapsed / chargeDuration);
            yield return null;
        }

        LaunchFist(Mathf.Clamp01(elapsed / chargeDuration));
        UpdatePowerBar(0f);
    }

    private void LaunchFist(float chargeFraction)
    {
        float height = Mathf.Max(0f, perfectHeight.position.y - fist.transform.position.y);
        float perfectSpeed = Mathf.Sqrt(2f * fist.Gravity * height);

        fist.Launch(perfectSpeed * GetPowerMultiplier(chargeFraction));
    }

    private float GetPowerMultiplier(float chargeFraction)
    {
        if (chargeFraction < perfectZone.x)
        {
            return Mathf.Lerp(minPowerMultiplier, 1f - missGap, Mathf.InverseLerp(0f, perfectZone.x, chargeFraction));
        }

        if (chargeFraction > perfectZone.y)
        {
            return Mathf.Lerp(1f + missGap, maxPowerMultiplier, Mathf.InverseLerp(perfectZone.y, 1f, chargeFraction));
        }

        return 1f;
    }

    private void OnFistHitObstacle()
    {
        if (_hasFinished)
        {
            return;
        }

        FinishGame(false);
    }

    private void OnFistLanded()
    {
        if (_hasFinished)
        {
            return;
        }

        FinishGame(AreAllWoodsBroken());
    }

    private bool AreAllWoodsBroken()
    {
        foreach (BreakableWood wood in woods)
        {
            if (!wood.IsBroken)
            {
                return false;
            }
        }

        return true;
    }

    private void FinishGame(bool won)
    {
        _hasFinished = true;
        _won = won;
        StopAllCoroutines();
        CheckResult();
    }

    private void UpdatePowerBar(float fraction)
    {
        powerBarImage.fillAmount = Mathf.Clamp01(fraction);
    }
}
