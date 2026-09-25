using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ShootBalloonsGameManager : MonoBehaviour, IMiniGame
{
    public MiniGameType Type => MiniGameType.ShootBalloons;

    [SerializeField] private ShootNeedle[] needles;
    [SerializeField] private ShootBalloon[] balloons;
    [SerializeField, Required] private PingPongMover obstacleMover;
    [SerializeField, Required] private Image attemptsImage;
    [SerializeField] private float nextNeedleDelay = 0.5f;
    [SerializeField] private bool obstacleStartsRight = true;

    private int _currentNeedleIndex;
    private int _attemptsRemaining;
    private bool _hasFinished;
    private bool _isBusy;
    private bool _won;

    private void Awake()
    {
        foreach (ShootNeedle needle in needles)
        {
            needle.Finished += OnNeedleFinished;
        }
    }

    private void OnDestroy()
    {
        foreach (ShootNeedle needle in needles)
        {
            needle.Finished -= OnNeedleFinished;
        }
    }

    private void OnDisable()
    {
        ResetGame();
    }

    private float ObstacleDirection => obstacleStartsRight ? 1f : -1f;

    public void StartGame()
    {
        obstacleMover.StartMoving(ObstacleDirection);

        if (needles.Length > 0)
        {
            needles[_currentNeedleIndex].StartSliding(-ObstacleDirection);
        }

        StartCoroutine(ListenForClickRoutine());
    }

    public void ResetGame()
    {
        StopAllCoroutines();
        obstacleMover.ResetMover();

        foreach (ShootNeedle needle in needles)
        {
            needle.ResetNeedle();
        }

        foreach (ShootBalloon balloon in balloons)
        {
            balloon.ResetBalloon();
        }

        _currentNeedleIndex = 0;
        _attemptsRemaining = needles.Length;
        _hasFinished = false;
        _isBusy = false;
        _won = false;

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
        _isBusy = true;
        _attemptsRemaining--;
        UpdateAttemptsImage();

        needles[_currentNeedleIndex].Launch();
    }

    private void OnNeedleFinished(ShootNeedle needle)
    {
        if (_hasFinished)
        {
            return;
        }

        if (AreAllBalloonsPopped())
        {
            FinishGame(true);
            return;
        }

        if (_attemptsRemaining <= 0)
        {
            FinishGame(false);
            return;
        }

        StartCoroutine(NextNeedleRoutine());
    }

    private IEnumerator NextNeedleRoutine()
    {
        yield return new WaitForSeconds(nextNeedleDelay);

        _currentNeedleIndex++;
        needles[_currentNeedleIndex].StartSliding(-ObstacleDirection);
        _isBusy = false;
    }

    private bool AreAllBalloonsPopped()
    {
        foreach (ShootBalloon balloon in balloons)
        {
            if (!balloon.IsPopped)
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
        obstacleMover.StopMoving();
        CheckResult();
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
