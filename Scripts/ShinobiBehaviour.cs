using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShinobiBehaviour : MonoBehaviour
{
    [SerializeField] float throwRange = 3.0f;
    [SerializeField] float parryRange = 1.0f;
    private LayerMask playerMask = 1 << 3;
    private LayerMask shurikenMask;

    [SerializeField] AudioClip parrySFX;

    private EnemyMovement enemyMovement;
    private Animator shinobiAnim;
    private Rigidbody2D shinobiRB;
    private AudioSource audioSource;

    private Player player;

    private bool canParry = false;
    public bool CanParry
    {
        get { return this.canParry; }
        set { this.canParry = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        this.shurikenMask = LayerMask.GetMask("Projectile");

        this.enemyMovement = this.gameObject.GetComponent<EnemyMovement>();
        this.shinobiAnim = this.gameObject.GetComponent<Animator>();
        this.shinobiRB = this.gameObject.GetComponent<Rigidbody2D>();
        this.audioSource = this.gameObject.GetComponent<AudioSource>();

        this.player = GameObject.FindObjectOfType<Player>();
    }

    void FixedUpdate()
    {
        if (this.enemyMovement.ActivateShinobiBehaviour)
        {
            if (this.player.IsAlive)
            {
                this.ParryShurikens();

                this.AttackPlayer();
            }
            else
            {
                this.enemyMovement.ResumePatrol();
            }
        }
    }

    private void ParryShurikens()
    {
        if (Physics2D.Raycast(this.gameObject.transform.position,
                              Vector2.right * Mathf.Sign(this.gameObject.transform.localScale.x),
                              this.parryRange, this.shurikenMask))
        {            
            if (!this.canParry)
                this.StartCoroutine(this.SetParryState());
        }
    }

    private IEnumerator SetParryState()
    {
        this.shinobiAnim.SetTrigger("parrying");
        this.canParry = true;

        this.audioSource.PlayOneShot(this.parrySFX);

        yield return new WaitForSeconds(0.35f); //The animation is 8 frames long in a sample of 24 frames per second. That makes 8 frames take approximately 0.33 seconds.

        this.canParry = false;
    }

    private void AttackPlayer()
    {
        this.shinobiAnim.SetBool("isPlayerSpotted", true);
        this.shinobiAnim.SetBool("isPlayerInRange", false);

        if (!Physics2D.Raycast(this.gameObject.transform.position,
                             Vector2.right * Mathf.Sign(this.gameObject.transform.localScale.x),
                             this.throwRange, this.playerMask))
        {
            if (this.player != null)
            {
                Vector2 playerPos = this.player.transform.position - this.gameObject.transform.position;
                this.shinobiRB.MovePosition(this.shinobiRB.position + (playerPos.normalized * this.enemyMovement.RunSpeed * Time.fixedDeltaTime));
            }
        }
        else if (!this.enemyMovement.CanThrow)
        {
            this.shinobiAnim.SetBool("isPlayerInRange", true);
            this.StartCoroutine(this.InitiateAttack());
        }
    }

    private IEnumerator InitiateAttack()
    {
        this.enemyMovement.CanThrow = true;

        this.shinobiAnim.SetTrigger("throwing");

        yield return new WaitForSeconds(this.enemyMovement.ThrowRate);

        this.enemyMovement.CanThrow = false;
    }

    public void UpdatePlayerInstance(Player player)
    {
        this.player = player;
    }
}
