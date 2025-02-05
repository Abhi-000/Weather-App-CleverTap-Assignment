using UnityEngine;
using System;
using UnityEngine.Android;
using System.Collections;
using CleverTap.Notifications;
public class PermissionsManager : MonoBehaviour
{
    public static PermissionsManager Instance { get; private set; }

    public event Action<bool> OnLocationPermissionGranted;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    protected virtual bool HasPermission()
    {
        return Permission.HasUserAuthorizedPermission(Permission.FineLocation)
            && Permission.HasUserAuthorizedPermission(Permission.CoarseLocation);
    }

    public void RequestLocationPermission()
    {
        if (!HasPermission())
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            StartCoroutine(WaitForPermission());
            //OnLocationPermissionGranted?.Invoke(false);
        }
        else
        {
            OnLocationPermissionGranted?.Invoke(true);
            LocationManager.Instance.RequestLocationData();
        }
    }

    public IEnumerator WaitForPermission()
    {
        yield return new WaitForSeconds(0.5f);

        yield return new WaitUntil(() => HasPermission() || PermissionDenied());
        if (HasPermission())
        {
            OnLocationPermissionGranted?.Invoke(true);
            LocationManager.Instance.RequestLocationData();
        }
        else
        {
            OnLocationPermissionGranted?.Invoke(false);
            ShowPermissionDeniedMessage();
            
        }
    }

    private bool PermissionDenied()
    {
        return !HasPermission();
    }

    private void ShowPermissionDeniedMessage()
    {
        ToastNotificationManager.Instance.ShowToast("Location permission denied. Enable it in Settings.");
    }
}
