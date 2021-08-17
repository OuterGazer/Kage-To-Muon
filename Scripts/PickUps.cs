using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUps : MonoBehaviour
{
    [SerializeField] int coinValue = 10;
    [SerializeField] AudioClip pickSFX;

    private GameSession gameSession;

    // Start is called before the first frame update
    void Start()
    {
        this.gameSession = GameObject.FindObjectOfType<GameSession>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (this.gameObject.CompareTag("Coin Pickup"))
            {
                this.gameSession.AddScore(this.coinValue);
                collision.GetComponent<Health>().Heal(this.coinValue);
            }
            else if (this.gameObject.CompareTag("Shur Pickup"))
            {
                this.gameSession.ChangeShurikenAmount(++GameObject.FindObjectOfType<Player>().ShurikenAmount);
            }

            AudioSource.PlayClipAtPoint(this.pickSFX, Camera.main.transform.position);
            GameObject.Destroy(this.gameObject);
        }
    }
}
