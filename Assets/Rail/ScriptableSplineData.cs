using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SplineData", menuName = "Scriptable Objects/Spline Data", order = 1)]
public class ScriptableSplineData : ScriptableObject
{
    public SplineParameters[] parameters;
}
