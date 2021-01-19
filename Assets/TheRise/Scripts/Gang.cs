using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AwesomeUtils;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
using JoystickMovement;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions.Must;
using Random = UnityEngine.Random;

public class Gang : MonoBehaviour
{
    private DynamicJoystick _joystick;
    private CharacterController _controller;
    private GangAnimationController _animationController;
    private GangHeadTrigger _headTrigger;
    
    private List<Member> _activeMembers;

    private float _radiusMultiplier;
    private float _baseRadius;

    private bool _onMove;
    private bool _onGround;
    private bool _onPush;
    
    private bool _fallFlag;
    private Vector3 _fallStartPos;
    
    private int _downRayLayer;
    private Queue<Action> _gangAnimationQueue;

    private Transform _pushed;
    
    [SerializeField] private Transform head;
    [SerializeField] private float gangSpeed = 5;
    
    public Transform Head => head;
    public bool IsMoving => _onMove;
    //public int MemberCount => _activeMembers.Count;
    public float idleTimer = 0f;
    public List<Member> startingMembers;
    
    private void Start()
    {
        _joystick = FindObjectOfType<DynamicJoystick>();
        _controller = GetComponent<CharacterController>();
        _animationController = new GangAnimationController(this);
        _headTrigger = GetComponentInChildren<GangHeadTrigger>();    
        
        _activeMembers = new List<Member>();
        
        if (head == null)
            head = transform.GetChild(0);

        _baseRadius = transform.lossyScale.x / 2f;
        _radiusMultiplier = _baseRadius / 2f;
        
        //set new members
        foreach (var member in startingMembers)
        {
            AddMember(member);
        }
        
        _downRayLayer = 1 << LayerMask.NameToLayer("Props");
        // This would cast rays only against colliders in layer Props.
        // But instead we want to collide against everything except layer props. The ~ operator does this, it inverts a bitmask.
        _downRayLayer = ~_downRayLayer;
        
        _headTrigger.interactBarrier += InteractBarrier;
        _headTrigger.interactPushable += InteractPushable;
    }

    private void OnDisable()
    {
        _headTrigger.interactBarrier -= InteractBarrier;
        _headTrigger.interactPushable -= InteractPushable;
    }

    private void Update()
    {
        if (_onPush)
        {
           // _pushed.rotation = head.rotation;
           // _pushed.position += _pushed.forward * gangSpeed / 2f * Time.deltaTime;
        }
        
        Vector3 moveVec;
        _onMove = HandleMovement(out moveVec);
        _controller.Move(moveVec * Time.deltaTime * gangSpeed);
        if(_onPush)
            Debug.LogWarning(moveVec);
    }

    private void FixedUpdate()
    {            
        _onGround = TransformUtils.IsHitDirection(transform, Vector3.down,_baseRadius*_controller.radius*2f,_controller.height,4, _downRayLayer);
        if (!_fallFlag && !_onGround) //just started falling
        {
            _fallFlag = true;
            _fallStartPos = GetPosAroundBase(head.forward.normalized * -1f, _baseRadius);

            Debug.LogWarning($"Gang fall start on pos {_fallStartPos}");
        }
        else if (_fallFlag && _onGround) // gang falled and landed
        {
            _fallFlag = false;
            FelledGround(_fallStartPos,_activeMembers);
            
            Debug.LogWarning($"Gang fall end on pos {head.position}");
        }
    }

    bool HandleMovement(out Vector3 move)
    {
        move = new Vector3(_joystick.Horizontal,head.transform.localPosition.y * 2.5f,_joystick.Vertical);
        head.localPosition = Vector3.MoveTowards(head.localPosition,(move / 2.5f), Time.deltaTime * gangSpeed);
        head.transform.localEulerAngles = TransformUtils.RotateInputDirection(head.transform.localEulerAngles,
            _joystick.Vertical, _joystick.Horizontal);

        if (_onGround == false) 
        {
            move *= 0.2f;
            move.y = -2f;
        }
        
        if (!_joystick.Active)
        {
            idleTimer += Time.deltaTime;
            return false;
        }
        
        idleTimer = 0f;
        
        return true;
    }

    void AddMember(Member newMember)
    {
        _activeMembers.Add(newMember);
        newMember.destroyed += RemoveMember;
        newMember.AddToGang();
        ChangeSize(_radiusMultiplier);
        
        GameManager.manager.gangCountChange(_activeMembers.Count);
    }

    void RemoveMember(Member removedMember)
    {
        _activeMembers.Remove(removedMember);
        removedMember.destroyed -= RemoveMember;
        removedMember.RemoveFromGang();
        ChangeSize(_radiusMultiplier * -1);

        GameManager.manager.gangCountChange(_activeMembers.Count);
    }
    
    void InteractPushable(Transform pushable)
    {
        if (pushable != null)
        {
            _onPush = true;
            _pushed = pushable;
        }
        else
        {
            _onPush = false;
            _pushed = null;
        }
    }

    void InteractBarrier(Barrier barrier)
    {
        if(!_onGround) return;
        
        if (barrier.passed)
        {
            //move gang to up
            MoveGangToBarrierEnd(barrier);
            PassBarrier(barrier,_activeMembers);
        }
        else if (barrier.CanGangPassBarrier(_activeMembers.Count))
        {
            Debug.LogWarning("Build Pass");
            barrier.SetPass(head.transform.position);
            //move gang to up
            MoveGangToBarrierEnd(barrier);

            List<Member> passMembers = new List<Member>();
            passMembers.AddRange(_activeMembers.GetRange(0,barrier.neededManCount));

            _activeMembers.RemoveRange(0, barrier.neededManCount);
            
            BuildPass(barrier,passMembers);
            PassBarrier(barrier,_activeMembers);
        }
        else
        {
            Debug.LogWarning("Cant pass!!");
        }
    }
    
    void BuildPass(Barrier barrier, List<Member> passMembers)
    {
        GangAnimData buildAnim = new GangAnimData();
        buildAnim.type = GangAnimType.BuildBarrierPass;
        buildAnim.barrier = barrier;
            
        _animationController.AddAnimationToMembers(buildAnim,passMembers);
    }

    void PassBarrier(Barrier barrier, List<Member> activeMembers)
    {
        GangAnimData passAnim = new GangAnimData();
        passAnim.type = GangAnimType.PassBarrier;
        passAnim.barrier = barrier;
            
        _animationController.AddAnimationToMembers(passAnim,activeMembers);

    }
    
    void FelledGround(Vector3 startPos, List<Member> activeMembers)
    {
        GangAnimData fellAnim = new GangAnimData();
        fellAnim.type = GangAnimType.FelledGround;
        fellAnim.fallStartPos = startPos;
        fellAnim.fallPos = head.position;

        _animationController.AddAnimationToMembers(fellAnim,activeMembers);
    }

    void MoveGangToBarrierEnd(Barrier barrier)
    {
        if (barrier.passStartPosition == Vector3.zero)
        {
            Debug.LogError("Barrier should be set");
            return;
        }
            
        Vector3 newPos = barrier.passEndPosition;
        newPos += new Vector3(head.forward.x * _baseRadius * 2f, transform.localScale.y,head.forward.z * _baseRadius * 2f);
        Beam(newPos);
    }
    
    void Beam(Vector3 toPos)
    {
        _controller.enabled = false;
        transform.position = toPos;
        _controller.enabled = true;
    }
    
    void ChangeSize(float changeSize)
    {
        transform.localScale += new Vector3(changeSize ,0f,changeSize);
        _baseRadius = transform.localScale.x / 2f;
    }


    #region public methods

    public bool IsPosInBase(Vector3 position,float tolerance = 0f)
    {
        Vector3 myPos = transform.position;
        
        myPos.y = 0f;
        position.y = 0f;
        
        if (Vector3.Distance(myPos, position) < _baseRadius + tolerance)
            return true;
        
        return false;
    }

    public Vector3 GetPosAroundBase(Vector2 posOnUnitCircle,float distance)
    {
        posOnUnitCircle = posOnUnitCircle.normalized * distance;
        //Debug.LogError(posOnUnitCircle);
        Vector3 pos = new Vector3(transform.position.x + posOnUnitCircle.x,transform.position.y,transform.position.z + posOnUnitCircle.y);
        return pos;
    }

    #endregion
}

