using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
public class PlayerMove : MonoBehaviour
{
    public GameObject player;
    public GameObject blockEffect;
    public GameObject blockPlaceEffect;
    public BlockTypeEnum blockType;
    public bool _inventoryLock = true;
    public bool _mouseLockHide = true;
    public bool PasueLock = true;

    public ToolBarManager2 toolbarManager;
    private MinecraftTerrain _terrain;
    private Rigidbody _rigidbody;
    private Camera _mainCamera;
    
    private Ray _ray;
    private RaycastHit _hit;
    
    //정적 데이터
    
    private float _reach = 8f;
    private float _walkSpeed = 3f;
    private float _runSpeed = 6f;
    private float _mouseSpeed = 3f;
    private float _jumpForce = 10f;
    private float _flyForce = 6f;
    private float _playerHeight = 2f;
    
    //가변 데이터
    private float _rotateX;
    private float _rotateY;
    private float _distance;
    private bool _isGravity = false;
    private bool _isGround;
    
    private void Start()
    {
        _mainCamera = Camera.main;
        _rigidbody = gameObject.GetComponent<Rigidbody>();
        _terrain= GameObject.Find("Terrain").GetComponent<MinecraftTerrain>();
        blockType = BlockTypeEnum.Air;
        _rigidbody.freezeRotation = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _rigidbody.useGravity = false;
    }
    
    private void Update()
    {
        if (_mouseLockHide && _inventoryLock)
        {
            Rotate();
            JumpAndFly();
        }
        BlockTypeSet();
        BlockPutCheck();
        
        GravitySet();
        SetCursorLock();
    }

    private void BlockTypeSet()
    {
        blockType = toolbarManager.GetItemID();
    }
    
    private void GravitySet()
    {
        if (Input.GetKeyDown(KeyCode.F2))
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
    }
    
    private void BlockPutCheck()
    {
        _ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
        
        if (Physics.Raycast(_ray, out _hit, _reach))
        {
            Vector3 hitPosition  = _hit.point;
            Vector3 hitNormal = _hit.normal;
            
            blockEffect.transform.position = new Vector3(
                Mathf.FloorToInt(hitPosition.x - (hitNormal.x * 0.5f)), 
                Mathf.FloorToInt(hitPosition.y - (hitNormal.y * 0.5f)), 
                Mathf.FloorToInt(hitPosition.z - (hitNormal.z * 0.5f)));
            
            blockPlaceEffect.transform.position = blockEffect.transform.position + hitNormal;
            Vector3 playerInt = new Vector3(
                Mathf.FloorToInt(player.transform.position.x), 
                Mathf.FloorToInt(player.transform.position.y), 
                Mathf.FloorToInt(player.transform.position.z));
            
            _distance = Vector3.Distance(playerInt, blockPlaceEffect.transform.position);
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
                _terrain.Vector3ToChunk(blockEffect.transform.position).EditBlockInChunk(blockEffect.transform.position, BlockTypeEnum.Air);

            if (Input.GetMouseButtonDown(1) && _distance > 1f)
                _terrain.Vector3ToChunk(blockPlaceEffect.transform.position).EditBlockInChunk(blockPlaceEffect.transform.position, blockType);
        }
    }
    
    private void SetCursorLock()
    {
        if (!_mouseLockHide)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (_mouseLockHide)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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
            _rigidbody.position += moveDir * (_runSpeed * Time.deltaTime); 
        else
            _rigidbody.position += moveDir * (_walkSpeed * Time.deltaTime);
    }
    
    private void Rotate()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * _mouseSpeed * _terrain.setting.mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * _mouseSpeed * _terrain.setting.mouseSensitivity;
        
        _rotateX -= mouseY;
        _rotateY += mouseX;
        _rotateX = Mathf.Clamp(_rotateX, -90f, 90f);
        
        _mainCamera.transform.rotation = Quaternion.Euler(_rotateX, _rotateY, 0f);
        transform.rotation = Quaternion.Euler(0f, _rotateY, 0f);
    }
    
    private void JumpAndFly()
    {
        _isGround = Physics.Raycast(player.transform.position, Vector3.down, _playerHeight * 0.5f + 0.2f);
        
        if (_isGravity == true)
        {
            if (_isGround == true && Input.GetKeyDown(KeyCode.Space))
                _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }
        else if (_isGravity == false)
        {
            if (Input.GetKey(KeyCode.Space))
                _rigidbody.position += Vector3.up * (_flyForce * Time.deltaTime);
        }
    }
}
