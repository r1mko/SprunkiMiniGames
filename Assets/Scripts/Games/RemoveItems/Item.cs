using NaughtyAttributes;
using UnityEngine;

public enum ItemType
{
    Removable,
    Rotten,
}

[RequireComponent(typeof(Collider2D))]
public class Item : MonoBehaviour
{
    [SerializeField] private ItemType itemType;

    [SerializeField, Required] private RemoveItemsGameManager gameManager;

    public ItemType Type => itemType;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out Bird bird) || bird.HasPecked)
        {
            return;
        }

        bird.MarkPecked();

        if (itemType == ItemType.Rotten)
        {
            gameManager.OnRottenHit();
        }
        else
        {
            gameObject.SetActive(false);
            gameManager.OnItemRemoved();
        }
    }

    public void ResetItem()
    {
        gameObject.SetActive(true);
    }
}
