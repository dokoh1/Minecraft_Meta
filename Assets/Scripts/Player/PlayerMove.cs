using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerMove : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject player;
    public GameObject blockEffect;
    public GameObject blockPlaceEffect;
    public BlockTypeEnum blockType;

    private MinecraftTerrain _terrain;
    
    private Rigidbody _rigidbody;
    private Camera _mainCamera;
    private Ray ray;
    private RaycastHit hit;
    
    private float _rotateX;
    private float _rotateY;
    private float distance;
    private float _reach = 8f;
    private float _walkSpeed = 3f;
    private float _runSpeed = 6f;
    private float _mouseSpeed = 300f;
    private float _jumpForce = 10f;
    private float _playerHeight = 2f;
    private bool _mouseLockHide = true;
    private bool _isGravity = false;
    
    void Start()
    {
        _mainCamera = Camera.main;
        _rigidbody = gameObject.GetComponent<Rigidbody>();
        _terrain= GameObject.Find("Terrain").GetComponent<MinecraftTerrain>();
        blockType = BlockTypeEnum.Dirt;
        _rigidbody.freezeRotation = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _rigidbody.useGravity = false;
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

        BlockPutCheck();
        
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (_isGravity == false)
            {
                _rigidbody.useGravity = true;
                _isGravity = true;
            }
            else if (_isGravity == true)
            {
                _rigidbody.useGravity = false;
                _isGravity = false;
            }
        }

        SetCursorLock();
    }

    private void BlockPutCheck()
    {
        ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
        if (Physics.Raycast(ray, out hit, _reach))
        {
            Vector3 hitPosition  = hit.point;
            Vector3 hitNormal = hit.normal;
            blockEffect.transform.position = new Vector3(
                Mathf.FloorToInt(hitPosition.x - (hitNormal.x * 0.5f)), 
                Mathf.FloorToInt(hitPosition.y - (hitNormal.y * 0.5f)), 
                Mathf.FloorToInt(hitPosition.z - (hitNormal.z * 0.5f)));
            blockPlaceEffect.transform.position = blockEffect.transform.position + hitNormal;
            Vector3 playerInt = new Vector3(
                Mathf.FloorToInt(player.transform.position.x), 
                Mathf.FloorToInt(player.transform.position.y), 
                Mathf.FloorToInt(player.transform.position.z));
            distance = Vector3.Distance(playerInt, blockPlaceEffect.transform.position);
            blockEffect.SetActive(true);
            blockPlaceEffect.SetActive(true);
        }
        else
        {
            blockEffect.SetActive(false);
            blockPlaceEffect.SetActive(false);
        }

        if (blockEffect.gameObject.activeSelf)
        {

            if (Input.GetMouseButtonDown(0))
            {
                _terrain.Vector3ToChunk(blockEffect.transform.position).EditBlockInChunk(blockEffect.transform.position, BlockTypeEnum.Air);
            }

            if (Input.GetMouseButtonDown(1) && distance > 1f)
            {
                _terrain.Vector3ToChunk(blockPlaceEffect.transform.position).EditBlockInChunk(blockPlaceEffect.transform.position, blockType);
            }

        }
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
