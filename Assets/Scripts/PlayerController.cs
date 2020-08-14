﻿using System.Collections;
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
    private bool _canJump;
    private bool _iswallSliding;
    private int _amountOfJumpsLeft;
    private float _movementInputDirection; 


    //public
    public Transform groundCheck;
    public Transform wallCheck;
    public LayerMask whatIsGround;
    public int amountOfJumps = 1;
    public float movementSpeed = 10.0f;
    public float jumpForce = 16.0f;
    public float groundCheckRadius;
    public float wallCheckDistance;
    public float wallSlideSpeed;
    public float movementForceInAir;
    public float airDragMultiplier = 0.95f;
    public float variableJumpHeightMultiplier = 0.5f;


    
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _amountOfJumpsLeft = amountOfJumps;
    }
    
    void Update()
    {
        CheckInput();
        CheckMovementDirection();
        UpdateAnimations();
        CheckIfCanJump();
        CheckIfWallSliding();
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
        if(_isTouchingWall && !_isGrounded && _rb.velocity.y < 0)
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
            Jump();
        }

        if(Input.GetButtonUp("Jump"))
        {
            _rb.velocity = new Vector2(_rb.velocity.x, _rb.velocity.y * variableJumpHeightMultiplier);
        }
    }

    private void CheckIfCanJump()
    {
        //bugfix: trocar o valor 0 por 0.01
        if (_isGrounded && _rb.velocity.y <= 0.1)
        {
            _amountOfJumpsLeft = amountOfJumps;
        }
        
        if (_amountOfJumpsLeft <= 0)
        {
            _canJump = false;
        }
        
        else
        {
            _canJump = true;
        }
    }
   
    private void Jump()
    {
        if (_canJump)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
            _amountOfJumpsLeft--;
        }
    }

    private void ApplyMovement()
    {

        //DESATIVAR PRA FICAR MELHOR A MOVIMENTAÇÃO
        if (_isGrounded)
        {
            _rb.velocity = new Vector2(movementSpeed * _movementInputDirection, _rb.velocity.y);

        }
        else if (!_isGrounded && !_iswallSliding && _movementInputDirection != 0)
        {
            Vector2 forceToAdd = new Vector2(movementForceInAir * _movementInputDirection, 0);
            _rb.AddForce(forceToAdd);

            if(Mathf.Abs(_rb.velocity.x) > movementSpeed)
            {
                _rb.velocity = new Vector2(movementSpeed * _movementInputDirection, _rb.velocity.y);
            }
        }
        else if (!_isGrounded && !_iswallSliding && _movementInputDirection == 0)
        {
            _rb.velocity = new Vector2(_rb.velocity.x * airDragMultiplier, _rb.velocity.y);
        }

        //ATIVAR
        //_rb.velocity = new Vector2(movementSpeed * _movementInputDirection, _rb.velocity.y);

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
        if(!_iswallSliding)
        {
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
