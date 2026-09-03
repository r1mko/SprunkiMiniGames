using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class OrangePeelMask : MonoBehaviour
{
    [SerializeField] private int maskResolution = 128;
    [Range(0f, 1f)]
    [SerializeField] private float completionThreshold = 0.95f;

    [Header("Working Area")]
    [Tooltip("Shrinks the sprite bounds on each side (per axis), e.g. to exclude transparent padding.")]
    [SerializeField] private Vector2 boundsInset = Vector2.zero;
    [Tooltip("Shifts the working area center relative to the sprite.")]
    [SerializeField] private Vector2 boundsOffset = Vector2.zero;

    private SpriteRenderer _spriteRenderer;
    private MaterialPropertyBlock _propertyBlock;
    private Texture2D _maskTexture;
    private Bounds _localBounds;

    private bool[] _erasedCells;
    private int _erasedCount;

    public bool IsFullyCleaned { get; private set; }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _propertyBlock = new MaterialPropertyBlock();

        _localBounds = ComputeInsetBounds(_spriteRenderer.sprite.bounds, boundsInset, boundsOffset);

        _maskTexture = new Texture2D(maskResolution, maskResolution, TextureFormat.R8, false)
        {
            wrapMode = TextureWrapMode.Clamp
        };

        _erasedCells = new bool[maskResolution * maskResolution];
        FillMaskWhite();

        _spriteRenderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetTexture("_MaskTex", _maskTexture);
        _propertyBlock.SetVector("_BoundsMin", new Vector4(_localBounds.min.x, _localBounds.min.y, 0f, 0f));
        _propertyBlock.SetVector("_BoundsSize", new Vector4(_localBounds.size.x, _localBounds.size.y, 0f, 0f));
        _spriteRenderer.SetPropertyBlock(_propertyBlock);
    }

    public void ResetMask()
    {
        FillMaskWhite();
        System.Array.Clear(_erasedCells, 0, _erasedCells.Length);
        _erasedCount = 0;
        IsFullyCleaned = false;
    }

    private void FillMaskWhite()
    {
        Color32[] fill = new Color32[maskResolution * maskResolution];
        for (int i = 0; i < fill.Length; i++)
        {
            fill[i] = new Color32(255, 255, 255, 255);
        }

        _maskTexture.SetPixels32(fill);
        _maskTexture.Apply();
    }

    public void EraseAt(Vector3 worldPosition, Vector2 eraseSize)
    {
        DrawDebugMarker(worldPosition, eraseSize);

        Vector3 local = transform.InverseTransformPoint(worldPosition);
        float u = (local.x - _localBounds.min.x) / _localBounds.size.x;
        float v = (local.y - _localBounds.min.y) / _localBounds.size.y;

        int centerX = Mathf.RoundToInt(u * maskResolution);
        int centerY = Mathf.RoundToInt(v * maskResolution);

        Vector2 localEraseSize = new Vector2(eraseSize.x / transform.lossyScale.x, eraseSize.y / transform.lossyScale.y);

        int halfWidth = Mathf.Max(1, Mathf.RoundToInt(localEraseSize.x / _localBounds.size.x * maskResolution * 0.5f));
        int halfHeight = Mathf.Max(1, Mathf.RoundToInt(localEraseSize.y / _localBounds.size.y * maskResolution * 0.5f));

        int minX = Mathf.Clamp(centerX - halfWidth, 0, maskResolution - 1);
        int maxX = Mathf.Clamp(centerX + halfWidth, 0, maskResolution - 1);
        int minY = Mathf.Clamp(centerY - halfHeight, 0, maskResolution - 1);
        int maxY = Mathf.Clamp(centerY + halfHeight, 0, maskResolution - 1);

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;

        if (width <= 0 || height <= 0)
        {
            return;
        }

        Color32[] block = new Color32[width * height];
        _maskTexture.SetPixels32(minX, minY, width, height, block);
        _maskTexture.Apply();

        MarkErased(minX, minY, maxX, maxY);
    }

    private static Bounds ComputeInsetBounds(Bounds spriteBounds, Vector2 inset, Vector2 offset)
    {
        Vector3 min = spriteBounds.min + new Vector3(inset.x, inset.y, 0f);
        Vector3 max = spriteBounds.max - new Vector3(inset.x, inset.y, 0f);

        if (min.x > max.x)
        {
            float midX = (min.x + max.x) * 0.5f;
            min.x = midX;
            max.x = midX;
        }

        if (min.y > max.y)
        {
            float midY = (min.y + max.y) * 0.5f;
            min.y = midY;
            max.y = midY;
        }

        Bounds bounds = new Bounds();
        bounds.SetMinMax(min, max);
        bounds.center += new Vector3(offset.x, offset.y, 0f);
        return bounds;
    }

    private static void DrawDebugMarker(Vector3 worldPosition, Vector2 size)
    {
        float halfWidth = size.x * 0.5f;
        float halfHeight = size.y * 0.5f;

        Vector3 topLeft = worldPosition + new Vector3(-halfWidth, halfHeight, 0f);
        Vector3 topRight = worldPosition + new Vector3(halfWidth, halfHeight, 0f);
        Vector3 bottomLeft = worldPosition + new Vector3(-halfWidth, -halfHeight, 0f);
        Vector3 bottomRight = worldPosition + new Vector3(halfWidth, -halfHeight, 0f);

        Debug.DrawLine(topLeft, topRight, Color.red, 1f);
        Debug.DrawLine(topRight, bottomRight, Color.red, 1f);
        Debug.DrawLine(bottomRight, bottomLeft, Color.red, 1f);
        Debug.DrawLine(bottomLeft, topLeft, Color.red, 1f);
    }

    private void MarkErased(int minX, int minY, int maxX, int maxY)
    {
        if (IsFullyCleaned)
        {
            return;
        }

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                int index = y * maskResolution + x;

                if (_erasedCells[index])
                {
                    continue;
                }

                _erasedCells[index] = true;
                _erasedCount++;
            }
        }

        if (_erasedCount >= _erasedCells.Length * completionThreshold)
        {
            IsFullyCleaned = true;
            Debug.Log("OrangePeel: fully cleaned!");
        }
    }

    private void OnDrawGizmos()
    {
        SpriteRenderer spriteRenderer = _spriteRenderer != null ? _spriteRenderer : GetComponent<SpriteRenderer>();

        if (spriteRenderer == null || spriteRenderer.sprite == null)
        {
            return;
        }

        Bounds bounds = ComputeInsetBounds(spriteRenderer.sprite.bounds, boundsInset, boundsOffset);

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        Gizmos.matrix = Matrix4x4.identity;
    }
}
