using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSelection : MonoBehaviour
{
    private string player;
    public string Player => this.player;

    public void SetPlayer(string name)
    {
        this.player = name;
    }

    private void Awake()
    {
        GameObject.DontDestroyOnLoad(this.gameObject);
    }
}
