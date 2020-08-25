using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private float _movementInputDirection;
    private float _jumpTimer;
    private float _turnTimer;
    private float _wallJumpTimer;
    private float _dashTimeLeft;
    private float _lastImageXpos;
    private float _lastDash = -100f;

    private int _amountOfJumpsLeft;
    private int _facingDirection = 1;
    private int _lastWallJumpDirection;

    private bool _isFacingRight = true;
    private bool _isWalking;
    private bool _isGrounded;
    private bool _isTouchingWall;
    private bool _isWallSliding;
    private bool _canNormalJump;
    private bool _canWallJump;
    private bool _isAttemptingToJump;
    private bool _checkJumpMultiplier;
    private bool _canMove;
    private bool _canFlip;
    private bool _hasWallJumped;
    private bool _isTouchingLedge;
    private bool _canClimbLedge = false;
    private bool _ledgeDetected;
    private bool _isDashing;

    private Vector2 _ledgePosBot;
    private Vector2 _ledgePos1;
    private Vector2 _ledgePos2;

    private Rigidbody2D _rb;
    private Animator _anim;

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
    public float ledgeClimbXOffset1 = 0f;
    public float ledgeClimbYOffset1 = 0f;
    public float ledgeClimbXOffset2 = 0f;
    public float ledgeClimbYOffset2 = 0f;
    public float dashTime;
    public float dashSpeed;
    public float distanceBetweenImages;
    public float dashCoolDown;

    public Vector2 wallHopDirection;
    public Vector2 wallJumpDirection;

    public Transform groundCheck;
    public Transform wallCheck;
    public Transform ledgeCheck;

    public LayerMask whatIsGround;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _amountOfJumpsLeft = amountOfJumps;
        wallHopDirection.Normalize();
        wallJumpDirection.Normalize();
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
        CheckMovementDirection();
        UpdateAnimations();
        CheckIfCanJump();
        CheckIfWallSliding();
        CheckJump();
        CheckLedgeClimb();
        CheckDash();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        CheckSurroundings();
    }

    private void CheckIfWallSliding()
    {
        if (_isTouchingWall && _movementInputDirection == _facingDirection && _rb.velocity.y < 0 && !_canClimbLedge)
        {
            _isWallSliding = true;
        }
        else
        {
            _isWallSliding = false;
        }
    }

    private void CheckLedgeClimb()
    {
        if(_ledgeDetected && !_canClimbLedge)
        {
            _canClimbLedge = true;

            if (_isFacingRight)
            {
                _ledgePos1 = new Vector2(Mathf.Floor(_ledgePosBot.x + wallCheckDistance) - ledgeClimbXOffset1, Mathf.Floor(_ledgePosBot.y) + ledgeClimbYOffset1);
                _ledgePos2 = new Vector2(Mathf.Floor(_ledgePosBot.x + wallCheckDistance) + ledgeClimbXOffset2, Mathf.Floor(_ledgePosBot.y) + ledgeClimbYOffset2);
            }
            else
            {
                _ledgePos1 = new Vector2(Mathf.Ceil(_ledgePosBot.x - wallCheckDistance) + ledgeClimbXOffset1, Mathf.Floor(_ledgePosBot.y) + ledgeClimbYOffset1);
                _ledgePos2 = new Vector2(Mathf.Ceil(_ledgePosBot.x - wallCheckDistance) - ledgeClimbXOffset2, Mathf.Floor(_ledgePosBot.y) + ledgeClimbYOffset2);
            }

            _canMove = false;
            _canFlip = false;

            _anim.SetBool("canClimbLedge", _canClimbLedge);
        }

        if (_canClimbLedge)
        {
            transform.position = _ledgePos1;
        }
    }

    public void FinishLedgeClimb()
    {
        _canClimbLedge = false;
        transform.position = _ledgePos2;
        _canMove = true;
        _canFlip = true;
        _ledgeDetected = false;
        _anim.SetBool("canClimbLedge", _canClimbLedge);
    }

    private void CheckSurroundings()
    {
        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        _isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);
        _isTouchingLedge = Physics2D.Raycast(ledgeCheck.position, transform.right, wallCheckDistance, whatIsGround);

        if(_isTouchingWall && !_isTouchingLedge && !_ledgeDetected)
        {
            _ledgeDetected = true;
            _ledgePosBot = wallCheck.position;
        }
    }

    private void CheckIfCanJump()
    {
        if(_isGrounded && _rb.velocity.y <= 0.01f)
        {
            _amountOfJumpsLeft = amountOfJumps;
        }

        if (_isTouchingWall)
        {
            _checkJumpMultiplier = false;
            _canWallJump = true;
        }

        if(_amountOfJumpsLeft <= 0)
        {
            _canNormalJump = false;
        }
        else
        {
            _canNormalJump = true;
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

        if(Mathf.Abs(_rb.velocity.x) >= 0.01f)
        {
            _isWalking = true;
        }
        else
        {
            _isWalking = false;
        }
    }

    private void UpdateAnimations()
    {
        _anim.SetBool("isWalking", _isWalking);
        _anim.SetBool("isGrounded", _isGrounded);
        _anim.SetFloat("yVelocity", _rb.velocity.y);
        _anim.SetBool("isWallSliding", _isWallSliding);
    }

    private void CheckInput()
    {
        _movementInputDirection = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
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

        if (_turnTimer >= 0)
        {
            _turnTimer -= Time.deltaTime;

            if(_turnTimer <= 0)
            {
                _canMove = true;
                _canFlip = true;
            }
        }

        if (_checkJumpMultiplier && !Input.GetButton("Jump"))
        {
            _checkJumpMultiplier = false;
            _rb.velocity = new Vector2(_rb.velocity.x, _rb.velocity.y * variableJumpHeightMultiplier);
        }

        if (Input.GetButtonDown("Dash"))
        {
            if(Time.time >= (_lastDash + dashCoolDown))
            AttemptToDash();
        }

    }

    private void AttemptToDash()
    {
        _isDashing = true;
        _dashTimeLeft = dashTime;
        _lastDash = Time.time;

        PlayerAfterImagePool.Instance.GetFromPool();
        _lastImageXpos = transform.position.x;
    }

    public int GetFacingDirection()
    {
        return _facingDirection;
    }

    private void CheckDash()
    {
        if (_isDashing)
        {
            if(_dashTimeLeft > 0)
            {
                _canMove = false;
                _canFlip = false;
                _rb.velocity = new Vector2(dashSpeed * _facingDirection, 0.0f);
                _dashTimeLeft -= Time.deltaTime;

                if (Mathf.Abs(transform.position.x - _lastImageXpos) > distanceBetweenImages)
                {
                    PlayerAfterImagePool.Instance.GetFromPool();
                    _lastImageXpos = transform.position.x;
                }
            }

            if(_dashTimeLeft <= 0 || _isTouchingWall)
            {
                _isDashing = false;
                _canMove = true;
                _canFlip = true;
            }
            
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
            else if (_isGrounded)
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
            }else if(_wallJumpTimer <= 0)
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
        if (_canNormalJump)
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
        if (_canWallJump)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, 0.0f);
            _isWallSliding = false;
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
            _lastWallJumpDirection = -_facingDirection;

        }
    }

    private void ApplyMovement()
    {

        if (!_isGrounded && !_isWallSliding && _movementInputDirection == 0)
        {
            _rb.velocity = new Vector2(_rb.velocity.x * airDragMultiplier, _rb.velocity.y);
        }
        else if(_canMove)
        {
            _rb.velocity = new Vector2(movementSpeed * _movementInputDirection, _rb.velocity.y);
        }
        

        if (_isWallSliding)
        {
            if(_rb.velocity.y < -wallSlideSpeed)
            {
                _rb.velocity = new Vector2(_rb.velocity.x, -wallSlideSpeed);
            }
        }
    }

    public void DisableFlip()
    {
        _canFlip = false;
    }

    public void EnableFlip()
    {
        _canFlip = true;
    }

    private void Flip()
    {
        if (!_isWallSliding && _canFlip)
        {
            _facingDirection *= -1;
            _isFacingRight = !_isFacingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
    }
}
