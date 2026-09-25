using System.Collections;
using UnityEngine;

public class PingPongMover : MonoBehaviour
{
    [SerializeField] private float offset = 2f;
    [SerializeField] private float speed = 2f;

    private Vector3 _initialLocalPosition;
    private bool _isInitialized;
    private Coroutine _moveRoutine;

    public void StartMoving(float direction)
    {
        EnsureInitialized();
        StopMoving();
        _moveRoutine = StartCoroutine(MoveRoutine(Mathf.Sign(direction)));
    }

    public void StopMoving()
    {
        if (_moveRoutine != null)
        {
            StopCoroutine(_moveRoutine);
            _moveRoutine = null;
        }
    }

    public void ResetMover()
    {
        EnsureInitialized();
        StopMoving();
        transform.localPosition = _initialLocalPosition;
    }

    private void EnsureInitialized()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        _initialLocalPosition = transform.localPosition;
    }

    private IEnumerator MoveRoutine(float direction)
    {
        float range = Mathf.Abs(offset);
        float elapsed = 0f;

        while (true)
        {
            elapsed += Time.deltaTime;
            float x = direction * (Mathf.PingPong(elapsed * speed + range, 2f * range) - range);
            transform.localPosition = _initialLocalPosition + new Vector3(x, 0f, 0f);
            yield return null;
        }
    }
}
