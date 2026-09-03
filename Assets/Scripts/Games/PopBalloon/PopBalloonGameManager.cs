using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PopBalloonGameManager : MonoBehaviour, IMiniGame
{
    public MiniGameType Type => MiniGameType.PopBalloon;

    [SerializeField, Required] private Needle needle;
    [SerializeField, Required] private Balloon balloon;
    [SerializeField, Required] private Transform obstacle;
    [SerializeField] private float obstacleRotationSpeed = 30f;
    [SerializeField] private float stuckFallbackDelay = 4f;
    [SerializeField] private float missResetDelay = 0.5f;

    [SerializeField, Required] private Image attemptsImage;
    [SerializeField] private int startingAttempts = 3;

    private int _attemptsRemaining;
    private bool _hasFinished;
    private bool _isBusy;
    private bool _outcomeDecided;
    private bool _won;
    private Coroutine _fallbackRoutine;
    private Coroutine _obstacleRotationRoutine;

    private void OnDisable()
    {
        ResetGame();
    }

    public void StartGame()
    {
        StartCoroutine(ListenForClickRoutine());
        _obstacleRotationRoutine = StartCoroutine(RotateObstacleRoutine());
    }

    public void ResetGame()
    {
        StopAllCoroutines();
        _fallbackRoutine = null;
        _obstacleRotationRoutine = null;
        _attemptsRemaining = startingAttempts;
        _hasFinished = false;
        _isBusy = false;
        _outcomeDecided = false;
        _won = false;

        needle.ResetNeedle();
        balloon.ResetBalloon();
        UpdateAttemptsImage();
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

    public void OnBalloonHit()
    {
        if (_outcomeDecided)
        {
            return;
        }

        _outcomeDecided = true;
        _hasFinished = true;
        _won = true;
        StopFallback();
        StopObstacleRotation();
        needle.StopObstacleWobble();
        CheckResult();
    }

    public void OnNeedleMissed()
    {
        if (_outcomeDecided)
        {
            return;
        }

        _outcomeDecided = true;
        StopFallback();
        needle.Freeze();

        StartCoroutine(DelayedMissResetRoutine());
    }

    private IEnumerator DelayedMissResetRoutine()
    {
        yield return new WaitForSeconds(missResetDelay);

        needle.ResetNeedle();
        _isBusy = false;

        if (_attemptsRemaining <= 0)
        {
            _hasFinished = true;
            _won = false;
            CheckResult();
        }
    }

    private IEnumerator ListenForClickRoutine()
    {
        while (!_hasFinished)
        {
            if (!_isBusy && WasPointerPressedThisFrame())
            {
                Throw();
            }

            yield return null;
        }
    }

    private void Throw()
    {
        if (_attemptsRemaining <= 0)
        {
            return;
        }

        _isBusy = true;
        _outcomeDecided = false;
        _attemptsRemaining--;
        UpdateAttemptsImage();

        needle.Launch();
        _fallbackRoutine = StartCoroutine(FallbackRoutine());
    }

    private IEnumerator FallbackRoutine()
    {
        yield return new WaitForSeconds(stuckFallbackDelay);

        _fallbackRoutine = null;
        OnNeedleMissed();
    }

    private void StopFallback()
    {
        if (_fallbackRoutine != null)
        {
            StopCoroutine(_fallbackRoutine);
            _fallbackRoutine = null;
        }
    }

    private IEnumerator RotateObstacleRoutine()
    {
        while (true)
        {
            obstacle.Rotate(0f, 0f, obstacleRotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private void StopObstacleRotation()
    {
        if (_obstacleRotationRoutine != null)
        {
            StopCoroutine(_obstacleRotationRoutine);
            _obstacleRotationRoutine = null;
        }
    }

    private void UpdateAttemptsImage()
    {
        attemptsImage.sprite = DigitImageHelper.Instance.GetDigitSprite(_attemptsRemaining);
    }

    private static bool WasPointerPressedThisFrame()
    {
        return Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
    }
}
