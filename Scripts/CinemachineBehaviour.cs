using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinemachineBehaviour : MonoBehaviour
{
    private Player player;

    private CinemachineStateDrivenCamera playerCam;

    // Start is called before the first frame update
    void Start()
    {
        this.player = GameObject.FindObjectOfType<Player>();
        this.playerCam = this.gameObject.GetComponent<CinemachineStateDrivenCamera>();

        this.playerCam.Follow = this.player.transform;
        this.playerCam.m_AnimatedTarget = this.player.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
