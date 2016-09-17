using System;
using UnityEngine;

public class MapUtils
{
    /// <summary>
    /// Forward mapping equation to project geographical coordinates to Cartesian ones.
    /// </summary>
    /// <param name="geo">Geographical Coordinates</param>
    /// <param name="zoomLevel">Intended zoom level</param>
    /// <returns>Cartesian coordinates</returns>
    public static ProjectedPosition GeographicToProjection(Vector2 geo, int zoomLevel)
    {
        double tilesPerEdge = TilesPerEdge(zoomLevel);

        //Mercator Projection:
        double x = (geo.x + 180.0) * tilesPerEdge / 360.0;
        double y = (1.0 - (Math.Log(Math.Tan(Math.PI / 4 + (geo.y * Mathf.Deg2Rad) / 2.0)) / Math.PI)) / 2.0 * tilesPerEdge;

        return new ProjectedPosition(x, y, zoomLevel);
    }

    /// <summary>
    /// Inverse mapping equation to get geographical coordinates from Cartesian coordinates
    /// </summary>
    /// <param name="proj">Cartesian coordinates</param>
    /// <returns>Geographical Coordinates</returns>
    public static Vector2 ProjectionToGeographic(ProjectedPosition proj)
    {
        double tilesPerEdge = TilesPerEdge(proj.ZoomLevel);

        //Mercator Projection:
        double longitude = (proj.X * (360 / tilesPerEdge)) - 180;
        double latitude = Mathf.Rad2Deg * (Math.Atan(Math.Sinh((1 - proj.Y * (2 / tilesPerEdge)) * Math.PI)));

        return new Vector2((float)longitude, (float)latitude);
    }

    /// <summary>
    /// Amount of tiles on each edge which represents the world.
    /// </summary>
    /// <param name="zoomLevel">A zoomlevel of 1 means that the world map consist of 2x2 tiles.</param>
    /// <returns></returns>
    public static double TilesPerEdge(int zoomLevel)
    {
        return Math.Pow(2.0, zoomLevel);
    }

    public static double DistanceInKm(Vector2 geo1, Vector2 geo2)
    {
        double lat1 = geo1.y;
        double lat2 = geo2.y;
        double lon1 = geo1.x;
        double lon2 = geo2.x;

        double R = 6371.0;                     // Radius of the earth in km
        double dLat = Mathf.Deg2Rad * (lat2 - lat1); // deg2rad below
        double dLon = Mathf.Deg2Rad * (lon2 - lon1);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(Mathf.Deg2Rad * lat1) * Math.Cos(Mathf.Deg2Rad * lat2) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var d = R * c; // Distance in km

        return d;
    }

    public static double Distance(Vector2 geo1, Vector2 geo2)
    {
        return (geo2 - geo1).magnitude;
    }
}
