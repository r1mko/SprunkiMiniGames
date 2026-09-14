using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class KillMosquitoGameManager : MonoBehaviour, IMiniGame
{
    public MiniGameType Type => MiniGameType.KillMosquito;

    [SerializeField, Required] private MosquitoNeedle needle;
    [SerializeField, Required] private Mosquito mosquito;

    [SerializeField, Required] private Image attemptsImage;
    [SerializeField] private int startingAttempts = 5;

    private int _attemptsRemaining;
    private bool _hasFinished;
    private bool _won;

    public bool CanStab => !_hasFinished && _attemptsRemaining > 0;

    private void OnDisable()
    {
        ResetGame();
    }

    public void StartGame()
    {
        needle.StartWaiting();
        mosquito.StartFlying();
    }

    public void ResetGame()
    {
        needle.ResetNeedle();
        mosquito.ResetMosquito();

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

    public void OnMosquitoHit()
    {
        if (_hasFinished)
        {
            return;
        }

        _hasFinished = true;
        _won = true;
        CheckResult();
    }

    public void OnStabStarted()
    {
        if (!CanStab)
        {
            return;
        }

        _attemptsRemaining--;
        UpdateAttemptsImage();
    }

    public void OnStabFinished()
    {
        if (_hasFinished)
        {
            return;
        }

        if (_attemptsRemaining <= 0)
        {
            _hasFinished = true;
            _won = false;
            CheckResult();
        }
    }

    private void UpdateAttemptsImage()
    {
        attemptsImage.sprite = DigitImageHelper.Instance.GetDigitSprite(_attemptsRemaining);
    }
}
