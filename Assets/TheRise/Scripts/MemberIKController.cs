using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MemberIKController : MonoBehaviour
{
    [SerializeField]
    private Animator _anim;

    [SerializeField] 
    public LayerMask _ignoredLayers;
    
    [Header("Arms Settings")]
    [SerializeField]
    private Transform _rightHandTarget = default;
    [SerializeField] 
    private Transform _leftHandTarget;
    [SerializeField]
    private float _HandPosSpeed = 1f;

    Transform _leftToe;
    Transform _leftFoot;
    Transform _leftCalf;
    Transform _leftThigh;
    Transform _rightToe;
    Transform _rightFoot;
    Transform _rightCalf;
    Transform _rightThigh;
    
    
    void Start()
    {
        _leftFoot = _anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        _rightFoot = _anim.GetBoneTransform(HumanBodyBones.RightFoot);

        _leftToe = _leftFoot.GetChild(0);
        _leftCalf = _leftFoot.parent;
        _leftThigh = _leftCalf.parent;
        
        _rightToe = _rightFoot.GetChild(0);
        _rightCalf = _rightFoot.parent;
        _rightThigh = _rightCalf.parent;
        
        _ignoredLayers = ~_ignoredLayers;
        
        SetupArm();
        SetupFootIK();
    }

    private void LateUpdate()
    {
        //ArmsIKUpdate();
    }

    void SetupFootIK()
    {
        FootPlacementData leftFootData = _anim.gameObject.AddComponent<FootPlacementData>();
        FootPlacementData rightFootData = _anim.gameObject.AddComponent<FootPlacementData>();

        MecFootPlacer footIKController = _anim.gameObject.AddComponent<MecFootPlacer>();
        
        //set values
        Vector3 leftForward = _leftFoot.forward;
        Vector3 rightForward = _rightFoot.forward;

        float footOffsetDist = _leftCalf.position.y;

        float footLength = (_leftFoot.position - _leftToe.position).magnitude;
        float footHalfWidth = (Vector3.Dot(_anim.transform.right,  _leftFoot.lossyScale)) / 2f;

        //assign values
        leftFootData.mFootID = FootPlacementData.LimbID.LEFT_FOOT;
        rightFootData.mFootID = FootPlacementData.LimbID.RIGHT_FOOT;

        leftFootData.mForwardVector = Vector3.forward;//leftForward;
        rightFootData.mForwardVector = Vector3.forward;//rightForward;

        leftFootData.mFootOffsetDist = footOffsetDist;
        rightFootData.mFootOffsetDist = footOffsetDist;

        leftFootData.mFootLength = footLength;
        rightFootData.mFootLength = footLength;

        leftFootData.mFootHalfWidth = footHalfWidth;
        rightFootData.mFootHalfWidth = footHalfWidth;
        
        leftFootData.mFootHeight = 0.05f;
        rightFootData.mFootHeight = 0.05f;

        leftFootData.mExtraRayDistanceCheck = 0.0f;
        rightFootData.mExtraRayDistanceCheck = 0.0f;
        
        //only for right foot
        //rightFootData.mIKHintOffset = new Vector3(0f,-0.25f,0f);
        
        footIKController.mAdjustPelvisVertically = true;
        footIKController.mDampPelvis = true;

        footIKController.mLayersToIgnore = new[] {"Member"};
    }

    void SetupArm()
    {
        IKArm ikArm = _anim.gameObject.AddComponent<IKArm>();
        ikArm.RightHandTarget = _rightHandTarget;
        ikArm.LeftHandTarget = _leftHandTarget;
        ikArm.HandSpeed = _HandPosSpeed;
    }

    private void ArmsIKUpdate()
    {
        
    }

    private void FBBIKUpdate()
    {
        
    }
    

    private Vector3 GetPosFromAngle(Vector3 projectedPoint, float angle, Vector3 axis)
    {
        float dist = (projectedPoint - transform.position).magnitude * Mathf.Tan(angle * Mathf.Deg2Rad);
        return projectedPoint + (dist * axis);
    }
    

    private Vector3 LissajousCurve(float theta, float A, float delta, float B)
    {
        Vector3 pos = Vector3.zero;
        pos.x = Mathf.Sin(theta);
        pos.y = A * Mathf.Sin(B * theta + delta);
        return pos;
    }
}


/*
  void SetFootIK()
    {
        RaycastHit hit;
        
        if (Physics.Raycast(_leftFoot.position + Vector3.up, -Vector3.up, out hit, 3f, _ignoredLayers))
        {
            Debug.DrawRay(_leftFoot.position + Vector3.up, Vector3.down * 3f, Color.green);
            _FootTargetPos = hit.point;
            Quaternion rot = Quaternion.LookRotation(_anim.transform.forward);
            _FootTargetRot = Quaternion.FromToRotation (Vector3.up, hit.normal) * rot;
            
            _anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, _leftFoot_Weight);
            _anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot,  _leftFoot_Weight );
            
            _anim.SetIKPosition(AvatarIKGoal.LeftFoot, _FootTargetPos);
            _anim.SetIKRotation(AvatarIKGoal.LeftFoot, _FootTargetRot);

            Quaternion _kneeTargetPos;
            
            _anim.SetIKHintPosition(AvatarIKHint.LeftKnee, _FootTargetPos);
            _anim.SetIKRotation(AvatarIKGoal.LeftFoot, _FootTargetRot);
        }
        
        if (Physics.Raycast(_rightFoot.position + Vector3.up, -Vector3.up, out hit, 3f, _ignoredLayers))
        {

            _FootTargetPos = hit.point;
            Quaternion rot = Quaternion.LookRotation(_anim.transform.forward);
            _FootTargetRot = Quaternion.FromToRotation (Vector3.up, hit.normal) * rot;
            
            _anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, _rightFoot_Weight);
            _anim.SetIKRotationWeight(AvatarIKGoal.RightFoot,  _rightFoot_Weight );
            
            _anim.SetIKPosition(AvatarIKGoal.RightFoot, _FootTargetPos);
            _anim.SetIKRotation(AvatarIKGoal.RightFoot, _FootTargetRot);
        }
    }

    void SetArmIK()
    {
        if (_rightHandTarget)
        {
            RaycastHit hit;
            if (Physics.Raycast(_rightFoot.position + Vector3.up, -Vector3.up, out hit, 3f, _ignoredLayers))
            {
                Quaternion targetRot = Quaternion.FromToRotation (Vector3.right, hit.normal) * Quaternion.LookRotation(_anim.transform.forward);
                
                _anim.SetIKPositionWeight(AvatarIKGoal.RightHand, _rightFoot_Weight);
                _anim.SetIKRotationWeight(AvatarIKGoal.RightHand,  _rightFoot_Weight );

                _anim.SetIKPosition(AvatarIKGoal.RightHand, _rightHandTarget.position);
                _anim.SetIKRotation(AvatarIKGoal.RightHand, targetRot);
            }
        }
    }


*/