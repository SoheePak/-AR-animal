using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Just a serializable version of a Vector3
/// Useful for the file system
/// </summary>

[System.Serializable]
public struct Vector3Data
{
    public float x;
    public float y;
    public float z;

    public Vector3Data(float iX, float iY, float iZ)
    {
        x = iX;
        y = iY;
        z = iZ;
    }

    public override string ToString()
    {
        return String.Format("[{0}, {1}, {2}]", x, y, z);
    }

    //Convert to real vector
    public static implicit operator Vector3(Vector3Data rValue)
    {
        return new Vector3(rValue.x, rValue.y, rValue.z);
    }

    public static implicit operator Vector3Data(Vector3 rValue)
    {
        return new Vector3Data(rValue.x, rValue.y, rValue.z);
    }
}