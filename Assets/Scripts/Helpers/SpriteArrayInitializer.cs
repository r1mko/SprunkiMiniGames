using NaughtyAttributes;
using UnityEngine;

public class SpriteArrayInitializer : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] spriteRenderers;
    [SerializeField] private Sprite[] sprites;

    [Button("Assign Sprites")]
    private void AssignSprites()
    {
        if (spriteRenderers.Length != sprites.Length)
        {
            Debug.LogWarning($"SpriteArrayInitializer: renderers ({spriteRenderers.Length}) and sprites ({sprites.Length}) count mismatch, assigning by the shorter length.");
        }

        int count = Mathf.Min(spriteRenderers.Length, sprites.Length);

        for (int i = 0; i < count; i++)
        {
            spriteRenderers[i].sprite = sprites[i];
        }
    }
}
