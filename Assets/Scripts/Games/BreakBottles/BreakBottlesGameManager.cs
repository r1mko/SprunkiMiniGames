using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BreakBottlesGameManager : MonoBehaviour, IMiniGame
{
    public MiniGameType Type => MiniGameType.BreakBottles;

    [SerializeField, Required] private BottleHammer hammer;
    [SerializeField, Required] private Spinner bottlesSpinner;
    [SerializeField] private BreakableBottle[] bottles;
    [SerializeField, Required] private Image hitsImage;
    [SerializeField] private int startingHits = 7;

    private int _hitsRemaining;
    private bool _hasFinished;
    private bool _won;

    private void Awake()
    {
        hammer.StrikeFinished += OnStrikeFinished;
    }

    private void OnDestroy()
    {
        hammer.StrikeFinished -= OnStrikeFinished;
    }

    private void OnDisable()
    {
        ResetGame();
    }

    public void StartGame()
    {
        bottlesSpinner.StartSpinning();
        StartCoroutine(ListenForClickRoutine());
    }

    public void ResetGame()
    {
        StopAllCoroutines();
        bottlesSpinner.ResetSpinner();
        hammer.ResetHammer();

        foreach (BreakableBottle bottle in bottles)
        {
            bottle.ResetBottle();
        }

        _hitsRemaining = startingHits;
        _hasFinished = false;
        _won = false;

        UpdateHitsImage();
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

    private IEnumerator ListenForClickRoutine()
    {
        while (!_hasFinished)
        {
            if (_hitsRemaining > 0 && hammer.IsReady && WasPointerPressedThisFrame())
            {
                Strike();
            }

            yield return null;
        }
    }

    private void Strike()
    {
        _hitsRemaining--;
        UpdateHitsImage();

        hammer.Strike();
    }

    private void OnStrikeFinished()
    {
        if (_hasFinished)
        {
            return;
        }

        if (AreAllBottlesBroken())
        {
            FinishGame(true);
            return;
        }

        if (_hitsRemaining <= 0)
        {
            StartCoroutine(FinishAfterLastHitRoutine());
        }
    }

    private IEnumerator FinishAfterLastHitRoutine()
    {
        while (hammer.IsHitColliderActive)
        {
            yield return null;
        }

        FinishGame(AreAllBottlesBroken());
    }

    private bool AreAllBottlesBroken()
    {
        foreach (BreakableBottle bottle in bottles)
        {
            if (!bottle.IsBroken)
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
        bottlesSpinner.StopSpinning();
        CheckResult();
    }

    private void UpdateHitsImage()
    {
        hitsImage.sprite = DigitImageHelper.Instance.GetDigitSprite(_hitsRemaining);
    }

    private static bool WasPointerPressedThisFrame()
    {
        return Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
    }
}
