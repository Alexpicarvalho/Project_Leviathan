using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Properties")]
    [SerializeField] private float _startMovementSpeed;
    [SerializeField] private float _maxMovementSpeed;
    [SerializeField] private float _runningSpeedMultiplier;
    [SerializeField] private float _stalkingSpeedMultiplier;
    public float _currentMovementSpeed;
    [SerializeField] private float _accelarationPerSecond;

    [Header("Jump Properties")]
    [SerializeField] float _jumpHeight;
    [SerializeField] float _jumpSpeed;
    [SerializeField] float _groundCheckRaycastDistance;
    [SerializeField] LayerMask _whatIsGround = ~0;
    [SerializeField] float _airControlPercentage = 1;

    [Header("General Properties")]
    [SerializeField] float _gravity;

    [Header("References / Necessities")]
    private CharacterController _playerController;

    [Header("Runtime Properties")]
    [SerializeField] public bool _isGrounded;
    public Vector3 _movementDirection;
    public bool _isRunning;
    public bool _isStalking;
    public bool _showGizmos;

    private void Awake()
    {
        _playerController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        CheckGrounded();
        if(new Vector2(_movementDirection.x, _movementDirection.z).magnitude >= 0.1f)
        {
            _currentMovementSpeed += _accelarationPerSecond * Time.deltaTime;
            _currentMovementSpeed = Mathf.Clamp(
                _currentMovementSpeed, 
                _startMovementSpeed, 
                _maxMovementSpeed
                );

        }
        else _currentMovementSpeed = _startMovementSpeed;
    }

    public void ProcessMovement(Vector2 input)
    {
        _movementDirection.x = input.x;
        _movementDirection.z = input.y;
        _movementDirection.y += _gravity * Time.deltaTime;
        float speed = CalculateSpeed();

        if (!_isGrounded)
        {
            _movementDirection.x *= _airControlPercentage;
            _movementDirection.z *= _airControlPercentage;
        }
        if(_isGrounded && _movementDirection.y < 0) _movementDirection.y = -10f;


        _playerController.Move(speed * Time.deltaTime * transform.TransformDirection(_movementDirection));
    }

    public float CalculateSpeed()
    {
        if (_isStalking) return _currentMovementSpeed * _stalkingSpeedMultiplier;
        else if (_isRunning) return _currentMovementSpeed * _runningSpeedMultiplier;
        else return _currentMovementSpeed;
    }

    public void IsRunning(bool isRunning)
    {
        if (isRunning) _isRunning = true;
        else _isRunning = false;
    }

    public void IsStalking(bool isStalking)
    {
        if (isStalking) _isStalking = true;
        else _isStalking = _isStalking = false;
    }

    public void Jump()
    {
        if(_isGrounded) _movementDirection.y = Mathf.Sqrt(_jumpHeight * -3.0f * _gravity);
    }

    private void CheckGrounded()
    {
        if (Physics.Raycast(transform.position, Vector3.down,out RaycastHit hit, _groundCheckRaycastDistance, _whatIsGround))
        {
            _isGrounded = true;
        } 
        else _isGrounded = false;

    }

    private void OnDrawGizmos()
    {
        if (!_showGizmos) return;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * _groundCheckRaycastDistance);
    }
}