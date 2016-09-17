using System;
using UnityEngine;

public class LocationService : MonoBehaviour
{
    const float MOVE_SPEED = 3f;

    private static LocationService _instance;

    [SerializeField]
    private float _latitude = 52.50599f;
    [SerializeField]
    private float _longitude = 13.39407f;
    [SerializeField]
    private float _direction = 0f;

    public static void Initialize()
    {
        _instance = new GameObject("location_service").AddComponent<LocationService>();
        Input.location.Start();
        Input.compass.enabled = true;
    }

    public static Vector2 CurrentPosition
    {
        get
        {
            if (_instance == null)
            {
                throw new Exception("LocationService not initialized");
            }
            return new Vector2(_instance._longitude, _instance._latitude);
        }
    }

    public static float Direction
    {
        get
        {
            if (_instance == null)
            {
                throw new Exception("LocationService not initialized");
            }
            return _instance._direction;
        }
    }

    public static ProjectedPosition GetCurrentProjectedPos(int zoomLevel)
    {
        return MapUtils.GeographicToProjection(CurrentPosition, zoomLevel);
    }

    void Update()
    {
#if !UNITY_EDITOR
       UpdateLocationOnDevice();
#else
        UpdateLocationInEditor();
#endif
    }

    void UpdateLocationOnDevice()
    {
        if (Input.location.status != LocationServiceStatus.Running)
        {
            Debug.Log("Wrong Locations Status: "+ Input.location.status);
            return;
        }
        _longitude = Input.location.lastData.longitude;
        _latitude = Input.location.lastData.latitude;
        _direction = Mathf.LerpAngle(_direction, Input.compass.trueHeading, Time.deltaTime * MOVE_SPEED);
    }

#if UNITY_EDITOR
    void UpdateLocationInEditor()
    {
        _longitude += Input.GetAxis("Horizontal") * Time.deltaTime * 0.001f;
        _latitude += Input.GetAxis("Vertical") * Time.deltaTime * 0.001f;
        _direction += Input.mouseScrollDelta.y;
    }
#endif
}
