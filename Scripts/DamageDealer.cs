using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [SerializeField] int weaponDamage = 50;
    [SerializeField] GameObject parrySparks;

    [SerializeField] AudioClip projectileOnGroundSFX;
    [SerializeField] AudioClip swordParrySFX;
    [SerializeField] AudioClip swordDamageSFX;
    [SerializeField] AudioClip kunaiParrySFX;
    [SerializeField] AudioClip shurikenParrySFX;
    [SerializeField] AudioClip kunaiDamageSFX;
    [SerializeField] AudioClip shurikenDamageSFX;

    private NinjutsuBehaviour parriedEnemy = null;
    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        this.AnyProjectileHitGround(collision);

        this.PlayerProjectileHitEnemy(collision);

        this.PlayerSwordHitsEnemyOrEnemyProjectile(collision);

        this.EnemySwordHitsPlayerOrPlayerProjectile(collision);

        this.EnemyProjectileHitsPlayer(collision);
    }

    private void EnemyProjectileHitsPlayer(Collider2D collision)
    {
        if (this.gameObject.CompareTag("Enemy Projectile"))
        {
            if (collision.CompareTag("Player"))
            {
                if (collision.GetComponent<Player>().IsAlive)
                {
                    this.ProcessProjectileHit(collision);
                    AudioSource.PlayClipAtPoint(this.kunaiDamageSFX, Camera.main.transform.position);
                    collision.GetComponent<Player>().PlayerHurting();
                }
            }
        }
    }

    private void PlayerSwordHitsEnemyOrEnemyProjectile(Collider2D collision)
    {
        if (this.gameObject.CompareTag("Player"))
        {
            if (collision.CompareTag("Ninja") || collision.CompareTag("Kunoichi") || collision.CompareTag("Shinobi") || collision.CompareTag("Ninjutsu"))
            {
                if (!this.gameObject.GetComponent<Player>().CanParry && this.gameObject.GetComponent<Player>().IsAttacking)
                {
                    this.DamageCharacter(collision);
                    this.gameObject.GetComponent<Player>().AudioSource.PlayOneShot(this.swordDamageSFX);
                    collision.GetComponent<EnemyMovement>().PushBack();
                }
                else if(this.gameObject.GetComponent<Player>().CanParry && collision.CompareTag("Ninjutsu") && collision.GetComponent<NinjutsuBehaviour>().IsAttacking)
                {
                    this.parriedEnemy = collision.GetComponent<NinjutsuBehaviour>();
                    this.parriedEnemy.IsParried = true;
                    Instantiate<GameObject>(this.parrySparks, collision.transform.localPosition, Quaternion.identity);
                    this.gameObject.GetComponent<Player>().AudioSource.PlayOneShot(this.swordParrySFX);
                    this.parriedEnemy.GetComponent<EnemyMovement>().SendMessage("ParryNinjutsu");                   
                }
            }

            if (collision.CompareTag("Enemy Projectile"))
            {
                if (this.gameObject.GetComponent<Player>().CanParry)
                {
                    collision.gameObject.GetComponent<Animator>().enabled = true;
                    collision.gameObject.GetComponent<Animator>().SetTrigger("parried");
                    this.gameObject.GetComponent<Player>().AudioSource.PlayOneShot(this.kunaiParrySFX);
                    //GameObject.Destroy(collision.gameObject);
                }
            }
        }
    }

    private void EnemySwordHitsPlayerOrPlayerProjectile(Collider2D collision)
    {
        if (this.gameObject.CompareTag("Shinobi"))
        {
            if (collision.CompareTag("Projectile"))
            {
                if (this.gameObject.GetComponent<ShinobiBehaviour>().CanParry)
                {
                    collision.gameObject.GetComponent<Animator>().SetTrigger("parried");
                    this.gameObject.GetComponent<EnemyMovement>().AudioSource.PlayOneShot(this.shurikenParrySFX);
                    //GameObject.Destroy(collision.gameObject);
                }
            }
        }

        if (this.gameObject.CompareTag("Ninjutsu"))
        {
            if (collision.CompareTag("Projectile"))
            {
                if (this.gameObject.GetComponent<NinjutsuBehaviour>().CanParry)
                {
                    collision.gameObject.GetComponent<Animator>().SetTrigger("parried");
                    this.gameObject.GetComponent<EnemyMovement>().AudioSource.PlayOneShot(this.shurikenParrySFX);
                    //GameObject.Destroy(collision.gameObject);
                }
            }

            if(collision.CompareTag("Player") && !collision.GetComponent<Player>().CanParry)
            {
                if (collision.GetComponent<Player>().IsAlive)
                {
                    this.DamageCharacter(collision);
                    this.gameObject.GetComponent<EnemyMovement>().AudioSource.PlayOneShot(this.swordDamageSFX);
                    collision.GetComponent<Player>().PlayerHurting();
                }
            }
        }
    }

    private void PlayerProjectileHitEnemy(Collider2D collision)
    {
        if (this.gameObject.CompareTag("Projectile"))
        {
            if (collision.CompareTag("Ninja") || collision.CompareTag("Kunoichi"))
            {
                this.DamageEnemy(collision);
            }
            else if (collision.CompareTag("Shinobi") && !collision.GetComponent<ShinobiBehaviour>().CanParry)
            {
                this.DamageEnemy(collision);
            }
            else if (collision.CompareTag("Ninjutsu") && !collision.GetComponent<NinjutsuBehaviour>().CanParry)
            {
                this.DamageEnemy(collision);
            }
        }
    }

    private void DamageEnemy(Collider2D collision)
    {
        this.ProcessProjectileHit(collision);
        AudioSource.PlayClipAtPoint(this.shurikenDamageSFX, Camera.main.transform.position);
        collision.GetComponent<EnemyMovement>().PushBack();
    }

    private void AnyProjectileHitGround(Collider2D collision)
    {
        if (this.gameObject.CompareTag("Projectile") || (this.gameObject.CompareTag("Enemy Projectile")))
        {
            if (collision.CompareTag("Ground"))
            {
                Instantiate<GameObject>(this.parrySparks, this.gameObject.transform.localPosition, Quaternion.identity);
                AudioSource.PlayClipAtPoint(this.projectileOnGroundSFX, Camera.main.transform.position);

                this.gameObject.SetActive(false);
                GameObject.Destroy(this.gameObject);
                return;
            }
        }
    }

    private void ProcessProjectileHit(Collider2D collision)
    {
        this.DamageCharacter(collision);

        this.gameObject.SetActive(false);
        GameObject.Destroy(this.gameObject);
    }

    private void DamageCharacter(Collider2D collision)
    {
        if (collision.GetComponent<Health>())
        {
            collision.GetComponent<Health>().DealDamage(this.weaponDamage);
        }
    }
}
