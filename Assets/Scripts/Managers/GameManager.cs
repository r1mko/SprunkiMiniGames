using System.Collections.Generic;
using UnityEngine;

public enum MiniGameType
{
    Harpoon,
    CleanOrange,
    PopBalloon,
    KnockIce,
    CatchEggs,
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private MiniGameType currentGameType = MiniGameType.Harpoon;

    private readonly Dictionary<MiniGameType, IMiniGame> _games = new Dictionary<MiniGameType, IMiniGame>();
    private readonly List<IMiniGame> _orderedGames = new List<IMiniGame>();
    private int _currentIndex;

    private IMiniGame CurrentGame => _currentIndex >= 0 && _currentIndex < _orderedGames.Count ? _orderedGames[_currentIndex] : null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (IMiniGame game in GetComponentsInChildren<IMiniGame>(true))
        {
            _games[game.Type] = game;
            _orderedGames.Add(game);
        }

        _currentIndex = _games.TryGetValue(currentGameType, out IMiniGame startGame)
            ? _orderedGames.IndexOf(startGame)
            : 0;
    }

    private void Start()
    {
        foreach (IMiniGame game in _orderedGames)
        {
            ((MonoBehaviour)game).gameObject.SetActive(false);
        }

        ActivateCurrentGame();
    }

    public void StartGame()
    {
        CurrentGame?.StartGame();
    }

    public void ResetGame()
    {
        CurrentGame?.ResetGame();
    }

    public void NextGame()
    {
        IMiniGame current = CurrentGame;

        if (current != null)
        {
            ((MonoBehaviour)current).gameObject.SetActive(false);
        }

        _currentIndex++;

        if (_currentIndex >= _orderedGames.Count)
        {
            _currentIndex = 0;
        }

        ActivateCurrentGame();
    }

    private void ActivateCurrentGame()
    {
        IMiniGame game = CurrentGame;

        if (game == null)
        {
            return;
        }

        ((MonoBehaviour)game).gameObject.SetActive(true);
        game.ResetGame();
        game.StartGame();
    }
}
