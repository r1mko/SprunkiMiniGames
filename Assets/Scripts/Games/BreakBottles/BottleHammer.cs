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

    public bool IsReady => _animationRoutine == null;
    public bool IsHitColliderActive => _hitColliderRoutine != null;

    public void Strike()
    {
        if (!IsReady)
        {
            return;
        }

        EnsureInitialized();
        _animationRoutine = StartCoroutine(AnimationRoutine());
    }

    public void ResetHammer()
    {
        EnsureInitialized();
        StopRoutine(ref _animationRoutine);
        StopRoutine(ref _hitColliderRoutine);
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
        StrikeFinished?.Invoke();
    }

    private IEnumerator HitColliderRoutine()
    {
        hitCollider.enabled = true;
        Debug.Log($"[BottleHammer] {name}: hit collider '{hitCollider.name}' ON (tag '{hitCollider.tag}', isTrigger={hitCollider.isTrigger}, hasRigidbody={hitCollider.attachedRigidbody != null})", this);

        yield return new WaitForSeconds(hitColliderDuration);

        hitCollider.enabled = false;
        Debug.Log($"[BottleHammer] {name}: hit collider OFF", this);

        _hitColliderRoutine = null;
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
