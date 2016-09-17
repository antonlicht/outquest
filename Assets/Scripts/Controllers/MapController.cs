using UnityEngine;

public class MapController : MonoBehaviour
{
    public MapRenderer MapRenderer;
    public Transform MapRig;
    public Transform Arrow;

    void Start()
    {
        LocationService.Initialize();
    }

    void Update()
    {
        Arrow.localEulerAngles = new Vector3(90f, LocationService.Direction, 0f);
        MapRig.localEulerAngles = new Vector3(0f, -LocationService.Direction, 0f);
        var newPosition = LocationService.GetCurrentProjectedPos(MapRenderer.ZoomLevel);
        MapRenderer.CurrentPosition = newPosition;
    }
}