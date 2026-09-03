using UnityEngine;

public class DigitImageHelper : MonoBehaviour
{
    public static DigitImageHelper Instance { get; private set; }

    [SerializeField] private Sprite[] digitSprites = new Sprite[10];

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public Sprite GetDigitSprite(int digit)
    {
        if (digit < 0 || digit > 9)
        {
            Debug.LogWarning($"DigitImageHelper: digit {digit} is out of range 0-9");
            return null;
        }

        return digitSprites[digit];
    }
}
