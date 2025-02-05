# Weather App (Unity Test Project)

🚀 **A Unity-based weather application fetching real-time weather updates using a weather API.**


## 📌 Disclaimer
This project is developed as part of a Unity SDK developer assignment and is not an official weather application. It serves as a demonstration of API integration, location handling, and UI interaction
## 📦 Installation

### **1️⃣ Clone the Repository**
- Clone this repository using Git:
  ```sh
  git clone https://github.com/Abhi-000/Weather-App-CleverTap-Assignment.git
  ```
- Open the project in **Unity 2022.3 LTS or later**.

### **2️⃣ Install Dependencies**
- **TextMeshPro** (if not installed):
  - Go to `Window > Package Manager > Unity Registry > TextMeshPro`.
  - Click `Install`.
  - If text does not render properly, go to `Window > TextMeshPro > Import TMP Essential Resources`.
- **UnityWebRequest** is used for API calls (included by default in Unity).

## 🚀 Usage
### **1️⃣ Get User's Location**
- Attach `PermissionManager.cs` to a GameObject.
- Attach `LocationManager.cs` to a GameObject.

### **2️⃣ Fetch Weather Data**
Use `GetWeatherData` coroutine in  `WeatherAPI` script, by passing latitude and longitude to get the weather details:
```csharp
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
```

### **3️⃣ Display Weather on UI**
- Attach `WeatherController.cs` to a GameObject.
- Assign a `TextMeshProUGUI` UI element to `loadingText` in the Inspector.
- Assign a `Button` element to `Display Weather Button` in the Inspector.


## 🎮 Demo Scene
📂 **Location:** `Assets/Samples~/GetTemperatureDemo.unity`

## 🛠️ Test Build
The test build features a single button that, when clicked, requests location permission. Upon granting permission, a weather API call is made, and the current temperature is displayed to the user using CleverTap Toasts. 🚀

📂 **Location:** `Builds/`

## 🛠 Project Architecture

```
📂 WeatherApp/
│── 📂 Assets/
│    ├── 📂 CleverTap/                  # SDK for displaying Toast messages in unity
│    ├── 📂 Editor/
│    │    ├── WeatherAppTests.asmdef    # Assembly definition for weather app tests
│    │    ├── WeatherControllerTests.cs # Unit tests for testing majority of functionality
│    ├── 📂 Scenes~/
│    │    ├── GetTemperatureDemo.unity  # Sample scene demonstrating the working weather system
│    ├── 📂 Scripts/
│    │    ├── WeatherApp.asmdef         # Assembly definition for weather app scripts
│    │    ├── LocationManager.cs        # Handles location-related operations
│    │    ├── PermissionsManager.cs     # Manages app permissions
│    │    ├── WeatherAPI.cs             # Communicates with weather API
│    │    ├── WeatherController.cs      # Controls weather data processing and display
│    ├── 📜 Readme.md                   # Documentation for GitHub repository
│── 📜 .gitignore                       # Specifies files and directories to be ignored by Git

```


## 🧪 **Unit Testing**  
The package includes unit tests for core functionalities to ensure stability and correctness in real world scenarios.  

### ✅ **Basic Setup Tests**  
- **`BasicSetupTest_ShouldNotBeNull`**: Ensures all essential components (WeatherController, Button, Text, PermissionsManager) are properly initialized.  

### 📌 **Permission Handling**  
- **`DisplayWeatherBtn_Click_ShouldRequestLocationPermission`**: Checks if clicking the weather button requests location permission.  
- **`OnLocationPermissionGranted_WhenGranted_ShouldUpdateUIAndDisableButton`**: Verifies that when permission is granted, UI updates and the button is disabled.  
- **`OnLocationPermissionGranted_WhenDenied_ShouldShowErrorMessage`**: Ensures proper error handling when permission is denied.  

### 🌍 **Weather Data Fetching**  
- **`OnLocationUpdated_ShouldFetchWeatherData`**: Validates that when the location updates, weather data is requested.  
- **`OnLocationUpdated_ShouldCallFetchWeatherDataWithCorrectLatLon`**: Confirms that the correct latitude and longitude are passed to the weather API.  
- **`FetchWeatherData_ShouldReceiveCorrectLatLon`**: Ensures the fetched weather data corresponds to the correct coordinates that were passed.  

### ⚠️ **Error Handling**  
- **`FetchWeatherData_ShouldHandleApiErrorsGracefully`**: Simulates API failure and verifies that errors are handled correctly.  

These tests ensure the core functionalities of **location permissions, UI updates, and weather data fetching** work as expected.


📂 **Test Location:** `Assets/Editor/WeatherControllerTests.cs`

To run tests:
- Open `Unity Test Runner` (`Window > General > Test Runner`).
- Select **Play Mode Tests**.
- Click `Run All`.


## 📋 Requirements
✅ **Unity 2022.3 LTS or later**

✅ **Android SDK (for Android builds)**



## 🤔 FAQ & Troubleshooting

🔹 **Why is my weather data not loading?**
- Ensure you have an active internet connection.
- Verify that UnityWebRequest is enabled in project settings.

🔹 **Is the timezone fetched dynamically?**
- Yes! The API call includes `timezone=auto`, ensuring the correct timezone is automatically fetched.


## 📝 License
This project is created for testing and educational purposes. Not affiliated with any official weather service.


## 💬 Questions?
For any queries, feel free to reach out! 🚀

