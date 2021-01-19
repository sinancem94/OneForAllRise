using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class MemberAnimationController
{
    private Member _mem;
    private Animator _anim;
    //for fast access animation
    private int _animForward = Animator.StringToHash("Forward");
    private int _animSideway = Animator.StringToHash("Sideway");
    private int _animSpeed = Animator.StringToHash("Speed");
    private int _animClimb = Animator.StringToHash("Climbing");
    private int _animJump = Animator.StringToHash("Jump");
    private int _animPickpup = Animator.StringToHash("Pickup");
    
    private float _forwardValue = 0f;
    private float _sidewayValue = 0f;
    private float _speedValue = 0f;

    public MemberAnimationController(Member owner)
    {
        _mem = owner;
        _anim = owner.GetComponent<Animator>();
    }

    public void Deactivate()
    {
        Climb(false);
        
        _anim.enabled = false;
    }
    
    public void SetLocomotion(Vector2 delta, float speed,bool snap = false)
    {
//       Debug.LogError(delta.magnitude + " normasl " + delta.normalized + " delta normal mag " + delta.normalized.magnitude);
        if (delta.magnitude > 0.01f)
            _forwardValue = snap ? 1f : Mathf.Lerp(_forwardValue, 1f,0.1f);
        else
            _forwardValue = snap ? 0f : Mathf.Lerp(_forwardValue, 0f,0.1f);
        
        _speedValue = speed;

        _anim.SetFloat(_animForward, _forwardValue);
        _anim.SetFloat(_animSideway, _sidewayValue);
        _anim.SetFloat(_animSpeed,_speedValue);
    }
    
    void Jump()
    {
        _anim.SetTrigger(_animJump);
    }

    void Climb(bool val)
    {
        _anim.SetBool(_animClimb,val); 
    }
    
    IEnumerator DoAnimSequence(params AnimData[] animDatas)
    {
        foreach (var animData in animDatas)
        {
            switch (animData.memberAnimType)
            {
                case MemberAnimType.Locomotion:
                    yield return new WaitForSeconds(animData.duration);
                    break;
                case MemberAnimType.Climb:
                    Climb(true);
                    yield return new WaitForSeconds(animData.duration);
                    Climb(false);
                    break;
                case MemberAnimType.Jump:
                    Jump();
                    break;
                case MemberAnimType.PickUp:
                default:
                    break;
            }
        }
    }
    
    #region GangAnimations

    public void MemberJump(Vector3 fellStarPos, Action reachedJumpFrom)
    {
        fellStarPos.y = _mem.transform.position.y;
        float goStartTime = Mathf.Clamp(Vector3.Distance(_anim.transform.position, fellStarPos), 0.5f, 0.7f);
        
        TweenData goTowardJumpPos = MemberSequenceUtils.GoToDestination(_anim.transform, fellStarPos, goStartTime);
        //goTowardJumpPos.SetOnComplete(reachedJumpFrom);
        goTowardJumpPos.join = true;
        
        //_mem.transform.LookAt(fellStarPos);
        Quaternion targetRot = Quaternion.LookRotation(fellStarPos);
        TweenData rotateToJumpPos = MemberSequenceUtils.RotateTowardsDestination(_mem.transform,targetRot.eulerAngles, goStartTime / 2f);
        rotateToJumpPos.join = true;

        MemberSequenceUtils.BuildTweenSequence(new []{goTowardJumpPos,rotateToJumpPos},(() => reachedJumpFrom()));
        
        AnimData walkAnim = new AnimData(MemberAnimType.Locomotion, goStartTime);
        AnimData jumpAnim = new AnimData(MemberAnimType.Jump,0f);
        _mem.StartCoroutine(DoAnimSequence(walkAnim,jumpAnim));
    }
    
    public void BeBarrierPass(Barrier barrier, Action reachEnd)//, AnimData[] animDatas, TweenData[] tweens)
    {
        float goStartTime = Mathf.Clamp(Vector3.Distance(_mem.transform.position, barrier.passStartPosition), 0.5f, 0.7f);
        float goEndTime = (barrier.currentStepCount + 1) / 5f;
        Vector3 endPosition;

        TweenData[] passTweens =
            InteractBarrierBaseTweens(barrier, barrier.currentStepCount - 1, out endPosition , goStartTime , goEndTime);
        
        AnimData walkAnim = new AnimData(MemberAnimType.Locomotion, goStartTime);
        AnimData passAnim = new AnimData(MemberAnimType.Climb, goEndTime);
        
        MemberSequenceUtils.BuildTweenSequence(passTweens,(() => reachEnd()));
        
        _mem.StartCoroutine(DoAnimSequence(walkAnim,passAnim));
    }

    public void PassBarrier(Barrier barrier, Action reachEnd)
    {
        float goStartTime = Mathf.Clamp(Vector3.Distance(_mem.transform.position, barrier.passStartPosition), 0.5f, 1f);
        float goEndTime = (barrier.neededManCount + 1) / 5f;
        Vector3 endPosition;
        
        TweenData[] passTweens =
            InteractBarrierBaseTweens(barrier, barrier.neededManCount - 1, out endPosition , goStartTime , goEndTime,(() => barrier.UnlockBarrier()));
        
        //add an extra tween when passing barrier
        //Debug.Log($"current end position {endPosition} ");
        endPosition += -barrier.transform.forward * 2f;// * Mathf.Abs(_myGang.Head.position.z - endPosition.z) / 2f;
        endPosition.y = barrier.passEndPosition.y;
        TweenData goTowardGangHead = MemberSequenceUtils.GoToDestination(_mem.transform, endPosition, 0.1f);
        //Debug.Log($"after end position {endPosition} ");
        passTweens = passTweens.Append(goTowardGangHead).ToArray();

        AnimData walkAnim = new AnimData(MemberAnimType.Locomotion, goStartTime);
        AnimData passAnim = new AnimData(MemberAnimType.Climb, goEndTime);
        
        MemberSequenceUtils.BuildTweenSequence(passTweens, (() => reachEnd()));
        
        _mem.StartCoroutine(DoAnimSequence(walkAnim,passAnim));
    }

    //create 3 tweens destination while rotating and start climb until 
    TweenData[] InteractBarrierBaseTweens(Barrier barrier, int memberPositionInPass, out Vector3 endPosition , float goStartTime, float goEndTime, Action reachStart = null)
    {
        endPosition = barrier.passStartPosition;
        switch (barrier)
        {
            case LadderWall wall:
                endPosition.y = _mem.transform.localScale.y + barrier.steps[memberPositionInPass].y;
                                                            
                Debug.Log($"barrier step position y { barrier.steps[memberPositionInPass]} end target is {endPosition.y} and memberPositionInPass {memberPositionInPass}");
                break;
            default: 
                break;
        }

        TweenData goToPassStart = MemberSequenceUtils.GoToDestination(_mem.transform, barrier.passStartPosition, goStartTime);
        if (reachStart != null)
            goToPassStart.SetOnComplete(() => reachStart()); //when member reached to climb point sent another one
        goToPassStart.join = true;

       // Vector3 target = MathfUtils.Clerp( transform.rotation.eulerAngles, barrier.transform.rotation.eulerAngles * -1,1);
        Quaternion targetRotation = Quaternion.LookRotation(-barrier.transform.forward, Vector3.up);
        TweenData rotateToPassStart = MemberSequenceUtils.RotateTowardsDestination(_mem.transform, targetRotation.eulerAngles,
            goStartTime / 2f);
        rotateToPassStart.join = true;

        TweenData goToPassEnd = MemberSequenceUtils.GoToDestination(_mem.transform, endPosition, goEndTime);

        return new[] {goToPassStart, rotateToPassStart, goToPassEnd};
    }

    #endregion
}
