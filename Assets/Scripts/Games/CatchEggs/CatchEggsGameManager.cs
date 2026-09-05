using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CatchEggsGameManager : MonoBehaviour, IMiniGame
{
    private const int TotalClicks = 3;

    public MiniGameType Type => MiniGameType.CatchEggs;

    [SerializeField, Required] private EggStandMover mover;
    [SerializeField, Required] private GameObject secondStand;
    [SerializeField, Required] private GameObject thirdStand;
    [SerializeField, Required] private Image attemptsImage;

    [SerializeField] private Rigidbody2D[] eggs;
    [SerializeField] private Collider2D[] standZones;
    [SerializeField, Required] private Collider2D obstacle;
    [SerializeField] private float restVelocityThreshold = 0.05f;
    [SerializeField] private float minFallTime = 0.3f;
    [SerializeField] private float stuckFallbackDelay = 5f;

    private Collider2D[] _eggColliders;
    private Vector2[] _initialEggPositions;
    private Quaternion[] _initialEggRotations;
    private int _clicksRemaining;
    private Coroutine _inputRoutine;
    private Coroutine _watchRoutine;
    private bool _won;

    private void Awake()
    {
        _eggColliders = new Collider2D[eggs.Length];
        _initialEggPositions = new Vector2[eggs.Length];
        _initialEggRotations = new Quaternion[eggs.Length];

        for (int i = 0; i < eggs.Length; i++)
        {
            _eggColliders[i] = eggs[i].GetComponent<Collider2D>();
            _initialEggPositions[i] = eggs[i].position;
            _initialEggRotations[i] = eggs[i].transform.rotation;
        }
    }

    private void OnDisable()
    {
        ResetGame();
    }

    public void StartGame()
    {
        mover.StartSwinging();
        _inputRoutine = StartCoroutine(ListenForClickRoutine());
    }

    public void ResetGame()
    {
        if (_inputRoutine != null)
        {
            StopCoroutine(_inputRoutine);
            _inputRoutine = null;
        }

        if (_watchRoutine != null)
        {
            StopCoroutine(_watchRoutine);
            _watchRoutine = null;
        }

        mover.ResetMover();

        secondStand.SetActive(false);
        thirdStand.SetActive(false);

        for (int i = 0; i < eggs.Length; i++)
        {
            Rigidbody2D egg = eggs[i];
            egg.gameObject.SetActive(false);
            egg.gameObject.SetActive(true);

            egg.linearVelocity = Vector2.zero;
            egg.angularVelocity = 0f;
            egg.gravityScale = 0f;
            egg.position = _initialEggPositions[i];
            egg.transform.rotation = _initialEggRotations[i];

        }

        _clicksRemaining = TotalClicks;
        UpdateAttemptsImage();
    }

    public void CheckResult()
    {
        if (_won)
        {
            UIManager.Instance.ShowNextScreen();
        }
        else
        {
            UIManager.Instance.ShowRetryScreen();
        }
    }

    private IEnumerator ListenForClickRoutine()
    {
        while (_clicksRemaining > 0)
        {
            if (WasPointerPressedThisFrame())
            {
                OnClicked();
            }

            yield return null;
        }
    }

    private void OnClicked()
    {
        _clicksRemaining--;
        UpdateAttemptsImage();

        switch (_clicksRemaining)
        {
            case 2:
                PlaceStand(secondStand);
                break;
            case 1:
                PlaceStand(thirdStand);
                break;
            case 0:
                mover.StopSwinging();
                ReleaseEggs();
                break;
        }
    }

    private void PlaceStand(GameObject stand)
    {
        stand.transform.position = mover.transform.position;
        stand.SetActive(true);
    }

    private void ReleaseEggs()
    {
        foreach (Rigidbody2D egg in eggs)
        {
            egg.gravityScale = 1f;
        }

        _watchRoutine = StartCoroutine(WatchEggsRoutine());
    }

    private IEnumerator WatchEggsRoutine()
    {
        float elapsed = 0f;

        while (elapsed < stuckFallbackDelay)
        {
            for (int i = 0; i < eggs.Length; i++)
            {
                if (_eggColliders[i].IsTouching(obstacle))
                {
                    _watchRoutine = null;
                    FinishRound(false);
                    yield break;
                }
            }

            if (elapsed >= minFallTime)
            {
                bool allSettled = true;

                foreach (Rigidbody2D egg in eggs)
                {
                    if (egg.linearVelocity.sqrMagnitude > restVelocityThreshold * restVelocityThreshold)
                    {
                        allSettled = false;
                        break;
                    }
                }

                if (allSettled)
                {
                    break;
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        _watchRoutine = null;

        int standsWithEgg = 0;
        List<string> emptyStandNames = new List<string>();

        foreach (Collider2D zone in standZones)
        {
            bool hasEgg = false;

            foreach (Collider2D eggCollider in _eggColliders)
            {
                if (zone.IsTouching(eggCollider))
                {
                    hasEgg = true;
                    break;
                }
            }

            if (hasEgg)
            {
                standsWithEgg++;
            }
            else
            {
                emptyStandNames.Add(zone.name);
            }
        }

        bool allInsideZones = emptyStandNames.Count == 0;
        FinishRound(allInsideZones);
    }

    private void FinishRound(bool won)
    {
        _won = won;
        CheckResult();
    }

    private void UpdateAttemptsImage()
    {
        attemptsImage.sprite = DigitImageHelper.Instance.GetDigitSprite(_clicksRemaining);
    }

    private static bool WasPointerPressedThisFrame()
    {
        return Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
    }
}
