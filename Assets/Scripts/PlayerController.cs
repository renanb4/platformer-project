using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //private
    private Rigidbody2D _rb;
    private Animator _anim;
    private bool _isFacingRight = true;
    private bool _isWalking;
    private bool _isGrounded;
    private bool _isTouchingWall;
    private bool _canNormalJump;
    private bool _canWallJump;
    private bool _iswallSliding;
    private bool _isAttemptingToJump;
    private bool _checkJumpMultiplier;
    private bool _canMove;
    private bool _canFlip;
    private bool _hasWallJumped;
    private int _amountOfJumpsLeft;
    private int _lastWallJumpDirection;
    private int _facingDirection = 1;
    private float _movementInputDirection;
    private float _jumpTimer;
    private float _turnTimer;
    private float _wallJumpTimer;


    //public
    public Transform groundCheck;
    public Transform wallCheck;
    public LayerMask whatIsGround;
    public Vector2 wallHopDirection;
    public Vector2 wallJumpDirection;
    public int amountOfJumps = 1;
    public float movementSpeed = 10.0f;
    public float jumpForce = 16.0f;
    public float groundCheckRadius;
    public float wallCheckDistance;
    public float wallSlideSpeed;
    public float movementForceInAir;
    public float airDragMultiplier = 0.95f;
    public float variableJumpHeightMultiplier = 0.5f;
    public float wallHopForce;
    public float wallJumpForce;
    public float jumpTimerSet = 0.15f;
    public float turnTimerSet = 0.1f;
    public float wallJumpTimerSet = 0.5f;


    
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _amountOfJumpsLeft = amountOfJumps;
        wallHopDirection.Normalize();
        wallJumpDirection.Normalize();
    }
    
    void Update()
    {
        CheckInput();
        CheckMovementDirection();
        UpdateAnimations();
        CheckIfCanJump();
        CheckIfWallSliding();
        CheckJump();
    }   

    private void FixedUpdate()
    {
        ApplyMovement();
        CheckSurroundings();
    }

    private void UpdateAnimations()
    {
        _anim.SetBool("isWalking", _isWalking);
        _anim.SetBool("isGrounded", _isGrounded);
        _anim.SetFloat("yVelocity", _rb.velocity.y);
        _anim.SetBool("isWallSliding", _iswallSliding);
    }

    private void CheckIfWallSliding()
    {
        if(_isTouchingWall && _movementInputDirection == _facingDirection && _rb.velocity.y < 0)
        {
            _iswallSliding = true;
        }
        else
        {
            _iswallSliding = false;
        }
    }

    private void CheckMovementDirection()
    {
        if(_isFacingRight && _movementInputDirection < 0)
        {
            Flip();
        }
        else if(!_isFacingRight && _movementInputDirection > 0)
        {
            Flip();
        }

        //bugfix: trocar if(_rb.velocity.x != 0) por if(_rb.velocity.x > 0.01f || _rb.velocity.x < -0.01f)
        if(_rb.velocity.x > 0.01f || _rb.velocity.x < -0.01f)
        {
            _isWalking = true;
        }
        else
        {
            _isWalking = false;
        }
    }

    private void CheckSurroundings()
    {
        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
        _isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);
    }

    private void CheckInput()
    {
        _movementInputDirection = Input.GetAxisRaw("Horizontal");

        if(Input.GetButtonDown("Jump"))
        {
            if(_isGrounded || (_amountOfJumpsLeft > 0 && !_isTouchingWall))
            {
                NormalJump();
            }
            else
            {
                _jumpTimer = jumpTimerSet;
                _isAttemptingToJump = true;
            }
        }

        if(Input.GetButtonDown("Horizontal") && _isTouchingWall)
        {
            if(!_isGrounded && _movementInputDirection != _facingDirection)
            {
                _canMove = false;
                _canFlip = false;

                _turnTimer = turnTimerSet;
            }
        }

        if(!_canMove)
        {
            _turnTimer -= Time.deltaTime;

            if(_turnTimer <= 0)
            {
                _canMove = true;
                _canFlip = true;
            }
        }

        if(_checkJumpMultiplier && !Input.GetButton("Jump"))
        {
            _checkJumpMultiplier = false;
            _rb.velocity = new Vector2(_rb.velocity.x, _rb.velocity.y * variableJumpHeightMultiplier);
        }
    }

    private void CheckIfCanJump()
    {
        //bugfix: trocar o valor 0 por 0.01
        if (_isGrounded && _rb.velocity.y <= 0.1f)
        {
            _amountOfJumpsLeft = amountOfJumps;
        }
        
        if (_isTouchingWall)
        {
            _canWallJump = true;
        }

        if (_amountOfJumpsLeft <= 0)
        {
            _canNormalJump = false;
        }
        
        else
        {
            _canNormalJump = true;
        }
    }
   
    private void CheckJump()
    {
        if(_jumpTimer > 0)
        {
            //WallJump
            if(!_isGrounded && _isTouchingWall && _movementInputDirection != 0 && _movementInputDirection != _facingDirection)
            {
                WallJump();
            }
            else if(_isGrounded)
            {
                NormalJump();
            }
        }
        if(_isAttemptingToJump)
        {
            _jumpTimer -= Time.deltaTime;
        }

        if(_wallJumpTimer > 0)
        {
            if(_hasWallJumped && _movementInputDirection == -_lastWallJumpDirection)
            {
                _rb.velocity = new Vector2(_rb.velocity.x, 0.0f);
                _hasWallJumped = false;
            }
            else if(_wallJumpTimer <= 0)
            {
                _hasWallJumped = false;
            }
            else
            {
                _wallJumpTimer -= Time.deltaTime;
            }
        }
    }

    private void NormalJump()
    {
        if (_canNormalJump && !_iswallSliding)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
            _amountOfJumpsLeft--;
            _jumpTimer = 0;
            _isAttemptingToJump = false;
            _checkJumpMultiplier = true;
        }
    }

    private void WallJump()
    {
        if(_canWallJump)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, 0.0f);
            _iswallSliding = false;
            _amountOfJumpsLeft = amountOfJumps;
            _amountOfJumpsLeft--;
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * _movementInputDirection, wallJumpForce * wallJumpDirection.y);
            _rb.AddForce(forceToAdd, ForceMode2D.Impulse);
            _jumpTimer = 0;
            _isAttemptingToJump = false;
            _checkJumpMultiplier = true;
            _turnTimer = 0;
            _canMove = true;
            _canFlip = true;
            _hasWallJumped = true;
            _wallJumpTimer = wallJumpTimerSet;
            _lastWallJumpDirection = - _facingDirection;
        }
    }

    private void ApplyMovement()
    {

        if(!_isGrounded && !_iswallSliding && _movementInputDirection == 0)
        {
            _rb.velocity = new Vector2(_rb.velocity.x * airDragMultiplier, _rb.velocity.y);
        }
        else if(_canMove)
        {
            _rb.velocity = new Vector2(movementSpeed * _movementInputDirection, _rb.velocity.y);

        }
                
        if(_iswallSliding)
        {
            if(_rb.velocity.y < - wallSlideSpeed)
            {
                _rb.velocity = new Vector2(_rb.velocity.x, -wallSlideSpeed);
            }
        }
    }

    private void Flip()
    {
        if(!_iswallSliding  && _canFlip)
        {
            _facingDirection *= -1;
            _isFacingRight = !_isFacingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.DrawLine(wallCheck. position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
    }

}
