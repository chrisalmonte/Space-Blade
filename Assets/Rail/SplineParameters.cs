using UnityEngine;

[System.Serializable]
public class SplineParameters
{
    public float speed = 1;
    [Range(0.0f, 1.0f)] public float reachSpeedAt;
    public float cameraSize = 5;
    [Range(0.0f, 1.0f)] public float reachCamSizeAt;
    public bool followRotation;
    public bool loop;
}
