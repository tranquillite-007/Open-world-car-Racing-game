using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraFollow : MonoBehaviour
{
    [SerializeField] private Transform PlayercarTransform;
    [SerializeField] private Transform cameraPointTransform;
    private Vector3 velocity = Vector3.zero;
 
    void FixedUpdate()
    {
        transform.LookAt(PlayercarTransform);
        transform.position = Vector3.SmoothDamp(transform.position, cameraPointTransform.position, ref velocity, 5f * Time.deltaTime);
    }
}

    private void OnDrawGizmos()
    {
        if (cameraPointTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(PlayercarTransform.position, cameraPointTransform.position);
            Gizmos.DrawSphere(cameraPointTransform.position, 0.5f);
        }
    }
}