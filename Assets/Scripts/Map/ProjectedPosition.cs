using System;
using UnityEngine;

public struct ProjectedPosition
{
    public int X, Y, ZoomLevel;
    public double LocalX, LocalY;
    public ProjectedPosition(int x, int y, int zoomLevel, double localX = 0, double localY = 0)
    {
        X = x;
        Y = y;
        ZoomLevel = zoomLevel;
        LocalX = localX;
        LocalY = localY;
    }

    public ProjectedPosition(double x, double y, int zoomLevel)
    {
        X = (int)Math.Floor(x);
        Y = (int)Math.Floor(y);
        ZoomLevel = zoomLevel;
        LocalX = x - X;
        LocalY = y - Y;
    }

    public float Magnitude
    {
        get { return ((Vector2)this).magnitude; }
    }

    public static ProjectedPosition Lerp(ProjectedPosition from, ProjectedPosition to, float t)
    {
        Vector2 lerped = Vector2.Lerp(from, to, t);
        return new ProjectedPosition(lerped.x, lerped.y, from.ZoomLevel);
    }

    public static implicit operator Vector2(ProjectedPosition pos)
    {
        return new Vector2(pos.X + (float)pos.LocalX, pos.Y + (float)pos.LocalY);
    }


    public static bool operator ==(ProjectedPosition p1, ProjectedPosition p2)
    {
        return p1.X == p2.X && p1.Y == p2.Y && p1.ZoomLevel == p2.ZoomLevel;
    }

    public static bool operator !=(ProjectedPosition p1, ProjectedPosition p2)
    {
        return !(p1 == p2);
    }

    public static ProjectedPosition operator +(ProjectedPosition p1, ProjectedPosition p2)
    {
        double x = p1.X + p1.LocalX + p2.X + p2.LocalX;
        double y = p1.Y + p1.LocalY + p2.Y + p2.LocalY;
        return new ProjectedPosition(x, y, p1.ZoomLevel);
    }

    public static ProjectedPosition operator -(ProjectedPosition p1, ProjectedPosition p2)
    {
        double x = (p1.X + p1.LocalX) - (p2.X + p2.LocalX);
        double y = (p1.Y + p1.LocalY) - (p2.Y + p2.LocalY);
        return new ProjectedPosition(x, y, p1.ZoomLevel);
    }
}