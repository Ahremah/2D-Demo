using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public PlayerState currentState;

    public PlayerIdleState idleState;
    public PlayerJumpState jumpState;
    public PlayerMoveState moveState;
    public PlayerCrouchState crouchState;
    public PlayerSlideState slideState;

    [Header("Components")]
    public Rigidbody2D rb;
    public PlayerInput playerInput;
    public Animator anim;
    public CapsuleCollider2D playerCollider;

    [Header("Movement Variables")]
    public float jumpForce;
    public float sprintSpeed;
    public float walkSpeed;
    public float jumpCutMultiplier = .5f;
    public float normalGravity;
    public float fallGravity;
    public float jumpGravity;
    public float sprintMultiplier = 3.5f;

    private bool isRunning;
    private bool isSliding;

    public int facingDirection = -1;

    // Inputs 
    public Vector2 moveInput;
    public bool jumpPressed;
    public bool jumpReleased;
    public bool sprintPressed;

    [Header("Slide Settings")]
    public float slideDuration = .6f;
    public float slideSpeed;
    public float slideStopDuration = 0.15f;

    public float slideHeight;
    public Vector2 slideOffset;
    public float normalHeight;
    public Vector2 normalOffset;

    [Header("GroundCheck")]
    public Transform groundCheck;
    public float groundCheckRadius;
    public LayerMask groundLayer;

    public bool isGrounded;

    [Header("Crouch Check")]
    public Transform headCheck;
    public float headCheckRadius = 0.2f;
    public LayerMask headLayer;

    [Header("Footsteps")]
    public float walkStepRate = 0.5f;
    public float sprintStepRate = 0.3f;

    private float footstepTimer;

    private void Awake()
    {
        idleState = new PlayerIdleState(this);
        jumpState = new PlayerJumpState(this);
        moveState = new PlayerMoveState(this);
        crouchState = new PlayerCrouchState(this);
        slideState = new PlayerSlideState(this);
    }

    private void Start()
    {
        rb.gravityScale = normalGravity;
        ChangeState(idleState);
    }

    void Update()
    {
        currentState.Update();

        if (!isSliding)
            Flip();

        HandleAnimations();
        ResetFootstepsIfStopped();
        HandleFootsteps();
    }

    void FixedUpdate()
    {
        currentState.FixedUpdate();

        CheckGrounded();
        HandleSprint();
    }

    public void ChangeState(PlayerState newState)
    {
        if (currentState != null)
            currentState.Exit();

        currentState = newState;
        currentState.Enter();
    }

    public void SetColliderNormal()
    {
        playerCollider.size = new Vector2(playerCollider.size.x, normalHeight);
        playerCollider.offset = normalOffset;
    }

    public void SetColliderSlide()
    {
        playerCollider.size = new Vector2(playerCollider.size.x, slideHeight);
        playerCollider.offset = slideOffset;
    }

    // ---------------- ?? FOOTSTEPS ?? ----------------

    void HandleFootsteps()
    {
        if (!isGrounded) return;
        if (Mathf.Abs(moveInput.x) < 0.1f) return;

        float stepRate = isRunning ? sprintStepRate : walkStepRate;

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0f)
        {
            string step = isRunning ? "SprintStep" : "WalkStep";

            AudioManager.instance?.Play(step);

            // ?? IMPORTANT: reset based on movement start, not fixed value spam
            footstepTimer = stepRate;
        }
    }

    void ResetFootstepsIfStopped()
    {
        if (Mathf.Abs(moveInput.x) < 0.1f)
            footstepTimer = 0f;
    }

    // ---------------- MOVEMENT FLAGS ----------------

    private void HandleSprint()
    {
        if (isRunning && isGrounded)
        {
            sprintPressed = true;
        }
        else
        {
            sprintPressed = false;
        }
    }

    public void ApplyVariableGravity()
    {
        if (rb.linearVelocity.y < -0.1f)
            rb.gravityScale = fallGravity;
        else if (rb.linearVelocity.y > 0.1f)
            rb.gravityScale = jumpGravity;
        else
            rb.gravityScale = normalGravity;
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    public bool CheckForCeiling()
    {
        return Physics2D.OverlapCircle(
            headCheck.position,
            headCheckRadius,
            groundLayer
        );
    }

    void HandleAnimations()
    {
        anim.SetBool("isFalling", rb.linearVelocity.y < -0.1f && !isGrounded);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    void Flip()
    {
        if (moveInput.x > 0.1f)
            facingDirection = -1;
        else if (moveInput.x < -0.1f)
            facingDirection = 1;

        transform.localScale = new Vector3(facingDirection, 1, 1);
    }

    // ---------------- INPUT ----------------

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpPressed = true;
            jumpReleased = false;
        }
        else
        {
            jumpReleased = true;
        }
    }

    public void OnSprint(InputValue value)
    {
        isRunning = value.isPressed;
    }

    public void OnSlide(InputValue value)
    {
        isSliding = value.isPressed;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(headCheck.position, headCheckRadius);
    }
}