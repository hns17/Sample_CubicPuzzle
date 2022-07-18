using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAgent : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float boardUnit;
    
    void Start()
    {
        targetCamera.orthographicSize = boardUnit / targetCamera.aspect;
    }

}
