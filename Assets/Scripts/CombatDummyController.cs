using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatDummyController : MonoBehaviour
{
    [SerializeField]
    private float _maxHealth, _knockbackSpeedX, _knockbackSpeedY, _knockbackDuration, _knockbackDeathSpeedX, _knockbackDeathSpeedY, _deathTorque;
    [SerializeField]
    private bool _applyKnockback;
    [SerializeField]
    private GameObject _hitParticle;

    private float _currentHealth, _knockbackStart;

    private int _playerFacingDirection;

    private bool _playerOnLeft, _knockback;

    private PlayerController _pc;
    private GameObject _aliveGO, _brokenTopGO, _brokenBotGO;
    private Rigidbody2D _rbAlive, _rbBrokenTop, _rbBrokenBot;
    private Animator _aliveAnim;

    private void Start()
    {
        _currentHealth = _maxHealth;

        _pc = GameObject.Find("Player").GetComponent<PlayerController>();

        _aliveGO = transform.Find("Alive").gameObject;
        _brokenTopGO = transform.Find("Broken Top").gameObject;
        _brokenBotGO = transform.Find("Broken Bottom").gameObject;

        _aliveAnim = _aliveGO.GetComponent<Animator>();
        _rbAlive = _aliveGO.GetComponent<Rigidbody2D>();
        _rbBrokenTop = _brokenTopGO.GetComponent<Rigidbody2D>();
        _rbBrokenBot = _brokenBotGO.GetComponent<Rigidbody2D>();

        _aliveGO.SetActive(true);
        _brokenTopGO.SetActive(false);
        _brokenBotGO.SetActive(false);
    }

    private void Update()
    {
        CheckKnockback();
    }

    private void Damage(float amount)
    {
        _currentHealth -= amount;
        _playerFacingDirection = _pc.GetFacingDirection();

        Instantiate(_hitParticle, _aliveAnim.transform.position, Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)));

        if(_playerFacingDirection == 1)
        {
            _playerOnLeft = true;
        }
        else
        {
            _playerOnLeft = false;
        }

        _aliveAnim.SetBool("playerOnLeft", _playerOnLeft);
        _aliveAnim.SetTrigger("damage");

        if(_applyKnockback && _currentHealth > 0.0f)
        {
            //Knockback
            Knockback();
        }

        if(_currentHealth <= 0.0f)
        {
            //Die
            Die();
        }
    }

    private void Knockback()
    {
        _knockback = true;
        _knockbackStart = Time.time;
        _rbAlive.velocity = new Vector2(_knockbackSpeedX * _playerFacingDirection, _knockbackSpeedY);
    }

    private void CheckKnockback()
    {
        if(Time.time >= _knockbackStart + _knockbackDuration && _knockback)
        {
            _knockback = false;
            _rbAlive.velocity = new Vector2(0.0f, _rbAlive.velocity.y);
        }
    }

    private void Die()
    {
        _aliveGO.SetActive(false);
        _brokenTopGO.SetActive(true);
        _brokenBotGO.SetActive(true);

        _brokenTopGO.transform.position = _aliveGO.transform.position;
        _brokenBotGO.transform.position = _aliveGO.transform.position;

        _rbBrokenBot.velocity = new Vector2(_knockbackSpeedX * _playerFacingDirection, _knockbackSpeedY);
        _rbBrokenTop.velocity = new Vector2(_knockbackDeathSpeedX * _playerFacingDirection, _knockbackDeathSpeedY);
        _rbBrokenTop.AddTorque(_deathTorque * -_playerFacingDirection, ForceMode2D.Impulse);
    }
}
