using UnityEngine;

[System.Serializable]
public class SplineParameters
{
    public float speed = 1;
    public float cameraDistance = -10;
    [Range(0.0f, 1.0f)] public float reachCamDistAt;
    public bool followRotation;
    public bool loop;
}
