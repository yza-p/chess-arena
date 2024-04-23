using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connect : MonoBehaviour
{
    public static Connect Instance { get; set; }

    private void Awake()
    {
        Instance = this;
    }

    public void OnCreateRoom()
    {
        Debug.Log("OnCreateRoom");
    }

    public void OnJoinRoom()
    {
        Debug.Log("OnJoinRoom");
    }

    public void OnCancelConnect()
    {
        Debug.Log("OnCancelConnect");
    }

}
