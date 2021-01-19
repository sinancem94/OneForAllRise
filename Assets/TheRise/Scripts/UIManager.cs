using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Transform panel;

    private Text memberCount;

    private void Start()
    {
        memberCount = GetComponentInChildren<Text>();
        GameManager.manager.gangCountChange += ChangeMemberCount;
        
    }

    void ChangeMemberCount(int count)
    {
        memberCount.text = count.ToString();
    }
}
