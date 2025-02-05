using CleverTap.Notifications;
using TMPro;
using UnityEngine;
using UnityEngine.UI; 

public class WeatherController : MonoBehaviour
{
    private LocationManager locationManager;
    private WeatherAPI weatherAPI;
    [SerializeField]
    private Button displayWeatherBtn;
    public TextMeshProUGUI loadingText;
    public void Start()
    {
        locationManager = LocationManager.Instance;
        weatherAPI = GetComponent<WeatherAPI>();
        displayWeatherBtn.onClick.AddListener(FetchAndDisplayTemperature);
        PermissionsManager.Instance.OnLocationPermissionGranted += OnLocationPermissionGranted;

    }
    private void OnLocationPermissionGranted(bool granted)
    {
        if (granted)
        {
            loadingText.color = Color.white;
            loadingText.text = "Fetching Data...";
            displayWeatherBtn.interactable = false;
            weatherAPI.weatherDataReqReceived = false;
            locationManager.OnLocationUpdated += OnLocationUpdated;

        }
        else
        {
            loadingText.text = "Please allow location permission";
            loadingText.color = Color.red;
        }
    }

    private void FetchAndDisplayTemperature()
    {
        if (PermissionsManager.Instance != null)
        {
            PermissionsManager.Instance.RequestLocationPermission();
        }
        else
        {
            Debug.Log("Permission manager is null");
        }
    }

    private void OnLocationUpdated(double latitude, double longitude)
    {
        locationManager.OnLocationUpdated -= OnLocationUpdated;
        weatherAPI.FetchWeatherData(latitude, longitude);
        weatherAPI.OnWeatherDataReceived += OnWeatherDataReceived;
    }

    private void OnWeatherDataReceived(WeatherData weatherData)
    {
        loadingText.text = "";
        displayWeatherBtn.interactable = true;
        float temperature = (float)weatherData.current_weather.temperature;
        ToastNotificationManager.Instance.ShowToast($"Current temperature: {temperature}°C");
    }
    private void OnDisable()
    {
        if (locationManager != null)
            locationManager.OnLocationUpdated -= OnLocationUpdated;

        if (weatherAPI != null)
            weatherAPI.OnWeatherDataReceived -= OnWeatherDataReceived;
    }

}