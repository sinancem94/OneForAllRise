using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKLookAT : MonoBehaviour
{
    private Transform m_lookAtTarget;
    private Animator m_anim;
    private Transform m_head;
    private float _weight = 1;
    
    private void Start()
    {
        m_anim = GetComponent<Animator>();
        m_head = m_anim.GetBoneTransform(HumanBodyBones.Head);
    }

    void OnAnimatorIK()
    {
        float distance = Vector3.Distance(m_head.position, m_lookAtTarget.position);
        m_anim.SetLookAtWeight(_weight , _weight / 2f  );
        m_anim.SetLookAtPosition(m_lookAtTarget.position);
    }

    public Transform LookAtTarget
    {
        get => m_lookAtTarget;
        set => m_lookAtTarget = value;
    }

    public float Weight
    {
        get => _weight;
        set => _weight = value;
    }
}
