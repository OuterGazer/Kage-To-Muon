using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowPlayerHead : MonoBehaviour
{
    private Player player;

    // Start is called before the first frame update
    void Start()
    {
        this.player = GameObject.FindObjectOfType<Player>();

        if (this.player.name.Contains("Ninja"))
        {
            this.gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Player Icons/NinjaHeadScaled");
        }
        else
        {
            this.gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Player Icons/KunoHeadScaled");
        }

        this.gameObject.GetComponent<Image>().SetNativeSize();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
