using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class CarCircleGameManager : MonoBehaviour, IMiniGame
{
    public MiniGameType Type => MiniGameType.CarCircle;

    [SerializeField, Required] private CarTrack track;
    [SerializeField] private CircleCar[] cars;
    [SerializeField, Required] private Image carsLeftImage;
    [SerializeField] private float carSpeed = 3f;
    [SerializeField] private float resultDelay = 0.25f;

    private int _currentCarIndex;
    private int _carsRemaining;
    private bool _canLaunch;
    private bool _hasFinished;
    private bool _won;

    private void Awake()
    {
        foreach (CircleCar car in cars)
        {
            car.EnteredCircle += OnCarEnteredCircle;
            car.Crashed += OnCarCrashed;
        }
    }

    private void OnDestroy()
    {
        foreach (CircleCar car in cars)
        {
            car.EnteredCircle -= OnCarEnteredCircle;
            car.Crashed -= OnCarCrashed;
        }
    }

    private void OnDisable()
    {
        ResetGame();
    }

    public void StartGame()
    {
        if (cars.Length > 0)
        {
            ShowCurrentCar();
        }

        StartCoroutine(ListenForClickRoutine());
    }

    [Button("Reset", EButtonEnableMode.Playmode)]
    public void ResetGame()
    {
        StopAllCoroutines();

        foreach (CircleCar car in cars)
        {
            car.ResetCar();
        }

        _currentCarIndex = 0;
        _carsRemaining = cars.Length;
        _canLaunch = false;
        _hasFinished = false;
        _won = false;

        UpdateCarsLeftImage();
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
        while (!_hasFinished && _carsRemaining > 0)
        {
            if (_canLaunch && PointerInput.WasPressedThisFrame())
            {
                LaunchCurrentCar();
            }

            yield return null;
        }
    }

    private void ShowCurrentCar()
    {
        cars[_currentCarIndex].Show();
        _canLaunch = true;
    }

    private void LaunchCurrentCar()
    {
        _canLaunch = false;
        _carsRemaining--;
        UpdateCarsLeftImage();

        cars[_currentCarIndex].Launch(track, carSpeed);
    }

    private void OnCarEnteredCircle(CircleCar car)
    {
        if (_hasFinished || car != cars[_currentCarIndex])
        {
            return;
        }

        if (_currentCarIndex + 1 >= cars.Length)
        {
            StartCoroutine(ResultRoutine());
            return;
        }

        _currentCarIndex++;
        ShowCurrentCar();
    }

    private void OnCarCrashed(CircleCar car)
    {
        if (_hasFinished)
        {
            return;
        }

        FinishGame(false);
    }

    private IEnumerator ResultRoutine()
    {
        yield return new WaitForSeconds(resultDelay);

        FinishGame(true);
    }

    private void FinishGame(bool won)
    {
        _hasFinished = true;
        _won = won;
        StopAllCoroutines();

        if (!won)
        {
            foreach (CircleCar car in cars)
            {
                car.StopDriving();
            }
        }

        CheckResult();
    }

    private void UpdateCarsLeftImage()
    {
        carsLeftImage.sprite = DigitImageHelper.Instance.GetDigitSprite(_carsRemaining);
    }
}
