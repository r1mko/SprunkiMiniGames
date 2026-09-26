using System.Collections;
using UnityEngine;

public class Spinner : MonoBehaviour
{
    [SerializeField] private float speed = 90f;
    [SerializeField] private bool keepChildrenUpright = true;

    private Quaternion _initialLocalRotation;
    private Quaternion[] _childrenInitialLocalRotations;
    private bool _isInitialized;
    private Coroutine _spinRoutine;

    public void StartSpinning()
    {
        EnsureInitialized();
        StopSpinning();
        _spinRoutine = StartCoroutine(SpinRoutine());
    }

    public void StopSpinning()
    {
        if (_spinRoutine != null)
        {
            StopCoroutine(_spinRoutine);
            _spinRoutine = null;
        }
    }

    public void ResetSpinner()
    {
        EnsureInitialized();
        StopSpinning();
        transform.localRotation = _initialLocalRotation;

        for (int i = 0; i < _childrenInitialLocalRotations.Length; i++)
        {
            transform.GetChild(i).localRotation = _childrenInitialLocalRotations[i];
        }
    }

    private void EnsureInitialized()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        _initialLocalRotation = transform.localRotation;

        _childrenInitialLocalRotations = new Quaternion[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            _childrenInitialLocalRotations[i] = transform.GetChild(i).localRotation;
        }
    }

    private IEnumerator SpinRoutine()
    {
        while (true)
        {
            float angle = speed * Time.deltaTime;
            transform.Rotate(0f, 0f, angle);

            if (keepChildrenUpright)
            {
                CounterRotateChildren(angle);
            }

            yield return null;
        }
    }

    private void CounterRotateChildren(float angle)
    {
        for (int i = 0; i < _childrenInitialLocalRotations.Length; i++)
        {
            transform.GetChild(i).Rotate(0f, 0f, -angle);
        }
    }
}
