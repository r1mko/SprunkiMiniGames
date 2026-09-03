using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class CleanOrangeGameManager : MonoBehaviour, IMiniGame
{
    public MiniGameType Type => MiniGameType.CleanOrange;

    [SerializeField, Required] private Peeler peeler;
    [SerializeField, Required] private OrangePeelMask peelMask;
    [SerializeField, Required] private Image attemptsImage;
    [SerializeField] private int startingAttempts = 4;

    private int _attemptsRemaining;
    private bool _hasFinished;

    public bool CanClean => !_hasFinished && _attemptsRemaining > 0;

    public void StartGame()
    {
        peeler.StartSwinging();
    }

    public void ResetGame()
    {
        _attemptsRemaining = startingAttempts;
        _hasFinished = false;
        UpdateAttemptsImage();
        peelMask.ResetMask();
    }

    public void CheckResult()
    {
        if (peelMask.IsFullyCleaned)
        {
            UIManager.Instance.ShowNextScreen();
        }
        else
        {
            UIManager.Instance.ShowRetryScreen();
        }
    }

    public void OnPeelerClicked()
    {
        if (!CanClean)
        {
            return;
        }

        _attemptsRemaining--;
        UpdateAttemptsImage();
    }

    public void OnPeelerCleaned()
    {
        if (_hasFinished)
        {
            return;
        }

        if (peelMask.IsFullyCleaned || _attemptsRemaining <= 0)
        {
            _hasFinished = true;
            CheckResult();
        }
    }

    private void UpdateAttemptsImage()
    {
        attemptsImage.sprite = DigitImageHelper.Instance.GetDigitSprite(_attemptsRemaining);
    }
}
