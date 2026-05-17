using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExternalTypeUtil
{
    public static Vector2 NewVector2(float x, float y)
    {
        return new Vector2(x, y);
    }
    
    public static Vector3 NewVector3(float x, float y, float z)
    {
        return new Vector3(x, y, z);
    }
    
    public static Vector4 NewVector4(float x, float y, float z, float w)
    {
        return new Vector4(x, y, z, w);
    }
}
