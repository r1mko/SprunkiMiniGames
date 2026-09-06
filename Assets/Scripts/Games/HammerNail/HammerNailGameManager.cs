using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HammerNailGameManager : MonoBehaviour, IMiniGame
{
    public MiniGameType Type => MiniGameType.HammerNail;

    [SerializeField, Required] private Transform log;
    [SerializeField] private float logRotationSpeed = 30f;
    [SerializeField] private Nail[] nails;

    [SerializeField, Required] private Image tensDigitImage;
    [SerializeField, Required] private Image onesDigitImage;

    private int _currentNailIndex;
    private Coroutine _rotateRoutine;
    private Coroutine _inputRoutine;
    private bool _hasFinished;
    private bool _won;

    private void OnDisable()
    {
        ResetGame();
    }

    public void StartGame()
    {
        _rotateRoutine = StartCoroutine(RotateLogRoutine());
        _inputRoutine = StartCoroutine(ListenForClickRoutine());
    }

    public void ResetGame()
    {
        if (_rotateRoutine != null)
        {
            StopCoroutine(_rotateRoutine);
            _rotateRoutine = null;
        }

        if (_inputRoutine != null)
        {
            StopCoroutine(_inputRoutine);
            _inputRoutine = null;
        }

        foreach (Nail nail in nails)
        {
            nail.Hide();
        }

        _currentNailIndex = 0;
        _hasFinished = false;
        _won = false;

        if (nails.Length > 0)
        {
            nails[0].PrepareAndActivate();
        }

        UpdateRemainingNailsDisplay();
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

    public void OnNailHitLog()
    {
        if (_hasFinished)
        {
            return;
        }

        _currentNailIndex++;
        UpdateRemainingNailsDisplay();

        if (_currentNailIndex >= nails.Length)
        {
            _hasFinished = true;
            _won = true;
            CheckResult();
        }
        else
        {
            nails[_currentNailIndex].PrepareAndActivate();
        }
    }

    public void OnNailHitNail()
    {
        if (_hasFinished)
        {
            return;
        }

        _hasFinished = true;
        _won = false;

        if (_rotateRoutine != null)
        {
            StopCoroutine(_rotateRoutine);
            _rotateRoutine = null;
        }

        CheckResult();
    }

    private IEnumerator RotateLogRoutine()
    {
        while (true)
        {
            log.Rotate(0f, 0f, logRotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator ListenForClickRoutine()
    {
        while (!_hasFinished)
        {
            if (_currentNailIndex < nails.Length
                && WasPointerPressedThisFrame()
                && !nails[_currentNailIndex].IsFlying)
            {
                nails[_currentNailIndex].Launch();
            }

            yield return null;
        }
    }

    private void UpdateRemainingNailsDisplay()
    {
        int remaining = nails.Length - _currentNailIndex;
        int tens = remaining / 10;
        int ones = remaining % 10;

        tensDigitImage.gameObject.SetActive(tens > 0);

        if (tens > 0)
        {
            tensDigitImage.sprite = DigitImageHelper.Instance.GetDigitSprite(tens);
        }

        onesDigitImage.sprite = DigitImageHelper.Instance.GetDigitSprite(ones);
    }

    private static bool WasPointerPressedThisFrame()
    {
        return Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
    }
}
