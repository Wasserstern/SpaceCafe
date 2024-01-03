using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class VariousStuff {
    public static Vector2 RotateVector2(Vector2 v, float angle){
        float x2 = Mathf.Cos(angle) * v.x - Mathf.Sin(angle) * v.y;
        float y2 = Mathf.Sin(angle) * v.x + Mathf.Cos(angle) * v.y;

        return new Vector2(x2, y2);
    }
}