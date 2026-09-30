using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class Pliers : MonoBehaviour
{
    [SerializeField] private float verticalOffset = 1f;
    [SerializeField] private float verticalSpeed = 1f;

    [SerializeField, Required] private Transform grabTarget;
    [SerializeField] private float reachDuration = 0.3f;
    [SerializeField] private float returnDuration = 0.4f;
    [SerializeField] private AnimationCurve reachCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve returnCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField, Range(0f, 1f)] private float releaseFraction = 0.5f;

    private bool _movingUp = true;
    private Vector3 _initialLocalPosition;
    private bool _isInitialized;
    private readonly List<PullNail> _touchingNails = new List<PullNail>();
    private Coroutine _swingRoutine;
    private Coroutine _grabRoutine;

    public event Action GrabFinished;

    public bool IsReady => _swingRoutine != null && _grabRoutine == null;

    private void OnDisable()
    {
        _swingRoutine = null;
        _grabRoutine = null;
        _touchingNails.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PullNail nail = other.GetComponentInParent<PullNail>();
        if (nail != null && !_touchingNails.Contains(nail))
        {
            _touchingNails.Add(nail);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PullNail nail = other.GetComponentInParent<PullNail>();
        if (nail != null)
        {
            _touchingNails.Remove(nail);
        }
    }

    public void StartSwinging()
    {
        EnsureInitialized();
        StopMoving();
        _swingRoutine = StartCoroutine(SwingRoutine());
    }

    public void Grab()
    {
        if (!IsReady)
        {
            return;
        }

        StopRoutine(ref _swingRoutine);
        _grabRoutine = StartCoroutine(GrabRoutine());
    }

    public void StopMoving()
    {
        StopRoutine(ref _swingRoutine);
        StopRoutine(ref _grabRoutine);
    }

    public void ResetPliers()
    {
        EnsureInitialized();
        StopMoving();
        _movingUp = true;
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

    private void StopRoutine(ref Coroutine routine)
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private IEnumerator SwingRoutine()
    {
        float bottomY = _initialLocalPosition.y - verticalOffset;
        float topY = _initialLocalPosition.y + verticalOffset;

        while (true)
        {
            float targetY = _movingUp ? topY : bottomY;

            while (Mathf.Abs(transform.localPosition.y - targetY) > 0.01f)
            {
                Vector3 position = transform.localPosition;
                position.y = Mathf.MoveTowards(position.y, targetY, verticalSpeed * Time.deltaTime);
                transform.localPosition = position;
                yield return null;
            }

            _movingUp = !_movingUp;
        }
    }

    private IEnumerator GrabRoutine()
    {
        Vector3 restPosition = transform.position;
        Vector3 reachPosition = new Vector3(grabTarget.position.x, restPosition.y, restPosition.z);

        float elapsed = 0f;
        while (elapsed < reachDuration)
        {
            elapsed += Time.deltaTime;
            float t = reachCurve.Evaluate(Mathf.Clamp01(elapsed / reachDuration));
            transform.position = Vector3.LerpUnclamped(restPosition, reachPosition, t);
            yield return null;
        }

        transform.position = reachPosition;
        yield return new WaitForFixedUpdate();

        PullNail nail = FindTouchingNail();
        Vector3 turnPosition = transform.position;
        Vector3 nailOffset = nail != null ? nail.transform.position - turnPosition : Vector3.zero;

        if (nail != null)
        {
            nail.Grab();
        }

        elapsed = 0f;
        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = returnCurve.Evaluate(Mathf.Clamp01(elapsed / returnDuration));
            transform.position = Vector3.LerpUnclamped(turnPosition, restPosition, t);

            if (nail != null)
            {
                if (t >= releaseFraction)
                {
                    nail.Drop();
                    nail = null;
                }
                else
                {
                    nail.MoveTo(transform.position + nailOffset);
                }
            }

            yield return null;
        }

        transform.position = restPosition;

        if (nail != null)
        {
            nail.Drop();
        }

        _grabRoutine = null;
        _swingRoutine = StartCoroutine(SwingRoutine());
        GrabFinished?.Invoke();
    }

    private PullNail FindTouchingNail()
    {
        foreach (PullNail nail in _touchingNails)
        {
            if (!nail.IsPulled)
            {
                return nail;
            }
        }

        return null;
    }
}
