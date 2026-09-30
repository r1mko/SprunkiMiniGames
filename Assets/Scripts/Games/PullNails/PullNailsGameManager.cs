using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class PullNailsGameManager : MonoBehaviour, IMiniGame
{
    public MiniGameType Type => MiniGameType.PullNails;

    [SerializeField, Required] private Pliers pliers;
    [SerializeField] private PullNail[] nails;
    [SerializeField, Required] private Image attemptsImage;
    [SerializeField] private int startingAttempts = 3;

    private int _attemptsRemaining;
    private bool _hasFinished;
    private bool _won;

    private void Awake()
    {
        pliers.GrabFinished += OnGrabFinished;
    }

    private void OnDestroy()
    {
        pliers.GrabFinished -= OnGrabFinished;
    }

    private void OnDisable()
    {
        ResetGame();
    }

    public void StartGame()
    {
        pliers.StartSwinging();
        StartCoroutine(ListenForClickRoutine());
    }

    [Button("Reset", EButtonEnableMode.Playmode)]
    public void ResetGame()
    {
        StopAllCoroutines();
        pliers.ResetPliers();

        foreach (PullNail nail in nails)
        {
            nail.ResetNail();
        }

        _attemptsRemaining = startingAttempts;
        _hasFinished = false;
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
            if (_attemptsRemaining > 0 && pliers.IsReady && PointerInput.WasPressedThisFrame())
            {
                _attemptsRemaining--;
                UpdateAttemptsImage();
                pliers.Grab();
            }

            yield return null;
        }
    }

    private void OnGrabFinished()
    {
        if (_hasFinished)
        {
            return;
        }

        if (AreAllNailsPulled())
        {
            FinishGame(true);
        }
        else if (_attemptsRemaining <= 0)
        {
            FinishGame(false);
        }
    }

    private bool AreAllNailsPulled()
    {
        foreach (PullNail nail in nails)
        {
            if (!nail.IsPulled)
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
        pliers.StopMoving();
        CheckResult();
    }

    private void UpdateAttemptsImage()
    {
        attemptsImage.sprite = DigitImageHelper.Instance.GetDigitSprite(_attemptsRemaining);
    }
}
