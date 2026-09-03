using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

public class Peeler : MonoBehaviour
{
    [SerializeField] private float horizontalOffset = 1f;
    [SerializeField] private float horizontalSpeed = 1f;

    [SerializeField, Required] private Transform cleanTarget;
    [SerializeField] private float cleanDuration = 0.3f;
    [SerializeField] private AnimationCurve cleanCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [SerializeField, Required] private OrangePeelMask peelMask;
    [SerializeField, Required] private CleanOrangeGameManager gameManager;
    [SerializeField] private Vector2 eraseSize = new Vector2(0.3f, 0.3f);
    [SerializeField] private Vector2 eraseOffset;

    private bool _movingRight = true;

    public void StartSwinging()
    {
        StartCoroutine(MainRoutine());
    }

    private IEnumerator MainRoutine()
    {
        while (true)
        {
            yield return SwingRoutine();
            yield return CleanRoutine();
        }
    }

    private IEnumerator SwingRoutine()
    {
        float leftX = -horizontalOffset;
        float rightX = horizontalOffset;

        while (true)
        {
            float targetX = _movingRight ? rightX : leftX;

            while (Mathf.Abs(transform.localPosition.x - targetX) > 0.01f)
            {
                if (WasPointerPressedThisFrame() && gameManager.CanClean)
                {
                    yield break;
                }

                Vector3 position = transform.localPosition;
                position.x = Mathf.MoveTowards(position.x, targetX, horizontalSpeed * Time.deltaTime);
                transform.localPosition = position;
                yield return null;
            }

            _movingRight = !_movingRight;
        }
    }

    private IEnumerator CleanRoutine()
    {
        gameManager.OnPeelerClicked();

        Vector3 restPosition = transform.position;
        Vector3 topPosition = new Vector3(restPosition.x, cleanTarget.position.y, restPosition.z);

        yield return MoveTo(restPosition, topPosition, erase: true);
        yield return MoveTo(topPosition, restPosition, erase: false);

        gameManager.OnPeelerCleaned();
    }

    private IEnumerator MoveTo(Vector3 from, Vector3 to, bool erase)
    {
        float elapsed = 0f;

        while (elapsed < cleanDuration)
        {
            elapsed += Time.deltaTime;
            float t = cleanCurve.Evaluate(Mathf.Clamp01(elapsed / cleanDuration));
            transform.position = Vector3.LerpUnclamped(from, to, t);

            if (erase)
            {
                peelMask.EraseAt(ErasePosition(), eraseSize);
            }

            yield return null;
        }

        transform.position = to;

        if (erase)
        {
            peelMask.EraseAt(ErasePosition(), eraseSize);
        }
    }

    private Vector3 ErasePosition()
    {
        return transform.position + (Vector3)eraseOffset;
    }

    private static bool WasPointerPressedThisFrame()
    {
        return Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(ErasePosition(), new Vector3(eraseSize.x, eraseSize.y, 0f));
    }
}
