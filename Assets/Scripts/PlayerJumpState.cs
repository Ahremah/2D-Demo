using UnityEngine;

public class PlayerJumpState : PlayerState
{
    public PlayerJumpState(Player player) : base(player) { }

    public override void Enter()
    {
        base.Enter();

        anim.SetBool("isJumping", true);


        float jumpXBoost = player.moveInput.x * player.walkSpeed;

        player.rb.linearVelocity = new Vector2(
            jumpXBoost,
            player.jumpForce
        );

        JumpPressed = false;
        JumpReleased = false;
    }

    public override void Update()
    {
        base.Update();


        if (player.isGrounded && player.rb.linearVelocity.y <= 0)
        {
            player.ChangeState(player.idleState);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();


        player.ApplyVariableGravity();


        if (JumpReleased && player.rb.linearVelocity.y > 0)
        {
            player.rb.linearVelocity = new Vector2(
                player.rb.linearVelocity.x,
                player.rb.linearVelocity.y * player.jumpCutMultiplier
            );

            JumpReleased = false;
        }


        float speed = RunPressed ? player.sprintSpeed : player.walkSpeed;

        float airControl = player.rb.linearVelocity.y > 0 ? 0.05f : 0.15f;

        float airSpeedMultiplier = 1.2f;

        float targetSpeed = player.moveInput.x * speed * airSpeedMultiplier;

        float newX = Mathf.Lerp(
            player.rb.linearVelocity.x,
            targetSpeed,
            airControl
        );

        player.rb.linearVelocity = new Vector2(
            newX,
            player.rb.linearVelocity.y
        );
    }

    public override void Exit()
    {
        base.Exit();

        anim.SetBool("isJumping", false);
    }
}