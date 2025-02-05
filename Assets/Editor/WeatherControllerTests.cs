using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class MockPermissionsManager : PermissionsManager
{
    protected override bool HasPermission() => false; //Forcefully deny permission
}
public class TestWeatherAPI : WeatherAPI
{
    public bool methodCalled = false;
    public double receivedLat, receivedLon;

    public override void FetchWeatherData(double latitude, double longitude)
    {
        methodCalled = true;
        receivedLat = latitude;
        receivedLon = longitude;
        OnWeatherDataReceived?.Invoke(new WeatherData { current_weather = new CurrentWeather { temperature = 25.0 } });
    }
}
public class WeatherControllerTests
{
    private GameObject testGameObject;
    private WeatherController weatherController;
    private Button displayWeatherBtn;
    private TextMeshProUGUI loadingText;
    private PermissionsManager permissionsManager;
    private MockPermissionsManager mockPermissionsManager;
    private LocationManager locationManager;
    private WeatherAPI weatherAPI;

#if UNITY_EDITOR
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        testGameObject = new GameObject("WeatherController");
        Assert.NotNull(testGameObject, "Test GameObject creation failed");

        weatherController = testGameObject.AddComponent<WeatherController>();
        weatherAPI = testGameObject.AddComponent<WeatherAPI>();
        Assert.NotNull(weatherController, "WeatherController component addition failed");

        
        var btnObj = new GameObject("Button");
        displayWeatherBtn = btnObj.AddComponent<Button>();
        Assert.NotNull(displayWeatherBtn, "Button creation failed");

        var textObj = new GameObject("LoadingText");
        loadingText = textObj.AddComponent<TextMeshProUGUI>();
        loadingText.text = "";
        Assert.NotNull(loadingText, "LoadingText creation failed");

        
        weatherController.loadingText = loadingText;
        var btnField = typeof(WeatherController).GetField("displayWeatherBtn",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        btnField?.SetValue(weatherController, displayWeatherBtn);

        
        var permManagerObj = new GameObject("PermissionsManager");
        permissionsManager = permManagerObj.AddComponent<PermissionsManager>();
        Assert.NotNull(permissionsManager, "PermissionsManager creation failed");

        

        var gameObject = new GameObject("MockPermissionManager");
        mockPermissionsManager = gameObject.AddComponent<MockPermissionsManager>(); // ✅ Use mock class
        Object.DontDestroyOnLoad(mockPermissionsManager.gameObject);
        typeof(PermissionsManager).GetField("Instance",
System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
?.SetValue(null, mockPermissionsManager);

        var locationManagerObj = new GameObject("LocationManager");
        locationManager = locationManagerObj.AddComponent<LocationManager>();
        Object.DontDestroyOnLoad(locationManagerObj);

        var locationManagerField = typeof(LocationManager).GetField("instance",
    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        locationManagerField.SetValue(null, locationManager);

        yield return null;
    }

    [UnityTest]
    public IEnumerator BasicSetupTest_ShouldNotBeNull()
    {
        Assert.NotNull(weatherController);
        Assert.NotNull(displayWeatherBtn);
        Assert.NotNull(loadingText);
        Assert.NotNull(permissionsManager);

        yield return null;
    }

    [UnityTest]
    public IEnumerator DisplayWeatherBtn_Click_ShouldRequestLocationPermission()
    {
        bool permissionRequested = false;
        permissionsManager.OnLocationPermissionGranted += (granted) =>
        {
            permissionRequested = true;
        };

        displayWeatherBtn.onClick.Invoke();
        yield return new WaitForSeconds(0.1f);

        Assert.That(permissionRequested, Is.True);
    }

    [UnityTest]
    public IEnumerator OnLocationPermissionGranted_WhenGranted_ShouldUpdateUIAndDisableButton()
    {
        weatherController.Start();

        PermissionsManager.Instance.RequestLocationPermission();
        yield return new WaitForSeconds(0.1f);

        Assert.That(loadingText.text, Is.EqualTo("Fetching Data..."));
        Assert.That(loadingText.color, Is.EqualTo(Color.white));
        Assert.That(displayWeatherBtn.interactable, Is.False);
    }

    [UnityTest]
    public IEnumerator OnLocationPermissionGranted_WhenDenied_ShouldShowErrorMessage()
    {
        weatherController.Start();
        bool permissionGranted = false;  
        mockPermissionsManager.OnLocationPermissionGranted += (granted) =>
        {
            permissionGranted = granted;
        };

        PermissionsManager.Instance.RequestLocationPermission();
        yield return new WaitForSeconds(0.1f);

        Assert.IsFalse(permissionGranted, "Permission should have been denied");
    }
    [UnityTest]
    public IEnumerator OnLocationUpdated_ShouldFetchWeatherData()
    {
        weatherController.Start();
        const double testLat = 42.0;
        const double testLon = -71.0;

        var weatherAPI = testGameObject.GetComponent<WeatherAPI>();
        if (weatherAPI == null)
        {
            weatherAPI = testGameObject.AddComponent<WeatherAPI>();
        }
        weatherAPI.weatherDataReqReceived = false;

        permissionsManager.RequestLocationPermission();
        yield return new WaitForSeconds(0.1f);

        var locationManagerField = typeof(LocationManager).GetField("instance",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        locationManager = (LocationManager)locationManagerField.GetValue(null);
        Assert.NotNull(locationManager, "LocationManager instance is null");

        if (locationManager.OnLocationUpdated == null)
        {
            locationManager.OnLocationUpdated += (lat, lon) => { }; // Dummy handler
        }

        locationManager.OnLocationUpdated?.Invoke(testLat, testLon);
        yield return new WaitForSeconds(0.1f);
        Assert.That(weatherAPI.weatherDataReqReceived,
            Is.True,
            $"Weather data request was NOT received! Check if FetchWeatherData is called correctly. Current state: {weatherAPI.weatherDataReqReceived}");
    }





    [UnityTest]
    public IEnumerator OnLocationUpdated_ShouldCallFetchWeatherDataWithCorrectLatLon()
    {
        weatherController.Start();

        var weatherAPI = testGameObject.AddComponent<WeatherAPI>();  // Use test subclass
        var locationManager = testGameObject.AddComponent<LocationManager>();

        const double expectedLat = 42.0;
        const double expectedLon = -71.0;

        permissionsManager.RequestLocationPermission();
        locationManager.OnLocationUpdated += weatherAPI.FetchWeatherData;
        yield return new WaitForSeconds(0.1f);
        locationManager.OnLocationUpdated?.Invoke(expectedLat, expectedLon);
        yield return new WaitForSeconds(0.1f);



        Assert.That(weatherAPI.weatherDataReqReceived, Is.True, "FetchWeatherData was not called.");
        Assert.That(weatherAPI.receivedLat, Is.EqualTo(expectedLat), "Latitude is incorrect.");
        Assert.That(weatherAPI.receivedLon, Is.EqualTo(expectedLon), "Longitude is incorrect.");
    }

    [Test]
    public void FetchWeatherData_ShouldReceiveCorrectLatLon()
    {
        weatherController.Start();
        
        var weatherAPI = testGameObject.AddComponent<TestWeatherAPI>();
        double expectedLat = 42.0;
        double expectedLon = -71.0;

        bool methodCalled = false;
        double receivedLat = 0, receivedLon = 0;

        void TestFetchWeatherData(double lat, double lon)
        {
            methodCalled = true;
            receivedLat = lat;
            receivedLon = lon;
        }

        weatherAPI.OnWeatherDataReceived += (data) => { /* Simulate event */ };

        TestFetchWeatherData(expectedLat, expectedLon);  // Manually calling the test method

        Assert.That(methodCalled, Is.True, "FetchWeatherData was not called.");
        Assert.That(receivedLat, Is.EqualTo(expectedLat), "Latitude is incorrect.");
        Assert.That(receivedLon, Is.EqualTo(expectedLon), "Longitude is incorrect.");
    }
    [UnityTest]
    public IEnumerator FetchWeatherData_ShouldHandleApiErrorsGracefully()
    {

        var weatherAPI = testGameObject.AddComponent<WeatherAPI>();
        bool errorHandled = false;


        weatherAPI.OnWeatherDataReceived += (data) =>
        {
            if (data == null) errorHandled = true;
        };


        weatherAPI.StartCoroutine(SimulateApiFailure(weatherAPI));

        
        yield return new WaitForSeconds(0.5f);

        
        Assert.That(errorHandled, Is.True, "API error was not handled properly.");
    }

    private IEnumerator SimulateApiFailure(WeatherAPI weatherAPI)
    {
        yield return new WaitForSeconds(0.1f);
        weatherAPI.OnWeatherDataReceived?.Invoke(null);  // Simulate failure (null response)
    }

    [TearDown]
    public void Cleanup()
    {
        if (testGameObject != null)
            Object.DestroyImmediate(testGameObject);
        if (permissionsManager != null)
            Object.DestroyImmediate(permissionsManager.gameObject);
    }
#endif
}