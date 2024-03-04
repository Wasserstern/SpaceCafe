using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class VariousStuff {
    public static Vector2 RotateVector2(Vector2 v, float angle){
        float x2 = Mathf.Cos(angle) * v.x - Mathf.Sin(angle) * v.y;
        float y2 = Mathf.Sin(angle) * v.x + Mathf.Cos(angle) * v.y;

        return new Vector2(x2, y2);
    }

    public static Vector2 UpDirection(Vector2 origin){
        return (new Vector2(origin.x ,origin.y +1f) - origin).normalized;
    }
    public static Vector2 DownDirection(Vector2 origin){
        return (new Vector2(origin.x ,origin.y -1f) - origin).normalized;
    }
    public static Vector2 LeftDirection(Vector2 origin){
        return (new Vector2(origin.x -1f ,origin.y) - origin).normalized;

    }
    public static Vector2 RightDirection(Vector2 origin){
        return (new Vector2(origin.x +1f ,origin.y +1f) - origin).normalized;
    }
}