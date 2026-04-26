using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody2D rb;
    public PlayerInput playerInput;
    public Animator anim;

    [Header("Movement Variables")]
    public float jumpForce;
    public float speed;
    public float jumpCutMultiplier = .5f;
    public float normalGravity;
    public float fallGravity;
    public float jumpGravity;
    public float sprintMultiplier = 1.5f;
    private bool isRunning;
    private bool isSliding;

    public int facingDirection = -1;

    //Inputs
    public Vector2 moveInput;
    private bool jumpPressed;
    private bool jumpReleased;

    [Header("GroundCheck")]
    public Transform groundCheck;
    public float groundCheckRadius;
    public LayerMask groundLayer;
    private bool isGrounded;

    private void Start()
    {
        rb.gravityScale = normalGravity;
    }

    void Update()
    {
        Flip();
        HandleAnimations();
    }

    void FixedUpdate()
    {
        CheckGrounded(); // ALWAYS FIRST, DONT FUCK WITH THIS - BIG DIK
        ApplyVariableGravity();
        HandleMovement();
        HandleJump();
    }

    private void HandleMovement()
    {
        float targetSpeed = moveInput.x * speed;
        rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
    }

    private void HandleJump()
    {
        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpPressed = false;
            jumpReleased = false;
        }
        if (jumpReleased)
        {
            if (rb.linearVelocity.y > 0) //still going up
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            }
            jumpReleased = false;
        }
    }

    void ApplyVariableGravity()
    {
        if(rb.linearVelocity.y < -0.1f) // falling
        {
            rb.gravityScale = fallGravity;
        }
        else if (rb.linearVelocity.y > 0.1f) // rising
        {
            rb.gravityScale = jumpGravity;
        }
        else
        {
            rb.gravityScale = normalGravity; // grounded
        }
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }
    void HandleAnimations()
    {
        anim.SetBool("isJumping", rb.linearVelocity.y > 0.1f && !isGrounded);
        anim.SetBool("isFalling", rb.linearVelocity.y < -0.1f && !isGrounded);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetBool("isIdle", Mathf.Abs(moveInput.x) < .1f && isGrounded);
        anim.SetBool("isWalking", Mathf.Abs(moveInput.x) > .1f && isGrounded);
    }

    void Flip()
    {
        if (moveInput.x > 0.1f)
        {
            facingDirection = -1;
        }
        else if (moveInput.x < -0.1f)
        {
            facingDirection = 1;
        }

        transform.localScale = new Vector3(facingDirection, 1, 1);
    }

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
        else //jump is released
        {
            jumpReleased = true;
        }
    }

    public void OnSprint(InputValue value)
    {
        if (value.isPressed)
        {
        isRunning = value.isPressed;
        Debug.Log("Sprint pressed: " + isRunning);
        }
        else
        {
            isRunning = false;
        }
    }

    public void OnSlide(InputValue value)
    {
        if (value.isPressed)
        {
            isSliding = value.isPressed;
            Debug.Log("Slide pressed: " + isSliding);
        }
        else
        {
            isSliding = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

}
