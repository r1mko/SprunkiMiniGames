using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TableComputerGameManager : MonoBehaviour, IMiniGame
{
    public MiniGameType Type => MiniGameType.TableComputer;

    [SerializeField] private ThrowBall[] balls;
    [SerializeField, Required] private Image throwsImage;
    [SerializeField, Required] private Image powerBarImage;

    [SerializeField, Required] private Transform target;
    [SerializeField, Required] private Collider2D tableCollider;

    [SerializeField] private float chargeDuration = 1.5f;
    [SerializeField] private float minPowerMultiplier = 0.5f;
    [SerializeField] private float maxPowerMultiplier = 1.5f;
    [SerializeField] private float nextBallDelay = 1f;
    [SerializeField] private float settleLinearThreshold = 0.05f;
    [SerializeField] private float settleAngularThreshold = 5f;

    private int _currentBallIndex;
    private int _throwsRemaining;
    private bool _hasFinished;
    private bool _won;
    private Coroutine _inputRoutine;

    private void Awake()
    {
        foreach (ThrowBall ball in balls)
        {
            ball.HitObstacle += OnBallHitObstacle;
        }
    }

    private void OnDestroy()
    {
        foreach (ThrowBall ball in balls)
        {
            ball.HitObstacle -= OnBallHitObstacle;
        }
    }

    private void OnDisable()
    {
        ResetGame();
    }

    public void StartGame()
    {
        _inputRoutine = StartCoroutine(ListenForInputRoutine());
    }

    public void ResetGame()
    {
        StopInputRoutine();

        foreach (ThrowBall ball in balls)
        {
            ball.Hide();
        }

        _throwsRemaining = balls.Length;
        _currentBallIndex = 0;
        _hasFinished = false;
        _won = false;

        if (balls.Length > 0)
        {
            balls[0].PrepareAndActivate();
        }

        UpdateThrowsImage();
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

    private IEnumerator ListenForInputRoutine()
    {
        while (_throwsRemaining > 0)
        {
            if (WasPointerPressedThisFrame())
            {
                yield return ChargeAndLaunchRoutine();
            }
            else
            {
                yield return null;
            }
        }
    }

    private IEnumerator ChargeAndLaunchRoutine()
    {
        float elapsed = 0f;
        UpdatePowerBar(0f);

        while (elapsed < chargeDuration)
        {
            if (WasPointerReleasedThisFrame())
            {
                break;
            }

            elapsed += Time.deltaTime;
            UpdatePowerBar(elapsed / chargeDuration);
            yield return null;
        }

        float chargeFraction = Mathf.Clamp01(elapsed / chargeDuration);
        LaunchCurrentBall(chargeFraction);
        UpdatePowerBar(0f);

        if (_throwsRemaining > 0)
        {
            yield return new WaitForSeconds(nextBallDelay);
            _currentBallIndex++;
            balls[_currentBallIndex].PrepareAndActivate();
        }
        else
        {
            yield return null;

            ThrowBall lastBall = balls[_currentBallIndex];
            while (!lastBall.IsSettled(settleLinearThreshold, settleAngularThreshold))
            {
                yield return null;
            }

            FinishGame(AreAllBallsOnTable());
        }
    }

    private void OnBallHitObstacle(ThrowBall ball)
    {
        if (_hasFinished)
        {
            return;
        }

        StopInputRoutine();
        FinishGame(false);
    }

    private bool AreAllBallsOnTable()
    {
        foreach (ThrowBall ball in balls)
        {
            if (!ball.IsTouching(tableCollider))
            {
                return false;
            }
        }

        return true;
    }

    private void LaunchCurrentBall(float chargeFraction)
    {
        ThrowBall ball = balls[_currentBallIndex];

        if (!Ballistics2D.TryCalculateLaunchVelocity(ball.transform.position, target.position, ball.LaunchAngle, ball.Gravity, out Vector2 perfectVelocity))
        {
            Debug.LogWarning($"{ball.name}: target is unreachable at launch angle {ball.LaunchAngle}");
        }

        ball.Launch(perfectVelocity *GetPowerMultiplier(chargeFraction, ball.PerfectZone));

        _throwsRemaining--;
        UpdateThrowsImage();
    }

    private void StopInputRoutine()
    {
        if (_inputRoutine != null)
        {
            StopCoroutine(_inputRoutine);
            _inputRoutine = null;
        }
    }

    private float GetPowerMultiplier(float chargeFraction, Vector2 perfectZone)
    {
        if (chargeFraction < perfectZone.x)
        {
            return Mathf.Lerp(minPowerMultiplier, 1f, Mathf.InverseLerp(0f, perfectZone.x, chargeFraction));
        }

        if (chargeFraction > perfectZone.y)
        {
            return Mathf.Lerp(1f, maxPowerMultiplier, Mathf.InverseLerp(perfectZone.y, 1f, chargeFraction));
        }

        return 1f;
    }

    private void FinishGame(bool won)
    {
        _hasFinished = true;
        _won = won;
        CheckResult();
    }

    private void UpdateThrowsImage()
    {
        throwsImage.sprite = DigitImageHelper.Instance.GetDigitSprite(_throwsRemaining);
    }

    private void UpdatePowerBar(float fraction)
    {
        powerBarImage.fillAmount = Mathf.Clamp01(fraction);
    }

    private static bool WasPointerPressedThisFrame()
    {
        return Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
    }

    private static bool WasPointerReleasedThisFrame()
    {
        return Pointer.current != null && Pointer.current.press.wasReleasedThisFrame;
    }
}
