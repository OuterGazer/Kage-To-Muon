using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NinjutsuBehaviour : MonoBehaviour
{
    [SerializeField] GameObject smokePuff;
    public GameObject SmokePuff => this.smokePuff;

    [SerializeField] AudioClip smokePuffSFX;
    public AudioClip SmokePuffSFX => this.smokePuffSFX;

    [SerializeField] float parryRange = 1.0f;
    private LayerMask shurikenMask;
    [SerializeField] AudioClip parrySFX;
    [SerializeField] AudioClip swordAttackSFX;

    private EnemyMovement enemyMovement;
    private Animator ninjutsuAnim;
    private AudioSource audioSource;

    private Player player;

    private Coroutine attackCoroutine = null;

    private bool canParry = false;
    public bool CanParry
    {
        get { return this.canParry; }
        set { this.canParry = value; }
    }
    private bool isAttacking = false;
    public bool IsAttacking => this.isAttacking;
    private bool isParried = false;
    public bool IsParried
    {
        get { return this.isParried; }
        set { this.isParried = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        this.shurikenMask = LayerMask.GetMask("Projectile");

        this.enemyMovement = this.gameObject.GetComponent<EnemyMovement>();
        this.ninjutsuAnim = this.gameObject.GetComponent<Animator>();
        this.audioSource = this.gameObject.GetComponent<AudioSource>();

        this.player = GameObject.FindObjectOfType<Player>();
    }

    private void Update()
    {
        if ((!this.player.IsAlive || !this.enemyMovement.ActivateNinjutsuBehaviour) && this.isAttacking)
        {
            this.StopAttacking();
        }
    }

    public void StopAttacking()
    {
        if (this.attackCoroutine != null)
        {
            this.StopCoroutine(this.attackCoroutine);
            this.attackCoroutine = null;
        }

        this.isAttacking = false;
    }

    void FixedUpdate()
    {
        if (this.player.IsAlive && !this.isParried)
        {
            this.ParryShurikens();

            if (this.enemyMovement.ActivateNinjutsuBehaviour && !this.isAttacking)
            {
                if(this.attackCoroutine == null)
                    this.attackCoroutine = this.StartCoroutine(this.AttackPlayer());
            }
        }
    }

    public IEnumerator AttackPlayer()
    {
        this.isAttacking = true;
        Vector2 backOfPlayer;

        this.ninjutsuAnim.SetTrigger("throwing");

        if(this.player.transform.position.x > this.gameObject.transform.position.x)
            backOfPlayer = new Vector2(this.player.transform.position.x + 1.40f, this.player.transform.position.y);
        else
            backOfPlayer = new Vector2(this.player.transform.position.x - 1.40f, this.player.transform.position.y);

        yield return new WaitForSeconds(0.10f);

        this.enemyMovement.FlipCharSpriteOnIdle();

        this.StartCoroutine(this.enemyMovement.Teleport(backOfPlayer));

        yield return new WaitForSeconds(1.00f); //0.85f seems to work well, let's leave it at 1 seceond for now
        
        this.ninjutsuAnim.SetTrigger("attacking");
        this.audioSource.PlayOneShot(this.swordAttackSFX);

        yield return new WaitForSeconds(1.0f);

        this.isAttacking = false;
        this.attackCoroutine = null;
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
        this.ninjutsuAnim.SetTrigger("parrying");
        this.canParry = true;

        this.audioSource.PlayOneShot(this.parrySFX);

        yield return new WaitForSeconds(0.35f); //The animation is 8 frames long in a sample of 24 frames per second. That makes 8 frames take approximately 0.33 seconds.

        this.canParry = false;
    }

    public void UpdatePlayerInstance(Player player)
    {
        this.player = player;
    }
}
