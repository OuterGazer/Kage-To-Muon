using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class Player : MonoBehaviour
{
    //Configuration Parameters (things we need to know before the game)
    [Header("Horizontal Movement Properties")]
    [SerializeField] float maxRunSpeed = 10.0f;
    [SerializeField] float maxWalkSpeed = 3.0f;
    private float movementSpeed;
    [SerializeField] float maxSlidingTime = 0.75f;
    [SerializeField] float slidingSpeed = 5.0f;
    private float slidingTime;
    [Header("Jump Movement Properties")]
    [SerializeField] float jumpForce = 5.0f;
    [SerializeField] float wallJumpForce = 5.0f;
    [SerializeField] int maxJumpNumber = 2;
    private int jumps = 0;
    [SerializeField] int maxVerticalJumpsOnWalls = 3; //This will allow one vertical jump after we perform 1 jump to get to the wall
    //[SerializeField] float horImpulse = 1.0f;
    [Header("Ladder Movement Properties")]
    [SerializeField] float maxLadderClimbSpeed = 2.50f;
    private float ladderClimbSpeed;

    [Header("Inventory")]
    [SerializeField] int shurikenAmount = 5;
    [SerializeField] float throwRange;
    public int ShurikenAmount
    {
        get { return this.shurikenAmount; }
        set { this.shurikenAmount = value; }
    }

    [Header("Sound Effects")]
    [SerializeField] private AudioClip femaleJump;
    [SerializeField] private AudioClip maleJump;
    [SerializeField] private AudioClip jumpLanding;
    [SerializeField] private AudioClip swordAttack;
    [SerializeField] private AudioClip shurikenThrow;
    [SerializeField] private AudioClip swordParry;
    [SerializeField] private AudioClip hazardDeath;

    private LayerMask groundMask = 1 << 6; //Ground layermask is 6
    private LayerMask ladderMask = 1 << 7;

    //offset positions for projectile throwing
    private float xStandingOffset = 0.35f;
    private float xJumpingOffset = 0.40f;
    private float yCrouchingOffset = -0.20f;
    private float yJumpingOffset = -0.10f;



    //Cached Component References (references to other game objects or components of game objects)

    private GameSession gameSession;
    private Rigidbody2D playerRB;
    private Animator playerAnim;
    private Collider2D playerCol;
    private SpriteRenderer playerSprite;
    private AudioSource audioSource;
    public AudioSource AudioSource => this.audioSource;


    //State variables (to keep track of the variables that govern states)
    private bool hasStoppedSliding = false;
    private bool isAlive = true;
    public bool IsAlive => this.isAlive;
    public void SetIsAlive(bool isAlive)
    {
        this.isAlive = isAlive;
        this.playerAnim.SetTrigger("Die");
        this.gameSession.ProcessPlayerDeath();
    }
    private bool hasFlipped = false;
    private bool isGrounded;
    private bool hasPressedJumpButton;
    private bool hasClimbedLadder = false;
    private bool isThereALadderBelow = false;
    private bool shouldSlide = false;
    private bool isSlideExtended = false;
    private bool freezeXForAttack = false;
    private bool isAttacking = false;
    public bool IsAttacking
    {
        get { return this.isAttacking; }
        set { this.isAttacking = value; }
    }

    private bool canParry = false;
    public bool CanParry
    {
        get { return this.canParry; }
        set { this.canParry = value; }
    }
    private bool canPlayerHide = true;
    public bool CanPlayerHide
    {
        get { return this.canPlayerHide; }
        set { this.canPlayerHide = value; }
    } //This bool will be changed in the relevant enemy scripts to disallow the player from hiding when it is spotted.
    private bool isBehindCover = false;
    public bool IsBehindCover
    {
        get { return this.isBehindCover; }
        set { this.isBehindCover = value; }
    }

    private void Awake()
    {
        string name = GameObject.FindObjectOfType<PlayerSelection>().Player;

        if (!this.gameObject.name.Equals(name))
        {
            this.gameObject.SetActive(false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        this.gameSession = GameObject.FindObjectOfType<GameSession>();
        
        if (!this.gameSession.IsShurikenUpdated)
            this.gameSession.ChangeShurikenAmount(this.shurikenAmount); //to display the number of shurikens in the UI at start
        else
            this.shurikenAmount = this.gameSession.ShurikenAmount;

        this.gameSession.UpdatePlayerInfoAfterDeath();


        this.playerRB = this.gameObject.GetComponent<Rigidbody2D>();
        this.playerAnim = this.gameObject.GetComponent<Animator>();
        this.playerCol = this.gameObject.GetComponent<Collider2D>();
        this.playerSprite = this.gameObject.GetComponentInChildren<SpriteRenderer>();
        this.audioSource = this.gameObject.GetComponent<AudioSource>();

        this.playerCol.sharedMaterial.friction = 0.0f;

        this.UpdatePlyerInstanceToEnemiesAfterDeath();

        if (GameObject.Find("Checkpoint").GetComponent<LevelLoader>().IsCheckpointReached)
            this.playerRB.position = GameObject.Find("Checkpoint").transform.position;
    }

    private void UpdatePlyerInstanceToEnemiesAfterDeath()
    {
        EnemyMovement[] enemyArray = GameObject.FindObjectsOfType<EnemyMovement>();
        foreach (EnemyMovement item in enemyArray)
        {
            item.UpdatePlayerInstance(this);
        }

        ShinobiBehaviour[] shinobiArray = GameObject.FindObjectsOfType<ShinobiBehaviour>();
        foreach (ShinobiBehaviour item in shinobiArray)
        {
            item.UpdatePlayerInstance(this);
        }

        NinjutsuBehaviour[] ninjutsuArray = GameObject.FindObjectsOfType<NinjutsuBehaviour>();
        foreach (NinjutsuBehaviour item in ninjutsuArray)
        {
            item.UpdatePlayerInstance(this);
        }
    }

    private void Update()
    {
        if (this.isAlive && !this.gameSession.IsGamePaused)
        {
            if (!this.isBehindCover) //to avoid moving or attacking while on cover and buffering a jump right after going out of cover
            {
                //check input for horizontal movement
                this.RunningBehaviour();

                //check input for attacking idle, running crouching and jumping. Also for parrying
                this.AttackThrowAndParryBehaviour();

                //check input for jumping
                this.JumpingBehaviour();
            }            

            //check for crouching
            this.CrouchBehaviour();            

            //check input for climbing ladders
            this.ClimbingLadderBehaviour();
        }
    }

    private void FixedUpdate()
    {
        if (this.isAlive && !this.gameSession.IsGamePaused)
        {
            if (!this.isBehindCover)
            {
                this.Run();
            }            

            this.Jump();

            this.ClimbLadder();

            if (this.shouldSlide)
            {
                this.CheckGroundAboveWhileSliding();

                if (!this.shouldSlide) //if checkgroundabove method turns the boolean false, exit inmediately so sliding code doesn't run
                    return;

                this.Sliding();
            }

            if (this.freezeXForAttack)
            {
                this.playerRB.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }
    }

    //private methods for use within the class------------------------------------------------------------------------------------------

    private void AttackThrowAndParryBehaviour()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            this.playerAnim.SetTrigger("attacking");
            this.audioSource.PlayOneShot(this.swordAttack);
        }

        if (Input.GetButtonDown("Fire2"))
        {
            if(this.shurikenAmount > 0)
            {
                this.playerAnim.SetTrigger("throwing");
                this.shurikenAmount -= 1;
                this.gameSession.ChangeShurikenAmount(this.shurikenAmount);
            }
            
        }

        if (Input.GetButtonDown("Fire3"))
        {
            this.playerAnim.SetTrigger("parrying");
            this.audioSource.PlayOneShot(this.swordParry);

            if (!this.canParry)
                this.StartCoroutine(this.SetParryState());
        }
    }

    private IEnumerator SetParryState()
    {
        this.canParry = true;

        yield return new WaitForSeconds(0.35f); //The animation is 8 frames long in a sample of 24 frames per second. That makes 8 frames take approximately 0.33 seconds.

        this.canParry = false;
    }

    private void CrouchBehaviour()
    {
        if (Mathf.Approximately(this.movementSpeed, 0))
        {
            if (Input.GetButton("Crouch"))//This is just idle crouching in place, you can't move because there is no animation for moving while crouching
            {
                this.playerAnim.SetBool("canCrouch", true);
            }
            else
            {
                this.playerAnim.SetBool("canCrouch", false);
            }
        }
        else
        {
            this.playerAnim.SetBool("canCrouch", false);

            if ((Input.GetKeyDown(KeyCode.RightControl) || Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.C)) && 
                !this.shouldSlide)
            {
                this.shouldSlide = true;
                this.slidingTime = 0.0f;
                this.playerAnim.SetTrigger("startSliding");
            }
        }
    }

    private void CheckGroundAboveWhileSliding()
    {
        if (this.slidingTime > this.maxSlidingTime - 0.06f) //Check if there is ground above the last 3 frames of the sliding, if there's ground, keep sliding until there is nothing above
        {
            RaycastHit2D hit = Physics2D.Raycast(this.gameObject.transform.position, Vector2.up, this.playerCol.bounds.extents.y + 0.1f, this.groundMask);

            if (hit)
            {
                this.shouldSlide = true;
                this.isSlideExtended = true;
                this.slidingTime = 0.0f;
                //this.playerAnim.SetTrigger("startSliding");
            }
        }

        if (this.isSlideExtended)
        {
            RaycastHit2D hit = Physics2D.Raycast(this.gameObject.transform.position, Vector2.up, this.playerCol.bounds.extents.y + 0.1f, this.groundMask);

            if (!hit)
            {
                this.isSlideExtended = false;
                this.playerAnim.SetTrigger("stopSliding");
                this.shouldSlide = false;
                this.playerRB.velocity = Vector2.zero;//if the player keeps crouch pressed while sliding and don't press a movement direction, it keeps sliding forever. This prevents it.
            }
        }
    }

    private void Sliding()
    {
        if(this.slidingTime < this.maxSlidingTime)
        {
            this.slidingTime += Time.fixedDeltaTime;            

            this.playerRB.velocity = new Vector2(this.slidingSpeed * Mathf.Sign(this.gameObject.transform.localScale.x),
                                                 this.playerRB.velocity.y); //needs Vector2.zero when shouldSlide is turned to false so the player stops as soon as sliding is over
        }
        else
        {
            this.playerAnim.SetTrigger("stopSliding");
            this.shouldSlide = false;
            this.playerRB.velocity = Vector2.zero; //if the player keeps crouch pressed while sliding and don't press a movement direction, it keeps sliding forever. This prevents it.
        }
    }

    private void ClimbingLadderBehaviour()
    {
        if (!this.playerCol.IsTouchingLayers(this.ladderMask))
        {
            this.SetPlayerBackToNormal();

            return;
        }
        else
        {
            this.ladderClimbSpeed = Input.GetAxis("Vertical") * this.maxLadderClimbSpeed;
            
            if (this.isGrounded)
            {
                this.SetPlayerBackToNormal();

                //The character slides when touching ladders and stops moving horizontally, in this case we apply the remaining velocity as an impulse in the oppositte direction to stop it.
                if (Mathf.Approximately(this.movementSpeed, 0) && !this.hasStoppedSliding)
                {
                    this.playerRB.AddForce(-this.playerRB.velocity, ForceMode2D.Impulse);
                    this.hasStoppedSliding = true;
                }
            }
        }
    }

    private void ClimbLadder()
    {
        //To allow for going through platforms with the ladder look the OnCollisionEnter2D event at the bottom of this script

        if (!Mathf.Approximately(this.ladderClimbSpeed, 0))
        {
            this.playerAnim.SetBool("isClimbing", true);
            this.playerAnim.speed = Mathf.Abs(this.ladderClimbSpeed / this.maxLadderClimbSpeed);

            //I might need to set the x velocity component to 0 if I want the player to be able to jump from ladders
            this.playerRB.velocity = new Vector2(this.playerRB.velocity.x, this.ladderClimbSpeed);
            this.playerRB.gravityScale = 0f;

            this.hasClimbedLadder = true;

            if (this.isGrounded && this.ladderClimbSpeed < 0 && this.isThereALadderBelow) //allow to climb down ladders from above
            {
                this.playerCol.isTrigger = true;
                this.isGrounded = false;

                //TODO: allow for downclimbing ladders crossing multiple levels
            }
        }
        else
        {
            if (this.hasClimbedLadder)
            {
                this.playerRB.velocity = Vector2.zero;
                this.playerAnim.speed = 0;
            }
        }

        if (this.isGrounded)
        {
            this.CheckForLadderBelow();

            this.SetPlayerBackToNormal();
        }
    }

    private void CheckForLadderBelow()
    {
        //The raycast begins just sightly below the player's collider
        RaycastHit2D hit = Physics2D.Raycast(this.gameObject.transform.position, Vector2.down, 0.3f, this.ladderMask);

        if (hit)
        {
            this.isThereALadderBelow = true;
        }
        else
        {
            this.isThereALadderBelow = false;
        }
    }

    private void SetPlayerBackToNormal()
    {
        if (this.hasClimbedLadder)
        {
            this.hasClimbedLadder = false;
            this.playerAnim.speed = 1f;
            this.playerRB.gravityScale = 1f;
            this.playerAnim.SetBool("isClimbing", false);

            this.ladderClimbSpeed = 0;
        }
    }

    private void JumpingBehaviour()
    {
        if (Input.GetButtonDown("Jump") && !this.shouldSlide)
        {
            this.hasPressedJumpButton = true;
        }
    }

    private void CheckIfGrounded()
    {
        if (!this.playerCol.isTrigger)
        {
            RaycastHit2D hit = Physics2D.Raycast(this.gameObject.transform.position, Vector2.down, this.playerCol.bounds.extents.y + 0.15f, this.groundMask); //originally 0.02f

            if (hit)
            {
                if (!this.isGrounded && !this.audioSource.isPlaying && this.jumps != 0)
                    this.audioSource.PlayOneShot(this.jumpLanding);

                this.isGrounded = true;
                this.playerAnim.SetBool("isGrounded", true);
                this.jumps = 0;
            }
            else
            {
                //Delay isGrounded = false for 5 frames to allow for jumping right at the ledge
                this.StartCoroutine(this.DelayUngrounding());
            }
        }
    }

    private IEnumerator DelayUngrounding()
    {
        yield return new WaitForSeconds(5f / 60f); //5 frames divided by 60 in 1 second

        this.isGrounded = false;
        this.playerAnim.SetBool("isGrounded", false); //Delay isGrounded = false for 5 frames to allow for jumping right at the ledge
    }

    private void Jump()
    {
        this.CheckIfGrounded();

        if (this.hasPressedJumpButton && this.isGrounded)
        {
            //TODO: implement jumping from ladders

            //Allows for jumping when idle
            if (this.playerRB.constraints == RigidbodyConstraints2D.FreezeAll)
            {
                this.playerRB.constraints = RigidbodyConstraints2D.None;
                this.playerRB.freezeRotation = true;
            }

            this.playerRB.AddForce(Vector2.up * this.jumpForce, ForceMode2D.Impulse);
            this.playerAnim.SetTrigger("jumping");
            this.PlayJumpingSFX();

            this.hasPressedJumpButton = false;
            this.jumps++;

            //Code for adding a little horizontal impulse to the jump, discarded for now
            /*if (this.movementSpeed != 0) //if the player is moving, add a little impulse in the movement direction to achieve a longer jump
            {
                //multiply by (movementSpeed / maxRunSpeed) to make the horizontal impulse scale with current hor. movement speed
                if (this.movementSpeed > 0)
                    this.playerRB.AddForce(Vector2.one * this.jumpForce * (Mathf.Abs(this.movementSpeed) / this.maxRunSpeed),
                                           ForceMode2D.Impulse);
                else
                    this.playerRB.AddForce(new Vector2(-1.0f, 1.0f) * this.jumpForce * (Mathf.Abs(this.movementSpeed) / this.maxRunSpeed),
                                           ForceMode2D.Impulse);
            }
            else
            {
                this.playerRB.AddForce(Vector2.up * this.jumpForce, ForceMode2D.Impulse);
            }*/
        }

        //Code for walljumping
        if (this.hasPressedJumpButton &&
            !this.isGrounded &&
            (this.jumps < this.maxJumpNumber && this.jumps > 0))
        {
            this.hasPressedJumpButton = false;

            if (!this.hasFlipped) //if character is facing right, apply a force 45º in the opposite direction
            {
                this.WallJump(Vector2.right, new Vector2(-1.0f, 1.5f));
            }
            else
            {
                this.WallJump(Vector2.left, new Vector2(1.0f, 1.5f));
            }

        }
    }

    private void WallJump(Vector2 moveDir, Vector2 jumpDir)
    {
        RaycastHit2D hit = Physics2D.Raycast(this.gameObject.transform.position, moveDir, this.playerCol.bounds.extents.x + 0.2f, this.groundMask);

        if (hit)
        {
            this.playerRB.velocity = Vector2.zero;
            this.playerRB.AddForce(jumpDir * this.wallJumpForce, ForceMode2D.Impulse);
            this.jumps++;

            this.FlipCharSpriteOnPatrol();

            this.playerAnim.SetTrigger("wallJumping");
            this.PlayJumpingSFX();
        }
    }

    private void PlayJumpingSFX()
    {
        if (this.gameObject.name.Equals("Player Ninja"))
        {
            this.audioSource.PlayOneShot(this.maleJump);
        }
        else
        {
            this.audioSource.PlayOneShot(this.femaleJump);
        }
    }

    private void RunningBehaviour()
    {
        if (!Input.GetButton("Walk"))
        {
            this.movementSpeed = Input.GetAxis("Horizontal") * this.maxRunSpeed;

            this.playerAnim.SetFloat("canRun", Mathf.Abs(this.movementSpeed));

            this.playerAnim.SetFloat("canWalkFloat", 0);
            this.playerAnim.SetBool("canWalk", false);
        }
        else
        {
            this.movementSpeed = Input.GetAxis("Horizontal") * this.maxWalkSpeed;

            this.playerAnim.SetFloat("canWalkFloat", Mathf.Abs(this.movementSpeed));

            this.playerAnim.SetFloat("canRun", 0);
            this.playerAnim.SetBool("canWalk", true);
        }
        
        this.AvoidSlippingDownOnSlopes();

        this.KeepPlayerVertical();
    }

    private void Run()
    {
        if (!Mathf.Approximately(this.movementSpeed, 0) && (this.jumps < this.maxVerticalJumpsOnWalls)) //We account for jumps to avoid players jumping vertically on walls infinitely
        {
            this.playerRB.velocity = new Vector2(this.movementSpeed, this.playerRB.velocity.y);

            this.hasStoppedSliding = false;

            this.FlipCharSpriteOnPatrol(); //flip the character to face the movement direction
        }

        //this.AvoidGettingStuckOnVerticalWalls();
    }

    private void KeepPlayerVertical()
    {
        if (this.gameObject.transform.rotation != Quaternion.identity)
            this.gameObject.transform.rotation = Quaternion.identity;
    }

    private void AvoidSlippingDownOnSlopes()
    {
        if (!this.playerCol.IsTouchingLayers((this.ladderMask)))
        {
            if (Mathf.Approximately(this.movementSpeed, 0) &&
                this.jumps == 0 &&
                this.isGrounded && !this.shouldSlide)
            {
                //this.playerRB.gravityScale = 0;
                this.playerRB.constraints = RigidbodyConstraints2D.FreezeAll;

                //to avoid weird jump hickups sliding on slopes
                /*if (!this.hasStoppedSliding)
                {
                    this.playerRB.AddForce(-this.playerRB.velocity, ForceMode2D.Impulse);
                    this.hasStoppedSliding = true;
                }*/
            }
            else
            {
                //this.playerRB.gravityScale = 1;
                this.playerRB.constraints = RigidbodyConstraints2D.None;
                this.playerRB.freezeRotation = true;
            }
        }
    }

    private void FlipCharSpriteOnPatrol()
    {
        if (!this.playerAnim.GetBool("isClimbing"))
        {
            if (!this.hasFlipped && this.playerRB.velocity.x < 0)
            {
                this.ChangeLocalScaleX(true);
            }
            else if (this.hasFlipped && this.playerRB.velocity.x > 0)
            {
                this.ChangeLocalScaleX(false);
            }
        }
    }

    private void ChangeLocalScaleX(bool isFlipped)
    {
        this.gameObject.transform.localScale = new Vector3(Mathf.Abs(this.gameObject.transform.localScale.x) * Mathf.Sign(this.playerRB.velocity.x),
                                                                     this.gameObject.transform.localScale.y,
                                                                     this.gameObject.transform.localScale.z);
        this.hasFlipped = isFlipped;
    }


    //Public methods for use outside the class------------------------------------------------------------------------------------------
    public void PlayerHurting()
    {
        this.playerAnim.SetTrigger("Hurt");

        //These 3 lines are in case that the player gets hit while idle.
        this.playerRB.gravityScale = 1;
        this.playerRB.constraints = RigidbodyConstraints2D.None;
        this.playerRB.freezeRotation = true;
        this.KeepPlayerVertical();

        //this.playerRB.AddForce(new Vector2(7.0f, 5.0f), ForceMode2D.Impulse);

        this.playerCol.sharedMaterial.friction = 1.0f;
    }

    public void HidePlayerBehindCover()
    {
        this.playerSprite.sortingLayerName = "Foreground Hideable";
        this.playerSprite.sortingOrder = -2;
        Physics2D.IgnoreLayerCollision(3, 9, true);
    }

    public void UnHidePlayerBehindCover()
    {
        this.playerSprite.sortingLayerName = "Interactables";
        this.playerSprite.sortingOrder = 2;
        Physics2D.IgnoreLayerCollision(3, 9, false);
    }


    //Regular Events------------------------------------------------------------------------------------------
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (this.isAlive)
        {
            //Collision with above laying platforms when coming from below climbing a ladder, to allow the player get on top of the platform
            if (collision.collider.CompareTag("Ground") &&
                collision.GetContact(0).point.y > this.gameObject.transform.position.y + 0.65f && //if the collision point is above the character's forehead
                this.playerCol.IsTouchingLayers(this.ladderMask) &&
                !this.isGrounded &&
                !Mathf.Approximately(this.ladderClimbSpeed, 0))
            {
                this.playerCol.isTrigger = true;
            }

            //Collision with hazards
            if (collision.collider.CompareTag("Hazard"))
            {
                if (this.playerAnim.GetBool("canWalk") && this.isGrounded)
                {
                    collision.collider.isTrigger = true;
                }
                else
                {
                    if (collision.relativeVelocity.x > 0)
                        this.playerRB.velocity = new Vector2(7.0f, 5.0f); //send the player a bit into the air backwards
                    else
                        this.playerRB.velocity = new Vector2(-7.0f, 5.0f);

                    this.PlayerHurting(); //this line is needed if I toggle back again that touching the enemies hurts the player

                    this.audioSource.PlayOneShot(this.hazardDeath);

                    this.gameObject.GetComponent<Health>().DealDamage(500);
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Hazard"))
        {
            if (!this.playerAnim.GetBool("canWalk") && !Mathf.Approximately(this.playerRB.velocity.sqrMagnitude, 0.0f))
                collision.isTrigger = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground") &&
           this.playerCol.isTrigger)
        {
            this.playerCol.isTrigger = false;
            this.isThereALadderBelow = false;
        }

        if (collision.CompareTag("Hazard"))
            collision.isTrigger = false;
    }


    //Animation events------------------------------------------------------------------------------------------
    public void ChangeIsAttacking()
    {
        this.isAttacking = !this.isAttacking;
    }
    
    public void SetIsGroundedToFalse()
    {
        this.playerAnim.SetBool("isGrounded", false);
    }

    public void FreezeForAttack()
    {
        this.freezeXForAttack = true;
    }

    public void UnfreezeChar()
    {
        this.freezeXForAttack = false;

        this.playerRB.constraints = RigidbodyConstraints2D.None;
        this.playerRB.freezeRotation = true;
    }

    public void SetFrictionToZeroAfterHurt()
    {
        this.playerCol.sharedMaterial.friction = 0.0f;
    }

    public void SpawnProjectile(ProjectileBehaviour inProjectile)
    {
        Vector2 spawnPosition;

        if (!this.hasPressedJumpButton && !this.playerAnim.GetBool("canCrouch"))
        {
            spawnPosition = new Vector2(this.gameObject.transform.position.x + (this.xStandingOffset * Mathf.Sign(this.gameObject.transform.localScale.x)),
                                        this.gameObject.transform.position.y);
        }
        else if(this.playerAnim.GetBool("canCrouch"))
        {
            spawnPosition = new Vector2(this.gameObject.transform.position.x + (this.xStandingOffset * Mathf.Sign(this.gameObject.transform.localScale.x)),
                                        this.gameObject.transform.position.y + this.yCrouchingOffset);
        }
        else
        {
            spawnPosition = new Vector2(this.gameObject.transform.position.x + (this.xJumpingOffset * Mathf.Sign(this.gameObject.transform.localScale.x)),
                                        this.gameObject.transform.position.y + this.yJumpingOffset);
        }

        ProjectileBehaviour projectile = Instantiate<ProjectileBehaviour>(inProjectile, spawnPosition, Quaternion.identity);
        this.audioSource.PlayOneShot(this.shurikenThrow);

        projectile.GetMovementDirection(Mathf.Sign(this.gameObject.transform.localScale.x), this.throwRange);
    }
}
