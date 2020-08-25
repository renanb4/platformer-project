using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAfterImageSprite : MonoBehaviour
{
    [SerializeField]
    private float _activeTime = 0.1f;
    private float _timeActivated;
    private float _alpha;
    [SerializeField]
    private float _alphaSet = 0.8f;
    private float _alphaMultiplier = 0.85f;

    private Transform _player;

    private SpriteRenderer _SR;
    private SpriteRenderer _playerSR;

    private Color _color;

    private void OnEnable()
    {
        _SR = GetComponent<SpriteRenderer>();
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _playerSR = _player.GetComponent<SpriteRenderer>();

        _alpha = _alphaSet;
        _SR.sprite = _playerSR.sprite;
        transform.position = _player.position;
        transform.rotation = _player.rotation;
        _timeActivated = Time.time;
    }

    private void Update()
    {
        _alpha *= _alphaMultiplier;
        _color = new Color(1f, 1f, 1f, _alpha);
        _SR.color = _color;

        if(Time.time >= (_timeActivated + _activeTime))
        {
            PlayerAfterImagePool.Instance.AddToPool(gameObject);
        }

    }
}
