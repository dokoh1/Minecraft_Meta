using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerMove : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Rigidbody _rigidbody;
    private Camera _mainCamera;
    public GameObject player;
    
    private float _rotateX;
    private float _rotateY;
    private float _walkSpeed = 3f;
    private float _runSpeed = 6f;
    private float _mouseSpeed = 300f;
    private float _jumpForce = 4.5f;
    private float _playerHeight = 2f;
    private bool _mouseLockHide = true;
    
    void Start()
    {
        _mainCamera = Camera.main;
        _rigidbody = gameObject.GetComponent<Rigidbody>();
        // _rigidbody.linearDamping = 10f;
        _rigidbody.freezeRotation = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void Update()
    {
        if (_mouseLockHide)
        {
            Rotate();
            var grounded = Physics.Raycast(player.transform.position, Vector3.down, _playerHeight * 0.5f + 0.2f);
            if (Input.GetKeyDown(KeyCode.Space) && grounded)
                Jump();
        }

        SetCursorLock();
    }

    private void SetCursorLock()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_mouseLockHide)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                _mouseLockHide = false;
            }
            else if (!_mouseLockHide)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                _mouseLockHide = true;
            }
        }
    }
    
    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");
        Vector3 moveDir = (transform.forward * v + transform.right * h).normalized;
        if (h == 0 && v == 0)
            return;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            _rigidbody.position += moveDir * (_runSpeed * Time.deltaTime); 
        }
        else
            _rigidbody.position += moveDir * (_walkSpeed * Time.deltaTime);
    }
    
    private void Rotate()
    {
        float mouseX = Input.GetAxis("Mouse X") * _mouseSpeed * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSpeed * Time.deltaTime;
        _rotateX -= mouseY;
        _rotateY += mouseX;
        _rotateX = Mathf.Clamp(_rotateX, -90f, 90f);
        
        _mainCamera.transform.rotation = Quaternion.Euler(_rotateX, _rotateY, 0f);
        transform.rotation = Quaternion.Euler(0f, _rotateY, 0f);
    }


    private void Jump()
    {
        _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
    }
}
