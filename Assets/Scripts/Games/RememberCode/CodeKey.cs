using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class CodeKey : MonoBehaviour
{
    [SerializeField, Range(0, 9)] private int digit;

    public int Digit => digit;

    public void SetSprite(Sprite sprite)
    {
        GetComponent<SpriteRenderer>().sprite = sprite;
    }
}
