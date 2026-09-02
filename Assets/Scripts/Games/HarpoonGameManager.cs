using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

public class HarpoonGameManager : MonoBehaviour, IMiniGame
{
    [SerializeField, Required] private Transform harpoon;
    [SerializeField, Required] private Transform target;

    [SerializeField] private float downDuration = 0.15f;
    [SerializeField] private float upDuration = 0.45f;
    [SerializeField] private AnimationCurve downCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve upCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [SerializeField] private Transform[] skewerSlots;
    [SerializeField] private float skewerDuration = 0.2f;
    [SerializeField] private AnimationCurve skewerCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Vector3 _restPosition;
    private Coroutine _strikeRoutine;
    private readonly List<Fish> _caughtFish = new List<Fish>();
    private int _nextSlotIndex;

    private void Awake()
    {
        _restPosition = harpoon.position;
    }

    private void Update()
    {
        if (WasPointerPressedThisFrame())
        {
            Strike();
        }
    }

    private static bool WasPointerPressedThisFrame()
    {
        return Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
    }

    public void StartGame()
    {
        Strike();
    }

    [Button("Reset", EButtonEnableMode.Playmode)]
    public void ResetGame()
    {
        StopAllCoroutines();
        _strikeRoutine = null;

        harpoon.position = _restPosition;

        foreach (Fish fish in _caughtFish)
        {
            fish.ReleaseFromHarpoon();
        }

        _caughtFish.Clear();
        _nextSlotIndex = 0;
    }

    public void CatchFish(Fish fish)
    {
        if (fish.IsCaught || _nextSlotIndex >= skewerSlots.Length)
        {
            return;
        }

        fish.Catch();
        Transform slot = skewerSlots[_nextSlotIndex];
        _nextSlotIndex++;
        _caughtFish.Add(fish);

        Debug.Log($"Caught fish: {_caughtFish.Count}/{skewerSlots.Length}");

        StartCoroutine(SkewerFish(fish, slot));
    }

    [Button("Strike", EButtonEnableMode.Playmode)]
    private void Strike()
    {
        if (_strikeRoutine != null)
        {
            return;
        }

        _strikeRoutine = StartCoroutine(StrikeRoutine());
    }

    private IEnumerator StrikeRoutine()
    {
        yield return MoveHarpoon(_restPosition, target.position, downDuration, downCurve);
        yield return MoveHarpoon(target.position, _restPosition, upDuration, upCurve);
        _strikeRoutine = null;
    }

    private IEnumerator MoveHarpoon(Vector3 from, Vector3 to, float duration, AnimationCurve curve)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = curve.Evaluate(Mathf.Clamp01(elapsed / duration));
            harpoon.position = Vector3.LerpUnclamped(from, to, t);
            yield return null;
        }

        harpoon.position = to;
    }

    private IEnumerator SkewerFish(Fish fish, Transform slot)
    {
        Vector3 from = fish.transform.position;
        float elapsed = 0f;

        while (elapsed < skewerDuration)
        {
            elapsed += Time.deltaTime;
            float t = skewerCurve.Evaluate(Mathf.Clamp01(elapsed / skewerDuration));
            fish.transform.position = Vector3.LerpUnclamped(from, slot.position, t);
            yield return null;
        }

        fish.transform.SetParent(slot);
        fish.transform.localPosition = Vector3.zero;
    }
}
