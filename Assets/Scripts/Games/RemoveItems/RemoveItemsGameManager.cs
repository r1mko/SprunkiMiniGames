using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class RemoveItemsGameManager : MonoBehaviour, IMiniGame
{
    public MiniGameType Type => MiniGameType.RemoveItems;

    [SerializeField, Required] private Bird bird;
    [SerializeField] private Item[] items;

    [SerializeField, Required] private Image attemptsImage;
    [SerializeField] private int startingAttempts = 5;

    private int _totalRemovable;
    private int _remainingRemovable;
    private int _attemptsRemaining;
    private bool _hasFinished;
    private bool _won;

    public bool CanPeck => !_hasFinished && _attemptsRemaining > 0;

    private void Awake()
    {
        foreach (Item item in items)
        {
            if (item.Type == ItemType.Removable)
            {
                _totalRemovable++;
            }
        }
    }

    private void OnDisable()
    {
        ResetGame();
    }

    public void StartGame()
    {
        bird.StartFlying();
    }

    public void ResetGame()
    {
        bird.ResetBird();

        foreach (Item item in items)
        {
            item.ResetItem();
        }

        _remainingRemovable = _totalRemovable;
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

    public void OnItemRemoved()
    {
        if (_hasFinished)
        {
            return;
        }

        _remainingRemovable--;

        if (_remainingRemovable <= 0)
        {
            _hasFinished = true;
            _won = true;
            CheckResult();
        }
    }

    public void OnRottenHit()
    {
        if (_hasFinished)
        {
            return;
        }

        bird.Freeze();

        _hasFinished = true;
        _won = false;
        CheckResult();
    }

    public void OnPeckStarted()
    {
        if (!CanPeck)
        {
            return;
        }

        _attemptsRemaining--;
        UpdateAttemptsImage();
    }

    public void OnPeckFinished()
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
