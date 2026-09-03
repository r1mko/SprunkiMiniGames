using System;
using System.Collections;
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

    [Header("Pop Animation")]
    [SerializeField] private float popDuration = 0.25f;
    [SerializeField] private AnimationCurve showScaleCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.7f, 1.2f),
        new Keyframe(1f, 1f));
    [SerializeField] private float showRotationAngle = 25f;
    [SerializeField] private AnimationCurve showRotationCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.33f, -1f),
        new Keyframe(0.66f, 1f),
        new Keyframe(1f, 0f));

    [Header("Curtain Transition")]
    [SerializeField, Required] private Transform curtain;
    [SerializeField] private float curtainTransitionDuration = 0.4f;
    [SerializeField] private float curtainFullScaleY = 1f;

    private Vector3 _curtainBaseScale;

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

        _curtainBaseScale = curtain.localScale;
        curtain.localScale = new Vector3(_curtainBaseScale.x, 0f, _curtainBaseScale.z);

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
        StartCoroutine(ShowScreenRoutine(retryScreen, retryImage.transform, new[] { retryButton.transform, skipButton.transform }));
    }

    public void ShowNextScreen()
    {
        nextImage.sprite = GetRandomSprite(nextSprites);
        StartCoroutine(ShowScreenRoutine(nextScreen, nextImage.transform, new[] { nextButton.transform, nextScreenRetryButton.transform }));
    }

    private void OnRetryClicked()
    {
        PlayCurtainTransition(() =>
        {
            retryScreen.SetActive(false);
            GameManager.Instance.ResetGame();
            GameManager.Instance.StartGame();
        });
    }

    private void OnSkipClicked()
    {
        Debug.Log("Skip button pressed");
    }

    private void OnNextClicked()
    {
        PlayCurtainTransition(() =>
        {
            nextScreen.SetActive(false);
            GameManager.Instance.NextGame();
        });
    }

    private void OnNextScreenRetryClicked()
    {
        PlayCurtainTransition(() =>
        {
            nextScreen.SetActive(false);
            GameManager.Instance.ResetGame();
            GameManager.Instance.StartGame();
        });
    }

    private IEnumerator ShowScreenRoutine(GameObject screen, Transform face, Transform[] buttons)
    {
        screen.SetActive(true);

        foreach (Transform button in buttons)
        {
            button.gameObject.SetActive(false);
        }

        yield return ShowPop(face);

        foreach (Transform button in buttons)
        {
            ShowPop(button);
        }
    }

    private void PlayCurtainTransition(Action onCovered, Action onRevealed = null)
    {
        StartCoroutine(CurtainTransitionRoutine(onCovered, onRevealed));
    }

    private IEnumerator CurtainTransitionRoutine(Action onCovered, Action onRevealed)
    {
        float elapsed = 0f;

        while (elapsed < curtainTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / curtainTransitionDuration);
            Vector3 scale = curtain.localScale;
            scale.y = Mathf.Lerp(0f, curtainFullScaleY, t);
            curtain.localScale = scale;
            yield return null;
        }

        curtain.localScale = new Vector3(_curtainBaseScale.x, curtainFullScaleY, _curtainBaseScale.z);

        onCovered?.Invoke();

        elapsed = 0f;

        while (elapsed < curtainTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / curtainTransitionDuration);
            Vector3 scale = curtain.localScale;
            scale.y = Mathf.Lerp(curtainFullScaleY, 0f, t);
            curtain.localScale = scale;
            yield return null;
        }

        curtain.localScale = new Vector3(_curtainBaseScale.x, 0f, _curtainBaseScale.z);

        onRevealed?.Invoke();
    }

    private Coroutine ShowPop(Transform target)
    {
        target.gameObject.SetActive(true);
        return StartCoroutine(ShowPopRoutine(target));
    }

    private IEnumerator ShowPopRoutine(Transform target)
    {
        float elapsed = 0f;

        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / popDuration);
            float scale = showScaleCurve.Evaluate(t);
            float rotationZ = showRotationCurve.Evaluate(t) * showRotationAngle;

            target.localScale = Vector3.one * scale;
            target.localRotation = Quaternion.Euler(0f, 0f, rotationZ);

            yield return null;
        }

        target.localScale = Vector3.one * showScaleCurve.Evaluate(1f);
        target.localRotation = Quaternion.identity;
    }

    private static Sprite GetRandomSprite(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length == 0)
        {
            return null;
        }

        return sprites[UnityEngine.Random.Range(0, sprites.Length)];
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
