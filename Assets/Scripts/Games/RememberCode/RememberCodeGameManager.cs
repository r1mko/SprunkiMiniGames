using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class RememberCodeGameManager : MonoBehaviour, IMiniGame
{
    public MiniGameType Type => MiniGameType.RememberCode;

    [SerializeField, Required] private Camera worldCamera;
    [SerializeField] private Sprite[] digitSprites = new Sprite[10];

    [Header("Paper")]
    [SerializeField, Required] private Animator paperAnimator;
    [SerializeField, AnimatorParam("paperAnimator")] private string showNumbersTrigger;
    [SerializeField, AnimatorParam("paperAnimator")] private string showResultTrigger;
    [SerializeField] private string initialStateName = "Idle";
    [SerializeField] private SpriteRenderer[] codeSlots;

    [Header("Input")]
    [SerializeField, Required] private Collider2D playButton;
    [SerializeField] private SpriteRenderer[] inputSlots;
    [SerializeField] private CodeKey[] keys;
    [SerializeField, Required] private Collider2D eraseButton;

    [SerializeField] private float resultDelay = 1.5f;

    private readonly List<int> _code = new List<int>();
    private readonly List<int> _enteredDigits = new List<int>();
    private bool _isInputLocked;
    private bool _won;

    private void OnDisable()
    {
        ResetGame();
    }

    public void StartGame()
    {
        GenerateCode();
        StartCoroutine(ListenForClickRoutine());
    }

    public void ResetGame()
    {
        StopAllCoroutines();

        _code.Clear();
        _enteredDigits.Clear();
        _isInputLocked = false;
        _won = false;

        foreach (SpriteRenderer slot in inputSlots)
        {
            slot.gameObject.SetActive(false);
        }

        foreach (CodeKey key in keys)
        {
            key.SetSprite(digitSprites[key.Digit]);
        }

        playButton.gameObject.SetActive(true);
        SetKeypadVisible(false);
        ResetPaperAnimator();
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

    private void GenerateCode()
    {
        List<int> pool = new List<int>();
        for (int digit = 0; digit < digitSprites.Length; digit++)
        {
            pool.Add(digit);
        }

        _code.Clear();
        for (int i = 0; i < codeSlots.Length; i++)
        {
            int poolIndex = Random.Range(0, pool.Count);
            _code.Add(pool[poolIndex]);
            pool.RemoveAt(poolIndex);

            codeSlots[i].sprite = digitSprites[_code[i]];
        }
    }

    private IEnumerator ListenForClickRoutine()
    {
        while (!_isInputLocked)
        {
            if (PointerInput.TryGetPressedThisFrame(out Vector2 screenPosition))
            {
                HandleClick(GetClickedCollider(screenPosition));
            }

            yield return null;
        }
    }

    private void HandleClick(Collider2D clicked)
    {
        if (clicked == null)
        {
            return;
        }

        if (clicked == playButton)
        {
            ShowNumbers();
            return;
        }

        if (clicked == eraseButton)
        {
            EraseLastDigit();
            return;
        }

        if (clicked.TryGetComponent(out CodeKey key))
        {
            EnterDigit(key.Digit);
        }
    }

    private void ShowNumbers()
    {
        playButton.gameObject.SetActive(false);
        SetKeypadVisible(true);
        paperAnimator.SetTrigger(showNumbersTrigger);
    }

    private void SetKeypadVisible(bool isVisible)
    {
        foreach (CodeKey key in keys)
        {
            key.gameObject.SetActive(isVisible);
        }

        eraseButton.gameObject.SetActive(isVisible);
    }

    private void EnterDigit(int digit)
    {
        if (_enteredDigits.Count >= inputSlots.Length)
        {
            return;
        }

        SpriteRenderer slot = inputSlots[_enteredDigits.Count];
        _enteredDigits.Add(digit);
        slot.sprite = digitSprites[digit];
        slot.gameObject.SetActive(true);

        if (_enteredDigits.Count == inputSlots.Length)
        {
            StartCoroutine(ShowResultRoutine());
        }
    }

    private void EraseLastDigit()
    {
        if (_enteredDigits.Count == 0)
        {
            return;
        }

        _enteredDigits.RemoveAt(_enteredDigits.Count - 1);
        inputSlots[_enteredDigits.Count].gameObject.SetActive(false);
    }

    private IEnumerator ShowResultRoutine()
    {
        _isInputLocked = true;
        _won = IsCodeCorrect();
        paperAnimator.SetTrigger(showResultTrigger);

        yield return new WaitForSeconds(resultDelay);

        CheckResult();
    }

    private bool IsCodeCorrect()
    {
        if (_enteredDigits.Count != _code.Count)
        {
            return false;
        }

        for (int i = 0; i < _code.Count; i++)
        {
            if (_enteredDigits[i] != _code[i])
            {
                return false;
            }
        }

        return true;
    }

    private void ResetPaperAnimator()
    {
        if (!paperAnimator.isActiveAndEnabled)
        {
            return;
        }

        paperAnimator.ResetTrigger(showNumbersTrigger);
        paperAnimator.ResetTrigger(showResultTrigger);
        paperAnimator.Play(initialStateName, 0, 0f);
        paperAnimator.Update(0f);
    }

    private Collider2D GetClickedCollider(Vector2 screenPosition)
    {
        Ray ray = worldCamera.ScreenPointToRay(screenPosition);
        return Physics2D.GetRayIntersection(ray).collider;
    }
}
