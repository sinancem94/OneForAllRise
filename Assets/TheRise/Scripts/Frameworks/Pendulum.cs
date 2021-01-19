using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pendulum : MonoBehaviour
{
    [SerializeField]
    Vector3 fromRot;
    [SerializeField]
    Vector3 toRot;

    [SerializeField] private float frequency;
    [SerializeField] private float startTime;
    
    float radiance; 

    void Start()
    {
        transform.eulerAngles = fromRot;
        radiance = 0.5f; //Mathf.Abs(to.x + from.x) / 360f;
    }

    void LateUpdate()
    {
        float t = pulse(Time.time + startTime,radiance);

        transform.eulerAngles = Vector3.Lerp(toRot, fromRot, t);
    }

    //pulse between 0 and 1. for smooth movement of wrecking ball
    float pulse(float time,float rad)
    { 
        return rad * (1 + Mathf.Sin(2 * Mathf.PI * frequency * time));
    }

}
