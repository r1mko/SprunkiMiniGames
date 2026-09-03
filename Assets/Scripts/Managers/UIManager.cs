using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Retry")]
    [SerializeField, Required] private GameObject retryScreen;
    [SerializeField, Required] private Image retryImage;
    [SerializeField] private Sprite[] retrySprites;
    [SerializeField, Required] private Button retryButton;
    [SerializeField, Required] private Button skipButton;

    [Header("Next")]
    [SerializeField, Required] private GameObject nextScreen;
    [SerializeField, Required] private Image nextImage;
    [SerializeField] private Sprite[] nextSprites;
    [SerializeField, Required] private Button nextButton;
    [SerializeField, Required] private Button nextScreenRetryButton;

    private int _retryTestIndex;
    private int _nextTestIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        retryScreen.SetActive(false);
        nextScreen.SetActive(false);

        retryButton.onClick.AddListener(OnRetryClicked);
        skipButton.onClick.AddListener(OnSkipClicked);
        nextButton.onClick.AddListener(OnNextClicked);
        nextScreenRetryButton.onClick.AddListener(OnNextScreenRetryClicked);
    }

    private void OnDestroy()
    {
        retryButton.onClick.RemoveListener(OnRetryClicked);
        skipButton.onClick.RemoveListener(OnSkipClicked);
        nextButton.onClick.RemoveListener(OnNextClicked);
        nextScreenRetryButton.onClick.RemoveListener(OnNextScreenRetryClicked);
    }

    public void ShowRetryScreen()
    {
        retryImage.sprite = GetRandomSprite(retrySprites);
        retryScreen.SetActive(true);
    }

    public void ShowNextScreen()
    {
        nextImage.sprite = GetRandomSprite(nextSprites);
        nextScreen.SetActive(true);
    }

    private void OnRetryClicked()
    {
        retryScreen.SetActive(false);
        GameManager.Instance.ResetGame();
    }

    private void OnSkipClicked()
    {
        Debug.Log("Skip button pressed");
    }

    private void OnNextClicked()
    {
        Debug.Log("Next button pressed");
    }

    private void OnNextScreenRetryClicked()
    {
        nextScreen.SetActive(false);
        GameManager.Instance.ResetGame();
    }

    private static Sprite GetRandomSprite(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length == 0)
        {
            return null;
        }

        return sprites[Random.Range(0, sprites.Length)];
    }

    [Button("Test Retry Image", EButtonEnableMode.Playmode)]
    private void TestNextRetryImage()
    {
        if (retrySprites == null || retrySprites.Length == 0)
        {
            return;
        }

        retryImage.sprite = retrySprites[_retryTestIndex];
        retryScreen.SetActive(true);
        _retryTestIndex = (_retryTestIndex + 1) % retrySprites.Length;
    }

    [Button("Test Next Image", EButtonEnableMode.Playmode)]
    private void TestNextNextImage()
    {
        if (nextSprites == null || nextSprites.Length == 0)
        {
            return;
        }

        nextImage.sprite = nextSprites[_nextTestIndex];
        nextScreen.SetActive(true);
        _nextTestIndex = (_nextTestIndex + 1) % nextSprites.Length;
    }
}
