using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class CutHalfGameManager : MonoBehaviour, IMiniGame
{
    public MiniGameType Type => MiniGameType.CutHalf;

    [SerializeField, Required] private Knife knife;
    [SerializeField, Required] private CuttableObject cuttableObject;
    [SerializeField] private float percentStep = 1.25f;

    [SerializeField, Required] private Image leftHundredsDigitImage;
    [SerializeField, Required] private Image leftTensDigitImage;
    [SerializeField, Required] private Image leftOnesDigitImage;

    [SerializeField, Required] private Image rightHundredsDigitImage;
    [SerializeField, Required] private Image rightTensDigitImage;
    [SerializeField, Required] private Image rightOnesDigitImage;

    public bool CanCut => !_hasAttempted;

    private bool _hasAttempted;
    private bool _won;

    private void OnDisable()
    {
        ResetGame();
    }

    public void StartGame()
    {
        knife.StartSwinging();
    }

    public void ResetGame()
    {
        knife.ResetKnife();
        cuttableObject.ResetObject();
        _hasAttempted = false;
        _won = false;

        UpdatePercentDisplay(0f, 0f);
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

    public void OnCutComplete()
    {
        CheckResult();
    }

    public void OnKnifeReachedTarget(float knifeWorldX, float horizontalOffset)
    {
        _hasAttempted = true;

        float offsetFromCenter = knifeWorldX - cuttableObject.transform.position.x;
        float t = Mathf.Clamp(offsetFromCenter / horizontalOffset, -1f, 1f);
        float leftPercent = (1f - t) / 2f * 100f;
        leftPercent = Mathf.Clamp(Mathf.Round(leftPercent / percentStep) * percentStep, 1f, 99f);
        float rightPercent = 100f - leftPercent;

        _won = Mathf.Approximately(leftPercent, 50f);

        UpdatePercentDisplay(leftPercent, rightPercent);

        cuttableObject.RevealCut();
        cuttableObject.Cut(knifeWorldX);
    }

    private void UpdatePercentDisplay(float leftPercent, float rightPercent)
    {
        SetDigits(leftHundredsDigitImage, leftTensDigitImage, leftOnesDigitImage, Mathf.RoundToInt(leftPercent));
        SetDigits(rightHundredsDigitImage, rightTensDigitImage, rightOnesDigitImage, Mathf.RoundToInt(rightPercent));
    }

    private static void SetDigits(Image hundreds, Image tens, Image ones, int value)
    {
        value = Mathf.Clamp(value, 0, 100);
        int hundredsDigit = value / 100;
        int tensDigit = (value / 10) % 10;
        int onesDigit = value % 10;

        bool showTens = hundredsDigit > 0 || tensDigit > 0;

        hundreds.gameObject.SetActive(hundredsDigit > 0);

        if (hundredsDigit > 0)
        {
            hundreds.sprite = DigitImageHelper.Instance.GetDigitSprite(hundredsDigit);
        }

        tens.gameObject.SetActive(showTens);

        if (showTens)
        {
            tens.sprite = DigitImageHelper.Instance.GetDigitSprite(tensDigit);
        }

        ones.sprite = DigitImageHelper.Instance.GetDigitSprite(onesDigit);
    }
}
