using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{
    [SerializeField]
    private bool _combatEnabled;
    [SerializeField]
    private float _inputTimer, _attack1Radius, _attack1Damage;
    [SerializeField]
    private Transform _attack1HitBoxPos;
    [SerializeField]
    private LayerMask _whatIsDamageable;
    
    private bool _gotInput, _isAttacking, _isFirstAttack;

    private float _lastInputTime = Mathf.NegativeInfinity;

    private Animator _anim;

    private void Start()
    {
        _anim = GetComponent<Animator>();
        _anim.SetBool("canAttack", _combatEnabled);
    }

    private void Update()
    {
        CheckCombatInput();
        CheckAttacks();
    }

    private void CheckCombatInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (_combatEnabled)
            {
                //Attempt combat
                _gotInput = true;
                _lastInputTime = Time.time;
            }
        }
    }

    private void CheckAttacks()
    {
        if (_gotInput)
        {
            //Perform Attack1
            if (!_isAttacking)
            {
                _gotInput = false;
                _isAttacking = true;
                _isFirstAttack = !_isFirstAttack;
                _anim.SetBool("attack1", true);
                _anim.SetBool("firstAttack", _isFirstAttack);
                _anim.SetBool("isAttacking", _isAttacking);
            }
        }

        if(Time.time >= _lastInputTime + _inputTimer)
        {
            //Wait for new input
            _gotInput = false;
        }
    }

    private void CheckAttackHitBox()
    {
        Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(_attack1HitBoxPos.position, _attack1Radius, _whatIsDamageable);

        foreach (Collider2D collider in detectedObjects)
        {
            collider.transform.parent.SendMessage("Damage", _attack1Damage);
            //Instantiate hit particle
        }
    }

    private void FinishAttack1()
    {
        _isAttacking = false;
        _anim.SetBool("isAttacking", _isAttacking);
        _anim.SetBool("attack1", false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(_attack1HitBoxPos.position, _attack1Radius);
    }

}
