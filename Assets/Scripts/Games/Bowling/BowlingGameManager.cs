using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class BowlingGameManager : MonoBehaviour, IMiniGame
{
    public MiniGameType Type => MiniGameType.Bowling;

    [SerializeField] private Ball[] balls;
    [SerializeField] private Skittle[] skittles;
    [SerializeField, Required] private Image throwsImage;
    [SerializeField] private float nextBallDelay = 0.25f;

    public bool CanThrow => !_hasFinished;

    private int _currentBallIndex;
    private int _throwsRemaining;
    private int _resolvedThrowCount;
    private int _skittlesRemaining;
    private bool _hasFinished;
    private bool _won;
    private Coroutine _nextBallRoutine;

    private void OnDisable()
    {
        ResetGame();
    }

    public void StartGame()
    {
        balls[_currentBallIndex].StartSwinging();
    }

    public void ResetGame()
    {
        if (_nextBallRoutine != null)
        {
            StopCoroutine(_nextBallRoutine);
            _nextBallRoutine = null;
        }

        foreach (Ball ball in balls)
        {
            ball.Hide();
        }

        foreach (Skittle skittle in skittles)
        {
            skittle.ResetSkittle();
        }

        _skittlesRemaining = skittles.Length;
        _throwsRemaining = balls.Length;
        _resolvedThrowCount = 0;
        _currentBallIndex = 0;
        _hasFinished = false;
        _won = false;

        if (balls.Length > 0)
        {
            balls[0].PrepareAndActivate();
        }

        UpdateThrowsImage();
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

    public void OnBallThrown(float launchX, bool launchMovingRight)
    {
        if (_hasFinished)
        {
            return;
        }

        _throwsRemaining--;
        UpdateThrowsImage();

        if (_throwsRemaining > 0)
        {
            _nextBallRoutine = StartCoroutine(ActivateNextBallRoutine(launchX, launchMovingRight));
        }
    }

    public void OnSkittleKnocked()
    {
        if (_hasFinished)
        {
            return;
        }

        _skittlesRemaining--;

        if (_skittlesRemaining <= 0)
        {
            FinishGame(true);
        }
    }

    public void OnThrowResolved()
    {
        if (_hasFinished)
        {
            return;
        }

        _resolvedThrowCount++;

        if (_skittlesRemaining <= 0)
        {
            FinishGame(true);
        }
        else if (_resolvedThrowCount >= balls.Length)
        {
            FinishGame(false);
        }
    }

    private IEnumerator ActivateNextBallRoutine(float launchX, bool launchMovingRight)
    {
        yield return new WaitForSeconds(nextBallDelay);
        _nextBallRoutine = null;

        if (_hasFinished)
        {
            yield break;
        }

        _currentBallIndex++;
        balls[_currentBallIndex].PrepareAndActivate(launchX, launchMovingRight);
        balls[_currentBallIndex].StartSwinging();
    }

    private void FinishGame(bool won)
    {
        _hasFinished = true;
        _won = won;

        if (_nextBallRoutine != null)
        {
            StopCoroutine(_nextBallRoutine);
            _nextBallRoutine = null;
        }

        CheckResult();
    }

    private void UpdateThrowsImage()
    {
        throwsImage.sprite = DigitImageHelper.Instance.GetDigitSprite(_throwsRemaining);
    }
}
