public interface IMiniGame
{
    MiniGameType Type { get; }

    void StartGame();
    void ResetGame();
    void CheckResult();
}
