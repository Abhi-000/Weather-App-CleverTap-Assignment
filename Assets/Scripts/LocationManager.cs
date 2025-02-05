using UnityEngine;
using System.Collections;
using System;
using CleverTap.Notifications;

public class LocationManager : MonoBehaviour
{
    private static LocationManager instance;
    public static LocationManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject managerObject = new GameObject("LocationManager");
                instance = managerObject.AddComponent<LocationManager>();
                DontDestroyOnLoad(managerObject);
            }
            return instance;
        }
    }

    private bool isLocationServiceStarted = false;
    public Action<double, double> OnLocationUpdated;
    

    internal void RequestLocationData()
    {
        StartLocationService();
    }    
    private void StartLocationService()
    {
        if (!isLocationServiceStarted)
        {
            isLocationServiceStarted = true;
            Input.location.Start();
            StartCoroutine(UpdateLocationCoroutine());
        }
    }

    private IEnumerator UpdateLocationCoroutine()
    {

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait <= 0)
        {
            ToastNotificationManager.Instance.ShowToast("Timed out trying to connect to location service.");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            ToastNotificationManager.Instance.ShowToast("Unable to determine device location. Location service failure.");
            yield break;
        }
        if (Input.location.status == LocationServiceStatus.Running)
        {
            OnLocationUpdated?.Invoke(Input.location.lastData.latitude, Input.location.lastData.longitude);
            StopLocationService();
        }

        //while (true)
        //{
        //    if (Input.location.status == LocationServiceStatus.Running)
        //    {
        //        OnLocationUpdated?.Invoke(Input.location.lastData.latitude, Input.location.lastData.longitude);
        //        StopLocationService();

        //    }
        //    yield return new WaitForSeconds(5); 
        //}
    }

    private void StopLocationService()
    {
        if (isLocationServiceStarted)
        {
            isLocationServiceStarted = false;
            Input.location.Stop();
        }
    }

    private void OnDestroy()
    {
        StopLocationService();
    }
}