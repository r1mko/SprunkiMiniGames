using NaughtyAttributes;
using UnityEngine;

public class KnockIceGameManager : MonoBehaviour, IMiniGame
{
    public MiniGameType Type => MiniGameType.KnockIce;

    [SerializeField, Required] private Spoon spoon;
    [SerializeField] private Block[] blocks;

    public bool CanKnock => !_hasFinished;

    private int _totalIce;
    private int _iceRemaining;
    private bool _hasFinished;
    private bool _won;

    private void Awake()
    {
        foreach (Block block in blocks)
        {
            if (block.Type == BlockType.Ice)
            {
                _totalIce++;
            }
        }
    }

    private void OnDisable()
    {
        ResetGame();
    }

    public void StartGame()
    {
        spoon.StartSwinging();
    }

    public void ResetGame()
    {
        spoon.ResetSpoon();

        foreach (Block block in blocks)
        {
            block.ResetBlock();
        }

        _iceRemaining = _totalIce;
        _hasFinished = false;
        _won = false;
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

    public void OnSpoonClicked()
    {
    }

    public void OnSpoonKnocked()
    {
    }

    public void OnIceKnocked()
    {
        if (_hasFinished)
        {
            return;
        }

        _iceRemaining--;

        if (_iceRemaining <= 0)
        {
            _hasFinished = true;
            _won = true;
            CheckResult();
        }
    }

    public void OnTreeKnocked()
    {
        if (_hasFinished)
        {
            return;
        }

        _hasFinished = true;
        _won = false;
        CheckResult();
    }
}
