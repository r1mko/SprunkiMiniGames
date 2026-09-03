using System.Collections;
using UnityEngine;

public class Fish : MonoBehaviour
{
    [SerializeField] private float xOffset = 1f;
    [SerializeField] private float swimSpeed = 1f;
    [SerializeField] private bool startMovingRight;

    public bool IsCaught { get; private set; }

    private Transform _initialParent;
    private Vector3 _initialLocalPosition;
    private float _initialAbsScaleX;
    private Coroutine _swimRoutine;

    private void Awake()
    {
        _initialParent = transform.parent;
        _initialLocalPosition = transform.localPosition;
        _initialAbsScaleX = Mathf.Abs(transform.localScale.x);
    }

    private void Start()
    {
        _swimRoutine = StartCoroutine(SwimRoutine());
    }

    public void Catch()
    {
        IsCaught = true;

        if (_swimRoutine != null)
        {
            StopCoroutine(_swimRoutine);
            _swimRoutine = null;
        }
    }

    public void ReleaseFromHarpoon()
    {
        transform.SetParent(_initialParent);
        transform.localPosition = _initialLocalPosition;
        IsCaught = false;

        _swimRoutine = StartCoroutine(SwimRoutine());
    }

    private IEnumerator SwimRoutine()
    {
        float leftX = -xOffset;
        float rightX = xOffset;
        bool movingRight = startMovingRight;
        SetFacing(movingRight);

        while (true)
        {
            float targetX = movingRight ? rightX : leftX;

            while (Mathf.Abs(transform.localPosition.x - targetX) > 0.01f)
            {
                Vector3 position = transform.localPosition;
                position.x = Mathf.MoveTowards(position.x, targetX, swimSpeed * Time.deltaTime);
                transform.localPosition = position;
                yield return null;
            }

            movingRight = !movingRight;
            SetFacing(movingRight);
        }
    }

    private void SetFacing(bool movingRight)
    {
        Vector3 scale = transform.localScale;
        scale.x = movingRight ? -_initialAbsScaleX : _initialAbsScaleX;
        transform.localScale = scale;
    }
}
