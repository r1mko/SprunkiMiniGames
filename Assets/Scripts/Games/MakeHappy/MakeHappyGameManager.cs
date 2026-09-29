using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class MakeHappyGameManager : MonoBehaviour, IMiniGame
{
    public MiniGameType Type => MiniGameType.MakeHappy;

    [SerializeField, Required] private Spinner arrow;
    [SerializeField] private Transform[] characters;
    [SerializeField] private HappyTarget[] targets;
    [SerializeField, Required] private Image clicksImage;
    [SerializeField] private int startingClicks = 5;
    [SerializeField] private float resultDelay = 0.25f;

    private bool[] _isHappy;
    private int _clicksRemaining;
    private bool _hasFinished;
    private bool _won;

    private void OnDisable()
    {
        ResetGame();
    }

    public void StartGame()
    {
        arrow.StartSpinning();
        StartCoroutine(ListenForClickRoutine());
    }

    [Button("Reset", EButtonEnableMode.Playmode)]
    public void ResetGame()
    {
        StopAllCoroutines();
        arrow.ResetSpinner();

        _isHappy = new bool[targets.Length];
        foreach (HappyTarget target in targets)
        {
            target.SetHappy(false);
        }

        _clicksRemaining = startingClicks;
        _hasFinished = false;
        _won = false;

        UpdateClicksImage();
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

    private IEnumerator ListenForClickRoutine()
    {
        while (!_hasFinished)
        {
            if (_clicksRemaining > 0 && PointerInput.WasPressedThisFrame())
            {
                Click();
            }

            yield return null;
        }
    }

    private void Click()
    {
        _clicksRemaining--;
        UpdateClicksImage();

        float angle = Mathf.Repeat(arrow.transform.localEulerAngles.z, 360f);

        for (int i = 0; i < targets.Length; i++)
        {
            if (!_isHappy[i] && targets[i].Contains(angle))
            {
                _isHappy[i] = true;
                targets[i].SetHappy(true);
                break;
            }
        }

        if (AreAllHappy())
        {
            FinishGame(true);
        }
        else if (_clicksRemaining <= 0)
        {
            FinishGame(false);
        }
    }

    private bool AreAllHappy()
    {
        foreach (bool isHappy in _isHappy)
        {
            if (!isHappy)
            {
                return false;
            }
        }

        return true;
    }

    private void FinishGame(bool won)
    {
        _hasFinished = true;
        _won = won;
        StopAllCoroutines();
        arrow.StopSpinning();
        StartCoroutine(ResultRoutine());
    }

    private IEnumerator ResultRoutine()
    {
        yield return new WaitForSeconds(resultDelay);

        CheckResult();
    }

    [Button("Fill Targets From Characters", EButtonEnableMode.Editor)]
    private void FillTargetsFromCharacters()
    {
#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(this, "Fill Targets From Characters");
#endif

        HappyTarget[] filled = new HappyTarget[characters.Length];

        for (int i = 0; i < characters.Length; i++)
        {
            if (targets != null && i < targets.Length)
            {
                filled[i].fromAngle = targets[i].fromAngle;
                filled[i].toAngle = targets[i].toAngle;
            }

            filled[i].sadFace = characters[i].GetChild(0).GetComponent<SpriteRenderer>();
            filled[i].happyFace = characters[i].GetChild(1).GetComponent<SpriteRenderer>();
        }

        targets = filled;
    }

    private void UpdateClicksImage()
    {
        clicksImage.sprite = DigitImageHelper.Instance.GetDigitSprite(_clicksRemaining);
    }
}
