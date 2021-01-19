using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Barrier : MonoBehaviour
{
    public int neededManCount;
    public int currentStepCount;
    
    public Vector3 passStartPosition;
    public Vector3 passEndPosition;

    public bool passed;

    public List<Vector3> steps;
    
    private bool _onLock;
    
    private void Start()
    {
        passed = false;
        passStartPosition = Vector3.zero;
        steps = new List<Vector3>(neededManCount);
        currentStepCount = 0;
        _onLock = false;
    }

    public bool CanGangPassBarrier(int count)
    {
        return neededManCount < count;
    }

    public bool CanMemberPass()
    {
        return !_onLock;
    }
    
    public void SetPass(Vector3 startPos)
    {
        passStartPosition = startPos;
        passEndPosition = passStartPosition;// + endPoint.localPosition;//passStartPosition + (neededManCount * Vector3.up);
        passEndPosition.y += transform.localScale.y;
        steps.Insert(currentStepCount,startPos);
        currentStepCount++;
        passed = true;
    }

    public void SetNewStep(Vector3 step)
    {
        if(currentStepCount >= neededManCount)
            return;
        steps.Insert(currentStepCount,step);
        currentStepCount++;
    }

    public void LockBarrier()
    {
        _onLock = true;
    }

    public void UnlockBarrier()
    {
        _onLock = false;
    }
    
}
