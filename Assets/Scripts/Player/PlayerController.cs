using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    public GameObject player;
    public GameObject blockEffect;
    public GameObject blockPlaceEffect;
    public PlayerData playerData;
    public ToolBarManager2 toolbarManager;
    public MinecraftTerrain terrain;
    
    public bool inventoryLock = true;
    public bool mouseLockHide = true;
    public bool pasueLock = true;

    
    private Rigidbody _rigidbody;
    private Camera _mainCamera;
    private BlockTypeEnum _putBlockType;
    
    private Ray _ray;
    private RaycastHit _hit;
    
    private float _rotateX;
    private float _rotateY;
    private float _distance;
    private bool _isGravity;
    private bool _isGround;
    
    private void Start()
    {
        _mainCamera = Camera.main;
        _rigidbody = gameObject.GetComponent<Rigidbody>();
        _putBlockType = BlockTypeEnum.Air;
        _rigidbody.freezeRotation = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _rigidbody.useGravity = false;
    }
    
    private void Update()
    {
        if (mouseLockHide && inventoryLock)
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
        _putBlockType = toolbarManager.GetItemID();
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
        
        if (Physics.Raycast(_ray, out _hit, playerData.reach))
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
                terrain.Vector3ToChunk(blockEffect.transform.position).EditBlockInChunk(blockEffect.transform.position, BlockTypeEnum.Air);

            if (Input.GetMouseButtonDown(1) && _distance > 1f)
                terrain.Vector3ToChunk(blockPlaceEffect.transform.position).EditBlockInChunk(blockPlaceEffect.transform.position, _putBlockType);
        }
    }
    
    private void SetCursorLock()
    {
        if (!mouseLockHide)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (mouseLockHide)
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
            _rigidbody.position += moveDir * (playerData.runSpeed * Time.deltaTime); 
        else
            _rigidbody.position += moveDir * (playerData.walkSpeed * Time.deltaTime);
    }
    
    private void Rotate()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * playerData.mouseSpeed * terrain.inGameSetting.mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * playerData.mouseSpeed * terrain.inGameSetting.mouseSensitivity;
        
        _rotateX -= mouseY;
        _rotateY += mouseX;
        _rotateX = Mathf.Clamp(_rotateX, -90f, 90f);
        
        _mainCamera.transform.rotation = Quaternion.Euler(_rotateX, _rotateY, 0f);
        transform.rotation = Quaternion.Euler(0f, _rotateY, 0f);
    }
    
    private void JumpAndFly()
    {
        _isGround = Physics.Raycast(player.transform.position, Vector3.down, playerData.playerHeight * 0.5f + 0.2f);
        
        if (_isGravity)
        {
            if (_isGround && Input.GetKeyDown(KeyCode.Space))
                _rigidbody.AddForce(Vector3.up * playerData.jumpForce, ForceMode.Impulse);
        }
        else if (_isGravity == false)
        {
            if (Input.GetKey(KeyCode.Space))
                _rigidbody.position += Vector3.up * (playerData.flyForce * Time.deltaTime);
        }
    }
}
