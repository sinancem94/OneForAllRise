using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager manager;

    public Camera mainCam;
    
    [SerializeField] private UIManager playerUI;
    [SerializeField] private Gang playerGang;
    
    [SerializeField] private List<GameObject> possibleMembers;
    [SerializeField] private int startSize = 5;

    public Action<int> gangCountChange;
    
    private void Awake()
    {
        if(manager)
            Debug.LogError("manager exist");
        else
            manager = this;
        
        
        if (!playerUI || !playerGang || possibleMembers.Count == 0)
        {
            Debug.LogError("GameManager set error!!");
            Application.Quit();
        }

        playerGang.startingMembers = CreateStartingMembers(startSize);
    }

    private void Start()
    {
        gangCountChange(startSize);
    }

    List<Member> CreateStartingMembers(int memberCount)
    {
        List<Member> initialMembers = new List<Member>(memberCount);
        
        for (int i = 0; i < memberCount; i++)
        {
            int newMemIndex = UnityEngine.Random.Range(0, possibleMembers.Count);
            Vector3 startPos = playerGang.GetPosAroundBase(Vector2.down,5f);
            startPos.y = possibleMembers[newMemIndex].transform.localScale.y;
            GameObject newMember = GameObject.Instantiate(possibleMembers[newMemIndex], startPos,
                Quaternion.identity, playerGang.transform.parent);
            
            initialMembers.Add(newMember.GetComponent<Member>());
        }

        return initialMembers;
    }
}
