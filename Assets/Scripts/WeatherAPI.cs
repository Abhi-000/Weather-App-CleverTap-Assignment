using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Collections;
using CleverTap.Notifications;

public class WeatherAPI : MonoBehaviour
{
    private const string API_ENDPOINT = "https://api.open-meteo.com/v1/forecast";

    public Action<WeatherData> OnWeatherDataReceived;
    [HideInInspector]
    public bool weatherDataReqReceived = false;
    [HideInInspector]
    public double receivedLat, receivedLon;

    public virtual void FetchWeatherData(double latitude, double longitude)
    {
        weatherDataReqReceived = true;
        receivedLat = latitude;
        receivedLon = longitude;
        StartCoroutine(GetWeatherData(latitude, longitude));
    }

    private IEnumerator GetWeatherData(double latitude, double longitude)
    {
        string url = $"{API_ENDPOINT}?latitude={latitude}&longitude={longitude}&current_weather=true&timezone=auto";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            webRequest.timeout = 10;

            var operation = webRequest.SendWebRequest();

            float startTime = Time.time;
            while (!operation.isDone)
            {
                if (Time.time - startTime > 10f) // 10 seconds total timeout
                {
                    webRequest.Abort();
                    ToastNotificationManager.Instance.ShowToast("Weather data request timed out.");
                    Debug.LogError("Weather API request timed out");
                    yield break;
                }
                yield return null;
            }

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                ToastNotificationManager.Instance.ShowToast("Failed to fetch weather data.");
                Debug.LogError($"Error fetching weather data: {webRequest.error}");
            }
            try
            {
                WeatherData weatherData = JsonConvert.DeserializeObject<WeatherData>(webRequest.downloadHandler.text);
                OnWeatherDataReceived?.Invoke(weatherData);
            }
            catch (Exception ex)
            {
                ToastNotificationManager.Instance.ShowToast("Error parsing weather data");
                Debug.LogError($"JSON Parsing Error: {ex.Message}");
            }
        }
    }
}

[System.Serializable]
public class WeatherData
{
    public double latitude;
    public double longitude;
    public double generationtime_ms;
    public int utc_offset_seconds;
    public string timezone;
    public string timezone_abbreviation;
    public double elevation;
    public CurrentWeather current_weather;
}

[System.Serializable]
public class CurrentWeather
{
    public double temperature;
    public double windspeed;
    public double winddirection;
    public double weathercode;
    public string time;
}