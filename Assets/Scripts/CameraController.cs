using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform targer;
    [SerializeField] private Vector3 offset = new Vector3(8, 12, -10);
    [SerializeField] private float smoothSpeed = 0.125f;

    private void Start()
    {
        offset = new Vector3(8, 12, -10);
        smoothSpeed = 0.125f;

        transform.rotation = Quaternion.Euler(45f, -45f, 0f);
    }

    private void LateUpdate()
    {
        if (targer == null) return;
        Vector3 desired = targer.position + offset;
        Vector3 smoothedPositoon = Vector3.Lerp(transform.position, desired, smoothSpeed);
        transform.position = smoothedPositoon;


        transform.LookAt(targer);
    }
}
