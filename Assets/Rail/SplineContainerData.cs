using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class SplineContainerData : MonoBehaviour
{
    [SerializeField] private ScriptableSplineData splineData;

    public SplineParameters Data(int index)
    {
        if (splineData == null || index < 0 || index >= splineData.parameters.Length)
        {
            Debug.Log("Spline data at index [" + index + "] is out of bounds. Returning default values");
            return new SplineParameters();
        }

        return splineData.parameters[index];
    }
}
