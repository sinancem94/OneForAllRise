using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public static class MemberSequenceUtils 
{
    public static void BuildTweenSequence(TweenData[] tweens, TweenCallback onComplete = null)
    {
        Sequence mySequence = DOTween.Sequence();
        mySequence.SetAutoKill(true);

        foreach (var tweenData in tweens)
        {
            if (tweenData.join)
            {
                mySequence.Join(tweenData.CreateTween());
            }
            else
            {
                mySequence.Append(tweenData.CreateTween());
            }
        }
        
        mySequence.OnComplete(onComplete);
        mySequence.PlayForward();
    }

    public static TweenData GoToDestination(Transform sequenced, Vector3 destination,float time)
    {
        //destination.y = _member.transform.position.y;
        TweenData goToDestination = new TweenData(TweenType.Move, sequenced, destination, time);
        return goToDestination;
    }

    public static TweenData RotateTowardsDestination(Transform sequenced, Vector3 rotation, float time)
    {
        /*//destination.y = _member.transform.position.y;
        Vector3 direction = destination - sequenced.position;
        direction.y = 0f;
        Quaternion toRotation = Quaternion.LookRotation(direction);*/
        TweenData rotateTowardStartTween = new TweenData(TweenType.Rotate, sequenced,
            rotation, time);
        return rotateTowardStartTween;
    }
}

public enum MemberAnimType
{
    Locomotion,
    Climb,
    Jump,
    PickUp
}

public enum TweenType
{
    Move,
    Rotate,
    Scale,
    ColorChange
}

public struct AnimData
{
    public MemberAnimType memberAnimType;
    public float duration;

    public float[] additionalData;

    public AnimData(MemberAnimType memberAnimType, float duration, float[] additionalData = null)
    {
        this.memberAnimType = memberAnimType;
        this.duration = duration;
        this.additionalData = additionalData;
    }
}

public class TweenData
{
    

    private TweenType _tweenType;
    private Transform _tweenedObject;
    private Vector3 _target;
    private float _duration;
    
    public bool join;

    private TweenCallback _onComplete;
    
    public TweenData(TweenType tweenType, Transform tweenedObject, Vector3 target, float duration)
    {
        this._tweenType = tweenType;
        this._tweenedObject = tweenedObject;
        this._target = target;
        this._duration = duration;
        
        this.join = false;

        _onComplete = null;
    }

    public void SetOnComplete(TweenCallback oncomplete)
    {
        _onComplete = oncomplete;
    }

    public Tween CreateTween()
    {
        Tween tween = null;

        switch (_tweenType)
        {
            case TweenType.Move:
                tween = _tweenedObject.DOMove(_target, _duration).OnComplete(_onComplete);
                break;
            case TweenType.Rotate:
                tween = _tweenedObject.DORotate(_target, _duration).OnComplete(_onComplete);
                break;
            case TweenType.Scale:
                tween = _tweenedObject.DOScale(_target, _duration).OnComplete(_onComplete);
                break;
            case TweenType.ColorChange:
                Debug.LogWarning("Not implemented");
                break;
            default:
                Debug.LogError("Unknown tweenType");
                break;
        }
        
        return tween;
    }

}