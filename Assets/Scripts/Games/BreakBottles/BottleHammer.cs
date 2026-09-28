using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

public class BottleHammer : MonoBehaviour
{
    [SerializeField, Required, FormerlySerializedAs("hitTrigger")] private Collider2D hitCollider;

    [Header("Animation")]
    [SerializeField] private Vector3 hitRotation = new Vector3(0f, 0f, -90f);
    [SerializeField] private float swingDuration = 0.15f;
    [SerializeField] private float holdDuration = 0.1f;
    [SerializeField] private float returnDuration = 0.25f;

    [Header("Hit Collider")]
    [SerializeField, FormerlySerializedAs("hitActiveDuration")] private float hitColliderDuration = 0.1f;

    private Quaternion _initialLocalRotation;
    private bool _isInitialized;
    private Coroutine _animationRoutine;
    private Coroutine _hitColliderRoutine;

    public event Action StrikeFinished;

    public bool IsReady => !_isSwinging;
    public bool IsHitColliderActive => _isHitColliderActive;

    private bool _isSwinging;
    private bool _isHitColliderActive;

    private void OnDisable()
    {
        _isSwinging = false;
        _isHitColliderActive = false;
        _animationRoutine = null;
        _hitColliderRoutine = null;
    }

    public void Strike()
    {
        if (!IsReady)
        {
            return;
        }

        EnsureInitialized();
        _isSwinging = true;
        _animationRoutine = StartCoroutine(AnimationRoutine());
    }

    public void ResetHammer()
    {
        EnsureInitialized();
        StopRoutine(ref _animationRoutine);
        StopRoutine(ref _hitColliderRoutine);
        _isSwinging = false;
        _isHitColliderActive = false;
        hitCollider.enabled = false;
        transform.localRotation = _initialLocalRotation;
    }

    private void EnsureInitialized()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        _initialLocalRotation = transform.localRotation;
    }

    private void StopRoutine(ref Coroutine routine)
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private IEnumerator AnimationRoutine()
    {
        Quaternion targetRotation = Quaternion.Euler(hitRotation);

        yield return RotateRoutine(_initialLocalRotation, targetRotation, swingDuration);

        StopRoutine(ref _hitColliderRoutine);
        _hitColliderRoutine = StartCoroutine(HitColliderRoutine());

        yield return new WaitForSeconds(holdDuration);

        yield return RotateRoutine(targetRotation, _initialLocalRotation, returnDuration);

        _animationRoutine = null;
        _isSwinging = false;
        StrikeFinished?.Invoke();
    }

    private IEnumerator HitColliderRoutine()
    {
        _isHitColliderActive = true;
        hitCollider.enabled = true;

        yield return new WaitForSeconds(hitColliderDuration);

        hitCollider.enabled = false;

        _hitColliderRoutine = null;
        _isHitColliderActive = false;
    }

    private IEnumerator RotateRoutine(Quaternion from, Quaternion to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        transform.localRotation = to;
    }
}
