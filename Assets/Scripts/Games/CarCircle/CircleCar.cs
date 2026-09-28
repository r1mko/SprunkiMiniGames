using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CircleCar : MonoBehaviour
{
    [SerializeField, Tag] private string carTag = "Car";

    private Vector3 _initialLocalPosition;
    private Quaternion _initialLocalRotation;
    private Vector3 _initialLocalScale;
    private bool _isInitialized;
    private Coroutine _driveRoutine;

    public event Action<CircleCar> EnteredCircle;
    public event Action<CircleCar> Crashed;

    private void OnDisable()
    {
        _driveRoutine = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(carTag))
        {
            Crashed?.Invoke(this);
        }
    }

    public void Show()
    {
        EnsureInitialized();
        gameObject.SetActive(true);
    }

    public void Launch(CarTrack track, float speed)
    {
        StopDriving();
        _driveRoutine = StartCoroutine(DriveRoutine(track, speed));
    }

    public void StopDriving()
    {
        if (_driveRoutine != null)
        {
            StopCoroutine(_driveRoutine);
            _driveRoutine = null;
        }
    }

    public void ResetCar()
    {
        EnsureInitialized();
        StopDriving();
        transform.localPosition = _initialLocalPosition;
        transform.localRotation = _initialLocalRotation;
        transform.localScale = _initialLocalScale;
        gameObject.SetActive(false);
    }

    private void EnsureInitialized()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        _initialLocalPosition = transform.localPosition;
        _initialLocalRotation = transform.localRotation;
        _initialLocalScale = transform.localScale;
    }

    private IEnumerator DriveRoutine(CarTrack track, float speed)
    {
        Vector3 target = WithOwnZ(track.StraightTarget);

        while (transform.position != target)
        {
            MoveTo(Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime));
            yield return null;
        }

        EnteredCircle?.Invoke(this);

        float angle = track.GetAngle(target);
        float angularSpeed = track.Direction * speed / track.Radius;

        while (true)
        {
            angle += angularSpeed * Time.deltaTime;
            MoveTo(WithOwnZ(track.GetPoint(angle)));
            yield return null;
        }
    }

    private void MoveTo(Vector3 position)
    {
        float deltaX = position.x - transform.position.x;

        if (deltaX != 0f)
        {
            Vector3 scale = _initialLocalScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(deltaX);
            transform.localScale = scale;
        }

        transform.position = position;
    }

    private Vector3 WithOwnZ(Vector3 position)
    {
        position.z = transform.position.z;
        return position;
    }
}
