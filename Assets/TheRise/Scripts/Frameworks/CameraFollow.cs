using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform objectFollowedByCam;
    [Range(1f,10f)]
    public float followSpeed;
    private Vector3 _offset;

    private void Start()
    {
        _offset = transform.position - objectFollowedByCam.position;
    }

    void Update()
    {
        Vector3 targetPos = objectFollowedByCam.position + _offset;
        //Calculate the delta
        float delta = Mathf.Abs(Vector3.Distance(transform.localPosition, targetPos)) * followSpeed / 100f;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, delta);
    }
}
