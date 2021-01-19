using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GangHeadTrigger : MonoBehaviour
{
    private Gang _myGang;
    private int interactableLayer;
    private int pushableLayer;

    public Action<Barrier> interactBarrier;
    public Action<Transform> interactPushable;

    
    private void Start()
    {
        _myGang = transform.GetComponentInParent<Gang>();
        interactableLayer = LayerMask.NameToLayer("Interactable");
        pushableLayer = LayerMask.NameToLayer("Pushable");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == interactableLayer)
        {
            if(interactBarrier != null)
                interactBarrier(other.GetComponent<Barrier>());
        }
        else if (other.gameObject.layer == pushableLayer)
        {
            if (interactPushable != null)
                interactPushable(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == pushableLayer)
        {
            if (interactPushable != null)
                interactPushable(null);
        }
    }
}
