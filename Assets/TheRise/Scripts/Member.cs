using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Threading;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using Random = System.Random;
using AwesomeUtils;

public class Member : MonoBehaviour
{
    [Range(1f, 10f)] [SerializeField] private float baseSpeed = 4f;
    [SerializeField] private float maxSpeed = 15f;
    private Gang _myGang;
    private NavMeshAgent _navigator;
    private MemberAnimationController _animationController;
    private Rigidbody _memberRb;
    private SkinnedMeshRenderer _memberRenderer;
    
    private Vector2 _memberDelta;
    private Vector2 _lastFramePos;
    
    private float _onPhysicsTime = 0f;
    private int _downRayLayer;

    private GangAnimData _processGangAnim;
    private Queue<GangAnimData> queuedAnimations;

    private bool _processing;
    private bool _jumped;
    
    public MemberState memState;
    public NavigatorState navState;
    
    public Action<Member> destroyed;

    public enum NavigatorState
    {
        noPath,
        pathValid,
        pathInvalid,
        pathJump,
        pathClimb
    }

    public enum MemberState
    {
        inactive = -1,
        onNav,
        onBuild,
        onClimb,
        onJump
    }

    private void Start()
    {
        _animationController = new MemberAnimationController(this);
        _myGang = transform.parent.GetComponentInChildren<Gang>();
        _navigator = GetComponent<NavMeshAgent>();
        _memberRb = GetComponent<Rigidbody>();
        _memberRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        _navigator.speed = baseSpeed;
        _memberDelta = Vector2.zero;
        
        _downRayLayer = 1 << LayerMask.NameToLayer("Default");

        memState = MemberState.onNav;
        navState = NavigatorState.noPath;
        
        queuedAnimations = new Queue<GangAnimData>();
        
        ClosePhysics();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void Update()
    {
        Vector3 target = _myGang.Head.position;
        _memberDelta = CalculateDelta(transform.position);
        SetNavigationState(target);
        HandleMemberState();
        switch (memState)
        {
            case MemberState.onNav:
                
                bool emptyFront = !HasAnotherMemberFront();
                _onPhysicsTime = 0f;
        
                HandleSpeed(emptyFront);
                HandleNavigation(target, emptyFront);
                    
                break;
            case MemberState.onBuild:
                
                if(_processGangAnim.barrier.CanMemberPass() == false)
                    break;
                _processGangAnim.barrier.LockBarrier();

                StopNav();
                _animationController.BeBarrierPass(_processGangAnim.barrier, 
                    (() => BuildPassEnd()));

                break;
            case MemberState.onClimb:
                
                if(_processGangAnim.barrier.CanMemberPass() == false || _processGangAnim.barrier.currentStepCount < _processGangAnim.barrier.neededManCount - 1)
                    break;
                _processGangAnim.barrier.LockBarrier();

                StopNav();
                _animationController.PassBarrier(_processGangAnim.barrier,(() => PassEnd()));
                
                break;
            case MemberState.onJump:
                
                StopNav();
                _animationController.MemberJump(_processGangAnim.fallStartPos,(() => Jump()));
                Debug.LogError("jump start");

                break;
            case MemberState.inactive:
                CheckJumped();
                break;
            default:
                break;
        }


    }

    private void OnAnimatorMove()
    {
        HandleLocAnimation();
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("Collision on layer " + LayerMask.LayerToName(other.gameObject.layer) + " and jumped " + memState + " and time passed " + _onPhysicsTime);

        if (other.gameObject.layer == LayerMask.NameToLayer("Destroyer"))
        {
            _animationController.Deactivate();
            StopNav();
            OpenPhysics();
            if(destroyed != null)
                destroyed(this);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Pushable"))
            Debug.LogError("warning in pushh member psuh mtfck");
    }

    bool HandleNavigation(Vector3 target,bool emptyFront)
    {
        if (_myGang.IsMoving == false)
        {
            bool pathNotValid = _navigator.path.status != NavMeshPathStatus.PathComplete;
            //finished if reached or non-empty front
            bool inBaseAndFinished = _myGang.IsPosInBase(transform.position, _navigator.radius) && !emptyFront;

            if (_navigator.hasPath && (inBaseAndFinished || pathNotValid || (_myGang.idleTimer > 1f && _memberRenderer.isVisible)))
            {
                _navigator.ResetPath();
                _navigator.speed = baseSpeed;
            }

            return true;
        }
        
        return SetDestination(target);
    }

    void HandleLocAnimation()
    {
        _animationController.SetLocomotion(_memberDelta, _navigator.velocity.magnitude / _navigator.speed);
    }

    void HandleSpeed(bool emptyFront)
    {
        if (_navigator.hasPath)
        {
            Vector3 dest = _navigator.destination;
            dest.y = transform.position.y;
            float dist = Vector3.Distance(transform.position, dest);

            _navigator.speed = Mathf.Clamp(baseSpeed + dist,baseSpeed,maxSpeed);
        }
        else if(_navigator.speed >= baseSpeed)  //revert speed
        {
            _navigator.speed = baseSpeed;
        }
        
        //if members front is empty speed up, 
     /*   if (emptyFront && _navigator.speed <= speed)
            _navigator.speed += UnityEngine.Random.Range(0.5f, 1.5f);
        else if (!emptyFront && _navigator.speed >= speed) //revert speed
            _navigator.speed = speed;*/
    }

    void HandleMemberState()
    {
        switch (navState)
        {
            case NavigatorState.pathValid:
                if (memState == MemberState.onBuild)//if onbuild dont break climb
                    break;

                _processing = false;
                queuedAnimations.Clear();
                memState = MemberState.onNav;
                break;
            case NavigatorState.pathClimb:
                if(_processing || queuedAnimations.Count <= 0 || memState == MemberState.inactive)
                    break;
                
                _processGangAnim = queuedAnimations.Dequeue();
                if (_processGangAnim.type == GangAnimType.PassBarrier)
                {
                    _processing = true;
                    memState = MemberState.onClimb;
                }
                else if (_processGangAnim.type == GangAnimType.BuildBarrierPass)
                {
                    _processing = true;
                    memState = MemberState.onBuild;
                }
                break;
            case NavigatorState.pathJump:
                if(_processing || queuedAnimations.Count <= 0 || memState == MemberState.inactive)
                    break;
                
                _processGangAnim = queuedAnimations.Dequeue();
                if (_processGangAnim.type == GangAnimType.FelledGround)
                {
                    _processing = true;
                    memState = MemberState.onJump;
                }
                break;
            case NavigatorState.pathInvalid:
                break;
            case NavigatorState.noPath:
            default:
                break;
        }
    }

    void SetNavigationState(Vector3 destination)
    {
        if(memState == MemberState.inactive)
            return;
        NavMeshPath path = new NavMeshPath();
        navState = NavigatorState.noPath;

        if (_navigator.CalculatePath(destination, path))
        {
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                navState = NavigatorState.pathValid;
                return;
            }
            else
            {
                navState = NavigatorState.pathInvalid;
                float diff = transform.position.y - destination.y;
                if (Mathf.Abs(diff) > _navigator.height / 2f)
                {
                    if (diff > 0) //member is on high ground jump to gangs plane
                        navState = NavigatorState.pathJump;
                    else
                        navState = NavigatorState.pathClimb;
                }
            }
        }
    }
    bool SetDestination(Vector3 destination)
    {
        return _navigator.SetDestination(destination);
    }
    
    bool HasAnotherMemberFront()
    {
        Vector3 rayStartPos = transform.position;
        rayStartPos.y += transform.localScale.y / 2f;
        
        int memberLayer = 1 << LayerMask.NameToLayer("Member");
        float rayLength = _navigator.radius * 2f;
        
        return TransformUtils.IsHitDirection(transform,transform.forward,0f,rayLength,1,memberLayer);
    }

    void CheckJumped()
    {
        if (_jumped)
        {
            _onPhysicsTime += Time.deltaTime;
            if (_onPhysicsTime > 0.25f && TransformUtils.IsHitDirection(transform, Vector3.down,
                _navigator.radius, _navigator.height, 4, _downRayLayer))
            {
                _processing = false;
                _jumped = false;
                ClosePhysics();
                StartNav();
            }
        }
    }
    

    Vector2 CalculateDelta(Vector3 pos)
    {
        Vector2 posIn2 = new Vector2(pos.x, pos.z);
        _memberDelta = _lastFramePos - posIn2;
        _lastFramePos = posIn2;

        return _memberDelta;
    }

    void OpenPhysics()
    {
        _memberRb.isKinematic = false;
        _memberRb.useGravity = true;
        
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    void ClosePhysics()
    {
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        
        _memberRb.isKinematic = true;
        _memberRb.useGravity = false;
    }

    void StopNav()
    {
        memState = MemberState.inactive;
        if (_navigator.hasPath)
            _navigator.ResetPath();
        
        _navigator.enabled = false;
    }

    void StartNav()
    {
        _navigator.enabled = true;
        memState = MemberState.onNav;
    }

    void BuildPassEnd()
    {
        _animationController.Deactivate();
        
        _processGangAnim.barrier.SetNewStep(GetComponentInChildren<StepTag>().transform.position);
        _processGangAnim.barrier.UnlockBarrier();
        destroyed(this);
    }

    void PassEnd()
    {
        _processing = false;
        StartNav();
    }

    void Jump()
    {
        OpenPhysics();

        Vector3 force = transform.forward * 2f;
        force.y = _navigator.height;
        _memberRb.AddForce(force,ForceMode.Impulse);
        _jumped = true;
    }
    
    #region public

    public void EnqueueAnimation(GangAnimData data)
    {
        queuedAnimations.Enqueue(data);
    }
    
    public void AddToGang()
    {
        if (transform.parent != _myGang.transform.parent)
        {
            transform.parent = _myGang.transform.parent;
        }
        
        StartNav();
    }
    
    public void RemoveFromGang()
    {
        StopNav();
        this.transform.parent = null;
        //this.enabled = false;
    }

    #endregion
}


