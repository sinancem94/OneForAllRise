using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data.SqlTypes;
using System.Threading;

class GangAnimationController
{
   
    public Coroutine animationCoroutine;
    
    public event Action onAnimationFinish;

    private readonly Gang _myGang;
    
    public GangAnimationController(Gang gang)
    {
        _myGang = gang;
    }

    public void AddAnimationToMembers(GangAnimData data,List<Member> effectedMembers)
    {
        foreach (var member in effectedMembers)
        {
            member.EnqueueAnimation(data);
        }
    }
    
}

public struct GangAnimData
{
    public GangAnimType type;
    public Vector3 fallStartPos;
    public Vector3 fallPos;
    public Barrier barrier;
}

public enum GangAnimType
{
    BuildBarrierPass,
    PassBarrier,
    FelledGround
}

