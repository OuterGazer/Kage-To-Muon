using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyMovement : MonoBehaviour
{
    //Configuration Parameters (things we need to know before the game)

    [SerializeField] float walkSpeed = 3.0f;
    public float WalkSpeed
    {
        get { return this.walkSpeed; }
        set { this.walkSpeed = value; }
    }
    private int patrolTriggerCounter = 0; //used by the patrolling code for the white shinobi.
    public int PatrolTriggerCounter
    {
        get { return this.patrolTriggerCounter; }
        set { this.patrolTriggerCounter = value; }
    }
    [SerializeField] float runSpeed = 10.0f;
    public float RunSpeed
    {
        get { return this.runSpeed; }
        set { this.runSpeed = value; }
    }
    [SerializeField] float pushBackForce = -2.0f;
    [SerializeField] float visionRange = 8.0f;
    [SerializeField] float throwRate = 1.0f;
    public float ThrowRate
    {
        get { return this.throwRate; }
        set { this.throwRate = value; }
    }
    [SerializeField] float throwRange;
    [SerializeField] float waitingTime = 2.0f;
    [SerializeField] AudioClip throwSFX;

    private LayerMask playerAndGroundMask = (1 << 3) | (1 << 6);
    //private LayerMask testMask = LayerMask.GetMask("Player", "Ground"); //both are equivalent and return a layermask that looks for both the ground and the player

    [SerializeField] GameObject enemyWaypoints;
    private List<Transform> enemyPath;
    private Vector2 nextWaypoint;
    private int currentWaypointIndex = 0;
    private Vector2 patrolLeavingPoint; //For use to the white shinobi
    [SerializeField] AudioClip jumpingSFX;
    public AudioClip JumpingSFX => this.jumpingSFX;
    [SerializeField] AudioClip jumpLandindSFX;
    public AudioClip JumpLandingSFX => this.jumpLandindSFX;

    private Coroutine ninjutsuLookOut = null;

    //offset positions for projectile throwing
    private float xStandingOffset = 0.48f;
    private float yStandingOffset = 0.00f;



    //Cached Component References (references to other game objects or components of game objects)

    private Rigidbody2D enemyRB;
    private Animator enemyAnim;
    private SpriteRenderer enemySprite;
    private Collider2D enemyCol;
    private AudioSource audioSource;
    public AudioSource AudioSource => this.audioSource;
    private Player player;
    private SceneCover sceneCover;
    private NinjutsuBehaviour ninjutsu;
    private GameSession gameSession;


    //State variables (to keep track of the variables that govern states)

    private bool isIdle = false;
    public bool IsIdle
    {
        get { return this.isIdle; }
        set { this.isIdle = value; }
    }
    private bool isAlive = true;
    public void SetIsAlive(bool isAlive)
    {
        this.isAlive = isAlive;
        this.enemyAnim.SetBool("isAlive", false);

        this.gameObject.GetComponent<Collider2D>().enabled = false;

        GameObject.Destroy(this.gameObject, 1.50f);
    }
    private bool isPushedBack = false;
    private bool freezeXForAttack = false;
    private bool canThrow = false;
    public bool CanThrow
    {
        get { return this.canThrow; }
        set { this.canThrow = value; }
    }
    private bool isPlayerSpotted = false;
    public bool IsPlayerSpotted => this.isPlayerSpotted;
    private bool isReturningBack = false;
    private bool activateShinobiBehaviour = false;
    public bool ActivateShinobiBehaviour
    {
        get { return this.activateShinobiBehaviour; }
        set { this.activateShinobiBehaviour = value; }
    }
    private bool isResumingPatrol = false;
    public bool IsResumingPatrol
    {
        get { return this.isResumingPatrol; }
        set { this.isResumingPatrol = value; }
    }
    private bool activateNinjutsuBehaviour = false;
    public bool ActivateNinjutsuBehaviour
    {
        get { return this.activateNinjutsuBehaviour; }
        set { this.activateNinjutsuBehaviour = value; }
    }
    private bool hasLookoutFinished = true;


    // Start is called before the first frame update
    void Start()
    {
        this.enemyRB = this.gameObject.GetComponent<Rigidbody2D>();
        this.enemyAnim = this.gameObject.GetComponent<Animator>();
        this.audioSource = this.gameObject.GetComponent<AudioSource>();
        this.gameSession = GameObject.FindObjectOfType<GameSession>();

        if (this.gameObject.CompareTag("Ninjutsu"))
        {
            this.enemySprite = this.gameObject.GetComponentInChildren<SpriteRenderer>();
            this.enemyCol = this.gameObject.GetComponent<Collider2D>();
            this.ninjutsu = this.gameObject.GetComponent<NinjutsuBehaviour>();
        }
            

        this.patrolLeavingPoint = default;
        this.GetEnemyPath();
        this.GetNextWaypoint();
    }

    private void GetEnemyPath()
    {
        this.enemyPath = new List<Transform>();

        foreach(Transform item in this.enemyWaypoints.transform)
        {
            this.enemyPath.Add(item);
        }
    }

    // FixedUpdate is called 50 times per second
    void FixedUpdate()
    {
        if (this.isAlive && !this.gameSession.IsGamePaused)
        {
            this.CheckForPlayerInFront();
            
            this.MoveCharacter();

            if (this.freezeXForAttack)
            {
                this.enemyRB.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }
        else if (this.gameObject.CompareTag("Ninjutsu") && !this.gameSession.IsGamePaused) //to prevent the ninjutsu expert from teleporting after dying we stop the coroutine
        {
            this.StopLookingAround();

            this.ninjutsu.StopAttacking();
        }
    }

    private void StopLookingAround()
    {
        if (this.ninjutsuLookOut != null)
        {
            this.StopCoroutine(this.ninjutsuLookOut);
            this.ninjutsuLookOut = null;
        }
    }

    private void MoveCharacter()
    {
        if (!this.gameObject.CompareTag("Ninjutsu"))
        {
            if (!this.isIdle && !this.isPushedBack)
            {
                this.ChangeWaypointOnApproach();

                if (!this.gameObject.CompareTag("Shinobi")) //ninja and kunoichi patrol runnin
                {
                    this.enemyRB.MovePosition(this.enemyRB.position + (this.nextWaypoint.normalized * this.runSpeed * Time.fixedDeltaTime));
                }
                else if (this.gameObject.CompareTag("Shinobi") && !this.activateShinobiBehaviour) //shinobi patrols walking
                {
                    this.enemyRB.MovePosition(this.enemyRB.position + (this.nextWaypoint.normalized * this.walkSpeed * Time.fixedDeltaTime));
                }
            }
            else if (this.isPushedBack)
            {
                this.enemyRB.MovePosition(this.enemyRB.position + (this.nextWaypoint.normalized * this.pushBackForce * Time.fixedDeltaTime)); //pushBackForce is already a negative number
            }
        }
        else //patrolling behaviour for ninjutsu expert
        {
            if (this.hasLookoutFinished && !this.activateNinjutsuBehaviour)
            {
                this.TeleportToNextPatrolPointAfterLookout();
            }     
        }
    }

    private void TeleportToNextPatrolPointAfterLookout()
    {
        if(this.ninjutsuLookOut == null)
            this.ninjutsuLookOut = this.StartCoroutine(this.LookAround());        
    }

    private IEnumerator LookAround()
    {
        this.hasLookoutFinished = false;

        yield return new WaitForSeconds(this.waitingTime);

        this.FlipCharSpriteOnIdle();

        this.ChangeWaypointOnApproach();

        yield return new WaitForSeconds(this.waitingTime);

        this.ChangeWaypointOnApproach();

        this.StartCoroutine(this.Teleport(this.nextWaypoint));

        this.FlipCharSpriteOnIdle();

        this.hasLookoutFinished = true;
        this.ninjutsuLookOut = null;
    }

    public IEnumerator Teleport(Vector2 targetPosition)
    {
        this.enemySprite.color = Color.clear;
        this.enemyCol.enabled = false;
        Instantiate<GameObject>(this.ninjutsu.SmokePuff, this.gameObject.transform.position, Quaternion.identity);
        this.audioSource.PlayOneShot(this.ninjutsu.SmokePuffSFX);

        yield return new WaitForSeconds(0.50f);

        //this.enemyRB.MovePosition(targetPosition);
        this.gameObject.transform.position = targetPosition;
        this.enemySprite.color = Color.white;
        this.enemyCol.enabled = true;
        Instantiate<GameObject>(this.ninjutsu.SmokePuff, targetPosition, Quaternion.identity);
        this.audioSource.PlayOneShot(this.ninjutsu.SmokePuffSFX);
    }

    private void ChangeWaypointOnApproach()
    {
        if ((this.gameObject.transform.position - this.enemyPath[this.currentWaypointIndex].position).sqrMagnitude <= 0.01f)
        {
            if (this.isResumingPatrol)
                this.isResumingPatrol = false;

            if (!this.isReturningBack)
                this.currentWaypointIndex++;
            else
                this.currentWaypointIndex--;

            this.GetNextWaypoint();

            if (this.currentWaypointIndex + 1 == this.enemyPath.Count)
            {
                this.isReturningBack = true;
            }
            else if (this.currentWaypointIndex == 0)
            {
                this.isReturningBack = false;
            }
        }
    }

    private void GetNextWaypoint()
    {
        Vector3 currentPosition = this.gameObject.transform.position;

        if (!this.gameObject.CompareTag("Ninjutsu"))
        {
            if (this.patrolLeavingPoint == default)
            {
                this.nextWaypoint = this.enemyPath[this.currentWaypointIndex].position - currentPosition; //standard is follow the regular patrol way
            }
            else
            {
                this.currentWaypointIndex = 0;
                this.nextWaypoint = this.enemyPath[this.currentWaypointIndex].position - currentPosition; //if the white shinobi is outside the patrol due to following player, reset his patrol
                this.patrolLeavingPoint = default;
            }

            this.FlipCharSpriteOnPatrol();
        }
        else //ninjutsu expert behaviour
        {
            if((this.currentWaypointIndex <= this.enemyPath.Count) && (this.currentWaypointIndex > 0))
                this.nextWaypoint = this.enemyPath[this.currentWaypointIndex].position;
            else
                this.nextWaypoint = this.enemyPath[0].position;
        }
    }

    private void CheckForPlayerInFront()
    {
        if (this.player.IsAlive && !this.player.IsBehindCover)
        {
            RaycastHit2D hit = default;

            if (this.ninjutsu == null)
                hit = Physics2D.Raycast(this.gameObject.transform.position, Vector2.right * Mathf.Sign(this.gameObject.transform.localScale.x), //raycast always to the direction the enemy is looking
                                                    this.visionRange, this.playerAndGroundMask);
            else if (!this.ninjutsu.IsParried)
                hit = Physics2D.Raycast(this.gameObject.transform.position, Vector2.right * Mathf.Sign(this.gameObject.transform.localScale.x), //raycast always to the direction the enemy is looking
                                                    this.visionRange, this.playerAndGroundMask);

            if (hit)
            {
                if (hit.collider.CompareTag("Ground"))
                {
                    if (this.isPlayerSpotted)
                    {
                        if(!this.gameObject.CompareTag("Ninjutsu"))
                            this.ResumePatrol();
                        else if(this.gameObject.CompareTag("Ninjutsu") && !this.ninjutsu.IsAttacking)//to avoid thte ninjutsu expert attack to stop midway because of this raycasting
                            this.ResumePatrol();
                    }

                    return;
                }
                else if (hit.collider.CompareTag("Player"))
                {
                    this.isPlayerSpotted = true;
                    this.player.CanPlayerHide = false;

                    if (!this.canThrow && !this.gameObject.CompareTag("Shinobi") && !this.gameObject.CompareTag("Ninjutsu")) //behaviour for ninja and kunoichi
                    {
                        this.StartCoroutine(this.ShootProjectile());
                    }
                    else if (this.gameObject.CompareTag("Shinobi") && !this.activateShinobiBehaviour) //behaviour for shinobi takes place in its own script
                    {
                        this.patrolLeavingPoint = this.gameObject.transform.position;
                        this.activateShinobiBehaviour = true;
                    }
                    else if (this.gameObject.CompareTag("Ninjutsu") && !this.activateNinjutsuBehaviour) //behaviour for ninjutsu expert takes place in its own script
                    {
                        this.StopLookingAround();
                        this.activateNinjutsuBehaviour = true;
                    }
                }
            }
            else if (this.isPlayerSpotted)
            {
                if (!this.gameObject.CompareTag("Ninjutsu"))
                    this.ResumePatrol();
                else if (this.gameObject.CompareTag("Ninjutsu") && !this.ninjutsu.IsAttacking)//to avoid thte ninjutsu expert attack to stop midway because of this raycasting
                    this.ResumePatrol();
            }
        }
    }

    public void ResumePatrol()
    {
        this.canThrow = false;
        this.isPlayerSpotted = false;
        this.player.CanPlayerHide = true;

        if (this.gameObject.CompareTag("Ninjutsu") && this.activateNinjutsuBehaviour)
        {
            this.activateNinjutsuBehaviour = false;
            this.hasLookoutFinished = true;
            this.isReturningBack = false;
            this.currentWaypointIndex = 0;
            this.nextWaypoint = this.enemyPath[this.currentWaypointIndex].position;
            this.StartCoroutine(this.Teleport(this.nextWaypoint));
        }

        if (this.gameObject.CompareTag("Shinobi") && this.activateShinobiBehaviour) //turn all necesary variables to the well functioning of the patrolling behaviour to default values
        {
            this.activateShinobiBehaviour = false;
            this.enemyAnim.SetBool("isPlayerSpotted", false);
            this.enemyAnim.SetBool("isPlayerInRange", false);

            this.isReturningBack = false;
            this.patrolTriggerCounter = 1; //should be 0
            this.isResumingPatrol = true;

            /*if (this.isIdle) //if I want to fix the moonwalk bug after returning to patrol some day
            {
                this.isIdle = false;
                this.enemyAnim.SetBool("isIdle", false);
            }*/           

            this.GetNextWaypoint();
        }
    }

    private IEnumerator ShootProjectile()
    {
        this.canThrow = true;

        this.enemyAnim.SetTrigger("throwing");

        yield return new WaitForSeconds(this.throwRate);

        this.canThrow = false;
    }

    /*private void TurnCharacter()
    {
        this.runSpeed *= -1;
        this.FlipCharSpriteOnPatrol();
    }*/

    private void FlipCharSpriteOnPatrol()
    {
        this.gameObject.transform.localScale = new Vector3(Mathf.Abs(this.gameObject.transform.localScale.x) * Mathf.Sign(this.nextWaypoint.x),
                                                           this.gameObject.transform.localScale.y,
                                                           this.gameObject.transform.localScale.z);
    }

    public void FlipCharSpriteOnIdle()
    {
        this.gameObject.transform.localScale = new Vector3(this.gameObject.transform.localScale.x * -1,
                                                           this.gameObject.transform.localScale.y,
                                                           this.gameObject.transform.localScale.z);
    }

    public void PushBack()
    {
        this.StartCoroutine(this.PushBackEnemy());
    }

    private IEnumerator PushBackEnemy()
    {
        this.isPushedBack = true;

        if (this.gameObject.CompareTag("Shinobi") || this.gameObject.CompareTag("Ninjutsu"))
            this.enemyAnim.SetTrigger("hurting");

        yield return new WaitForSeconds(0.15f);

        this.isPushedBack = false;
    }

    public void ParryNinjutsu()
    {
        this.StartCoroutine(this.ParryBehaviour());
    }
    private IEnumerator ParryBehaviour()
    {
        this.enemyAnim.SetTrigger("hurting");
        this.enemyAnim.SetBool("isParried", true);
        this.gameObject.GetComponent<NinjutsuBehaviour>().StopAttacking();

        yield return new WaitForSeconds(3.0f);

        this.gameObject.GetComponent<NinjutsuBehaviour>().IsParried = false;
        this.enemyAnim.SetBool("isParried", false);
    }

    public void UpdatePlayerInstance(Player player)
    {
        this.player = player;
    }

    public void UpdateSceneCoverInstance(SceneCover sceneCover)
    {
        this.sceneCover = sceneCover;
    }


    //general events

    private void OnTriggerExit2D(Collider2D collision)
    {
        /*if(this.isAlive && collision.gameObject.layer == 6) //layer 6 is the ground
        {
            this.TurnCharacter();
        }*/
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        /*if (collision.gameObject.CompareTag("Player"))
        {
            //this.isIdle = true;
            this.TurnCharacter();
        }*/
    }

    private void OnDestroy()
    {
        if (!this.player.CanPlayerHide)
            this.player.CanPlayerHide = true;
    }


    //animation events

    public void FreezeForAttack()
    {
        this.freezeXForAttack = true;
    }

    public void UnfreezeChar()
    {
        this.freezeXForAttack = false;

        this.enemyRB.constraints = RigidbodyConstraints2D.None;
        this.enemyRB.freezeRotation = true;
    }

    public void SpawnProjectile(ProjectileBehaviour inProjectile)
    {
        Vector2 spawnPosition;

        spawnPosition = new Vector2(this.gameObject.transform.position.x + (this.xStandingOffset * Mathf.Sign(this.gameObject.transform.localScale.x)),
                                    this.gameObject.transform.position.y + this.yStandingOffset);

        ProjectileBehaviour projectile = Instantiate<ProjectileBehaviour>(inProjectile, spawnPosition, Quaternion.identity);

        projectile.GetMovementDirection(Mathf.Sign(this.gameObject.transform.localScale.x), this.throwRange);

        this.audioSource.PlayOneShot(this.throwSFX);

        //IDEA: have enemies throw diagonally on slopes
    }
}
