using System.Collections.Generic;
using UnityEngine;

public enum MiniGameType
{
    None,
    Harpoon,
    CleanOrange,
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private MiniGameType currentGameType = MiniGameType.Harpoon;

    private readonly Dictionary<MiniGameType, IMiniGame> _games = new Dictionary<MiniGameType, IMiniGame>();

    private IMiniGame CurrentGame => _games.TryGetValue(currentGameType, out IMiniGame game) ? game : null;

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
            ((MonoBehaviour)game).gameObject.SetActive(false);
        }

        IMiniGame current = CurrentGame;

        if (current != null)
        {
            ((MonoBehaviour)current).gameObject.SetActive(true);
            current.ResetGame();
        }
    }

    public void StartGame()
    {
        CurrentGame?.StartGame();
    }

    public void ResetGame()
    {
        CurrentGame?.ResetGame();
    }
}
