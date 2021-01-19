using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKArm : MonoBehaviour
{
    private Transform m_rightHandTarget;
    private Transform m_leftHandTarget;
    private Animator m_anim;
    private Transform m_rightHand;
    private Transform m_leftHand;

    private Transform m_handMovableArea;
    private float m_handMovableAreaRadius;

    private float m_handSpeed;
    
    private void Start()
    {
        m_anim = GetComponent<Animator>();
        m_rightHand = m_anim.GetBoneTransform(HumanBodyBones.RightHand);
        m_leftHand = m_anim.GetBoneTransform(HumanBodyBones.LeftHand);
    }
    
    void OnAnimatorIK()
    {
       SetArmIK();
    }
    
    public Transform RightHandTarget
    {
        get => m_rightHandTarget;
        set => m_rightHandTarget = value;
    }

    public Transform LeftHandTarget
    {
        get => m_leftHandTarget;
        set => m_leftHandTarget = value;
    }

    public Transform HandMovableArea
    {
        get => m_handMovableArea;
        set => m_handMovableArea = value;
    }

    public float HandMovableAreaRadius
    {
        get => m_handMovableAreaRadius;
        set => m_handMovableAreaRadius = value;
    }

    public float HandSpeed
    {
        get => m_handSpeed;
        set => m_handSpeed = value;
    }

    void SetArmIK()
    {
        if (m_rightHandTarget)
        {
            m_anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            m_anim.SetIKRotationWeight(AvatarIKGoal.RightHand,  1 );

            m_anim.SetIKPosition(AvatarIKGoal.RightHand, m_rightHandTarget.position);
            m_anim.SetIKRotation(AvatarIKGoal.RightHand, m_rightHandTarget.rotation);
            
        }
        
        if(m_leftHandTarget)
        {
            m_anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            m_anim.SetIKRotationWeight(AvatarIKGoal.LeftHand,  1 );

            m_anim.SetIKPosition(AvatarIKGoal.LeftHand, m_leftHandTarget.position);
            m_anim.SetIKRotation(AvatarIKGoal.RightHand, m_leftHandTarget.rotation);
        }
    }
    
    /*void UpdateRightHandTargetPos()
    {
        if (m_inputManager.Aiming)
        {
            Vector3 offset = (transform.up * m_inputManager.YLookAxis) + (transform.right * m_inputManager.XLookAxis);
            //offset += m_rightHandTarget.localPosition;
            Vector3 projectedPoint = m_rightHandTarget.position + offset; // (offset - m_rightHand.localPosition).normalized / 20f + m_rightHand.localPosition;
 
            m_rightHandTarget.transform.position = Vector3.Lerp(m_rightHandTarget.position, projectedPoint,
                Time.smoothDeltaTime * m_handSpeed);
            
            Vector3 centerPosition = m_handMovableArea.position; //center of *black circle*
            float distance = Vector3.Distance(m_rightHandTarget.position, centerPosition); //distance from ~green object~ to *black circle*

            if (distance > m_handMovableAreaRadius) //If the distance is less than the radius, it is already within the circle.
            {
                Vector3 fromOriginToObject = m_rightHandTarget.position - centerPosition; //~GreenPosition~ - *BlackCenter*
                fromOriginToObject *= m_handMovableAreaRadius / distance; //Multiply by radius //Divide by Distance
                m_rightHandTarget.position = centerPosition + fromOriginToObject; //*BlackCenter* + all that Math
            }
        }
    }*/
}
