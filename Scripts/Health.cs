using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] float maxHealth = 100;

    [SerializeField] AudioClip hurtSFX;
    [SerializeField] AudioClip deathSFX;
    [SerializeField] AudioClip maleHurt;
    [SerializeField] AudioClip femaleHurt;
    [SerializeField] AudioClip maleDeath;
    [SerializeField] AudioClip femaleDeath;

    public float MaxHealth => this.maxHealth;
    private float charHealth;    
    public float CharHealth => this.charHealth;

    private EnemyMovement enemy;
    private GameSession gameSession;

    private void Start()
    {
        this.gameSession = GameObject.FindObjectOfType<GameSession>();

        if (!this.gameSession.IsHealthUpdated)
        {
            this.charHealth = this.maxHealth;
            this.gameSession.UpdateHealthInfo();
        }
        else
            this.charHealth = this.gameSession.PlayerHealth;

        if (this.gameObject.CompareTag("Ninja") || this.gameObject.CompareTag("Kunoichi") ||
            this.gameObject.CompareTag("Shinobi") || this.gameObject.CompareTag("Ninjutsu"))
        {
            this.enemy = this.gameObject.GetComponent<EnemyMovement>();
        }
    }

    public void DealDamage(int amount)
    {
        if(this.enemy != null)
        {
            if(!this.enemy.IsPlayerSpotted) //this enables stealth kills
                this.charHealth -= 1000;
        }

        this.charHealth -= amount;

        if (this.gameObject.CompareTag("Player"))
        {
            this.UpdateHealthUI(-amount);

            if(this.charHealth > 0)
            {
                if (this.gameObject.name.Equals("Player Ninja"))
                {
                    this.gameObject.GetComponent<Player>().AudioSource.PlayOneShot(this.maleHurt);
                }
                else
                {
                    this.gameObject.GetComponent<Player>().AudioSource.PlayOneShot(this.femaleHurt);
                }
            }
        }
        else
        {
            if (this.charHealth > 0)
                this.enemy.AudioSource.PlayOneShot(this.hurtSFX);
        }
            

        if (this.charHealth <= 0)
        {
            if (this.gameObject.CompareTag("Player") && this.gameObject.GetComponent<Player>().IsAlive)
            {
                this.gameSession.IsHealthUpdated = false;
                this.PlayDeathAnimation();
                
                if (this.gameObject.name.Equals("Player Ninja"))
                {
                    this.gameObject.GetComponent<Player>().AudioSource.PlayOneShot(this.maleDeath);
                }
                else
                {
                    this.gameObject.GetComponent<Player>().AudioSource.PlayOneShot(this.femaleDeath);
                }
            }

            if (!this.gameObject.CompareTag("Player"))
            {
                AudioSource.PlayClipAtPoint(this.deathSFX, Camera.main.transform.position);
                this.PlayDeathAnimation();
            }
        }
    }

    public void Heal(int amount)
    {
        this.charHealth += amount;

        if (this.charHealth > this.maxHealth)
            this.charHealth = this.maxHealth;

        this.UpdateHealthUI(amount);
    }

    private void UpdateHealthUI(int amount)
    {
        this.gameSession.PlayerHealth += amount;

        if (this.charHealth > this.maxHealth)
            this.gameSession.PlayerHealth = this.maxHealth;

        this.gameSession.UpdatePlayerHealthBar();
    }

    public void PlayDeathAnimation()
    {
        if (this.gameObject.CompareTag("Ninja") || this.gameObject.CompareTag("Kunoichi") || this.gameObject.CompareTag("Shinobi") || this.gameObject.CompareTag("Ninjutsu"))
            this.gameObject.GetComponent<EnemyMovement>().SetIsAlive(false);
        else if (this.gameObject.CompareTag("Player"))
            this.gameObject.GetComponent<Player>().SetIsAlive(false);
    }
}
