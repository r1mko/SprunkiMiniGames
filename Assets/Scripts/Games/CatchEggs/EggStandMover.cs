using System.Collections;
using UnityEngine;

public class EggStandMover : MonoBehaviour
{
    [SerializeField] private float horizontalOffset = 1f;
    [SerializeField] private float horizontalSpeed = 1f;

    private bool _movingRight = true;
    private Vector3 _initialLocalPosition;
    private Coroutine _swingRoutine;

    private void Awake()
    {
        _initialLocalPosition = transform.localPosition;
    }

    public void StartSwinging()
    {
        StopSwinging();
        _swingRoutine = StartCoroutine(SwingRoutine());
    }

    public void StopSwinging()
    {
        if (_swingRoutine != null)
        {
            StopCoroutine(_swingRoutine);
            _swingRoutine = null;
        }
    }

    public void ResetMover()
    {
        StopSwinging();
        transform.localPosition = _initialLocalPosition;
        _movingRight = true;
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
                Vector3 position = transform.localPosition;
                position.x = Mathf.MoveTowards(position.x, targetX, horizontalSpeed * Time.deltaTime);
                transform.localPosition = position;
                yield return null;
            }

            _movingRight = !_movingRight;
        }
    }
}
