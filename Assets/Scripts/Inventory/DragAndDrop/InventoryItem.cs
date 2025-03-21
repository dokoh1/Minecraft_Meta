using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

//마우스 이벤트를 감지
//using UnityEngine.EventSystems;
//using UnityEngine.UI;
// /*
// 이 스크립트는 아이템 개별관리, 드래그 및 드롭을 지원한다.
// 인벤토리 슬롯 안에 들어갈 아이템 개체를 나타내며, 드래그와 드롭이 가능한 구조이다.
// 주요역할: 아이템 정보를 담고 있고, UI Image로 아이템 이미지를 표시한다.
// InitializeItem()을 통해 아이템 데이터를 설정
// 드래그 & 드롭 구현: IBeginDragHandler, IDragHandler, IEndDragHandler
// 이 인터페이스를 구현하면 Unity의 UI 시스템에서 마우스 이벤트를 받을 수 있도록 자동 등록된다.
// 마우스 클릭 및 드래그는 Unity의 EventSystem과 Graphic Raycaster를 통해 감지된다. 
//
// */
//
// 드래그 앤 드롭 두번째 코드(복사한 이미지 드래그 가능)
/*
public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //필드 변수
    [Header("UI")]
    //UI에서 아이템의 이미지를 담당하는 변수
    //Image는 Unity의 ui요소로 아이템의 아이콘을 표시하는데 사용됨.(원본 아이템 이미지)
    public Image image;

    public GameObject copyItem;
    private Image copyImage;
    private Transform originalParent;
    
    //아이템이 원래 속해 있던 부모(슬롯)을 저장하는 변수.
    //HideInInspector -> Unity의 인스펙터에서 보이지 않도록 설정(코드에서는 접근 가능)
    [HideInInspector] public Transform parentAfterDrag; // 부모 저장 변수
    
    //Start()는 유니티에서 객체가 활성화될 때 한 번 호출됨.
    public void Awake()
    {
        //raycastTarget이 true이면 이 UI이미지가 마우스 클릭을 받을 수 있음. false면 클릭이 무시됨.
        //UI 이미지가 마우스 클릭을 받을 수 있도록 설정.
        image.raycastTarget = true;
    }
    

    //마우스를 클릭하고 드래그를 시작할 때 호출.
    //PointerEventData란? 마우스 클릭,터치 등의 입력 이벤트 정보를 포함하는 데이터 구조체.
    //eventData.position은 현재 마우스 커서의 위치
    public void OnBeginDrag(PointerEventData eventData)
    {
        //드래그 중에는 이미지가 클릭을 감지하지 않도록 설정.(이렇게 하면 UI드래그가 원활하게 이뤄짐)
        // image.raycastTarget = false;
        //현재 부모(슬롯)를 저장 -> 드래그가 끝나면 다시 원래 자리로 돌아오기 위해 필요.
        // parentAfterDrag = transform.parent;
        //드래그할 때 부모를 root로 설정하여 캔버스 맨 위로 이동.
        //이렇게 하면 드래그하는 아이템이 다른 UI 위에 표시됨.
        originalParent = transform.parent;
        //transform.SetParent(transform.root);
        //Debug.Log(eventData.position);
        // 아이템 복제하여 드래그 가능하도록 설정
        
        copyItem = Instantiate(gameObject, transform.position, Quaternion.identity,transform.root);
        copyImage = copyItem.GetComponent<Image>();
        
        //GameObject copyObject = Instantiate(gameObject, transform.position,Quaternion.identity,transform.root);
       
        //copyItem.GetComponent<Image>().raycastTarget = false;
        
        //copyItem.GetComponent<InventoryItem>().originalParent = null;

        if (copyImage != null)
        {
            copyImage.sprite = image.sprite;
            copyImage.raycastTarget = false;
            
            copyItem.transform.SetParent(transform.root, true);
            copyItem.transform.SetAsLastSibling();

            //copyItem.transform.localScale = Vector3.one;
            //RectTransform rect = copyItem.GetComponent<RectTransform>();
            //rect.sizeDelta = new Vector2(40,40);
            
            
            //Color tempColor = copyImage.color;
            //tempColor.a = 1f;
            //copyImage.color = tempColor;
            
            copyImage.raycastTarget = true;
            //Debug.Log($"드래그 시작 - copyItem 위치: {copyItem.transform.position}, 크기: {rect.sizeDelta}, alpha: {copyImage.color.a}");
        }
        
        //디버그
        if (copyItem.GetComponent<Image>()== true)
        {
            Debug.Log(copyItem.GetComponent<Image>().sprite.name);
            Debug.Log("아이콘 이미지가 복사되었습니다.");
        }
    }
    
    // public void StartDrag()
    // {
    //     copyItem.transform.SetParent(transform.root);
    //     transform.SetParent(parentAfterDrag);
    // }
    //
    //드래그 중 마우스가 움직일 때마다 계속 호출
    public void OnDrag(PointerEventData eventData)
    {
        //parentAfterDrag = transform.parent;
        //아이템의 위치를 마우스 커서 위치로 변경 -> 따라서 마우스가 움직이면 아이템도 따라옴.
        if (copyItem != null)
        {
            copyItem.transform.position = Input.mousePosition;
            Debug.Log($"드래그 중: {copyItem.transform.position}"); // 디버그 메시지 추가
        }
    }

    //마우스 버튼을 놓을 때 호출.(드래그 종료)
    public void OnEndDrag(PointerEventData eventData)
    {
        GameObject dropTarget = eventData.pointerEnter;
        if (dropTarget != null)
        {
            // 부모 계층에서 InventorySlot 탐색
            InventorySlot slot = dropTarget.GetComponentInParent<InventorySlot>();

            if (slot != null)
            {
                copyItem.transform.SetParent(slot.transform);
                parentAfterDrag = slot.transform;
                Debug.Log($"드롭 성공! 슬롯 이름: {slot.name}");
            }
            else
            {
                copyItem.transform.SetParent(originalParent);
                Debug.Log("드롭한 위치에 InventorySlot 컴포넌트를 가진 부모가 없음. 원래 부모로 복귀.");
            }
        }
        else
        {
            copyItem.transform.SetParent(originalParent);
            Debug.Log("드롭 대상 자체가 없음. 원래 부모로 복귀.");
        }
//         
//         GameObject dropTarget = eventData.pointerEnter;
//
//         if (dropTarget != null && dropTarget.GetComponent<InventorySlot>())
//         {
//             copyItem.transform.SetParent(dropTarget.transform);
//             parentAfterDrag = dropTarget.transform;
//             Debug.Log($"드롭 성공! 새로운 부모: {dropTarget.name}");
//         }
//         else
//         {
//             copyItem.transform.SetParent(originalParent);
//             Debug.Log("드롭한 위치가 슬롯이 아님. 원래 부모로 복귀.");
//         }
//         
//         copyItem.GetComponent<Image>().raycastTarget = true;  
//         
    }
    
    
}
*/

//이미지 복사해서 다른 슬롯 옮기는 것까지는 되는 코드

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //필드 변수
    [Header("UI")]
    //UI에서 아이템의 이미지를 담당하는 변수
    //Image는 Unity의 ui요소로 아이템의 아이콘을 표시하는데 사용됨.(원본 아이템 이미지)
    public Image image;

    public GameObject copyItem;
    private Image copyImage;
    private Transform originalParent;
    
    //아이템이 원래 속해 있던 부모(슬롯)을 저장하는 변수.
    //HideInInspector -> Unity의 인스펙터에서 보이지 않도록 설정(코드에서는 접근 가능)
    [HideInInspector] public Transform parentAfterDrag; // 부모 저장 변수
    
    //Start()는 유니티에서 객체가 활성화될 때 한 번 호출됨.
    public void Awake()
    {
        //raycastTarget이 true이면 이 UI이미지가 마우스 클릭을 받을 수 있음. false면 클릭이 무시됨.
        //UI 이미지가 마우스 클릭을 받을 수 있도록 설정.
        image.raycastTarget = true;
    }
    

    //마우스를 클릭하고 드래그를 시작할 때 호출.
    //PointerEventData란? 마우스 클릭,터치 등의 입력 이벤트 정보를 포함하는 데이터 구조체.
    //eventData.position은 현재 마우스 커서의 위치
    public void OnBeginDrag(PointerEventData eventData)
    {
        //드래그 중에는 이미지가 클릭을 감지하지 않도록 설정.(이렇게 하면 UI드래그가 원활하게 이뤄짐)
        // image.raycastTarget = false;
        //현재 부모(슬롯)를 저장 -> 드래그가 끝나면 다시 원래 자리로 돌아오기 위해 필요.
        // parentAfterDrag = transform.parent;
        //드래그할 때 부모를 root로 설정하여 캔버스 맨 위로 이동.
        //이렇게 하면 드래그하는 아이템이 다른 UI 위에 표시됨.
        originalParent = transform.parent;
        //transform.SetParent(transform.root);
        //Debug.Log(eventData.position);
        // 아이템 복제하여 드래그 가능하도록 설정
        
        copyItem = Instantiate(gameObject, transform.position, Quaternion.identity, transform.root);
        var originalRT = transform as RectTransform;
        var rt = copyItem.transform as RectTransform;
        rt.sizeDelta = originalRT.sizeDelta;

        copyImage = copyItem.GetComponent<Image>();
        //GameObject copyObject = Instantiate(gameObject, transform.position,Quaternion.identity,transform.root);
       
        //copyItem.GetComponent<Image>().raycastTarget = false;
        
        //copyItem.GetComponent<InventoryItem>().originalParent = null;

        if (copyItem != null)
        {
            copyImage.sprite = image.sprite;
            Debug.Log($"복사된 이미지의 스프라이트 설정 완료: {copyImage.sprite.name}");
            copyImage.raycastTarget = false;
            
            copyItem.transform.SetParent(transform.root, true);
        }
        
        //디버그
        if (copyItem.GetComponent<Image>()== true)
        {
            Debug.Log(copyItem.GetComponent<Image>().sprite.name);
            Debug.Log("아이콘 이미지가 복사되었습니다.");
        }
    }
    
    // public void StartDrag()
    // {
    //     copyItem.transform.SetParent(transform.root);
    //     transform.SetParent(parentAfterDrag);
    // }
    //
    //드래그 중 마우스가 움직일 때마다 계속 호출
    public void OnDrag(PointerEventData eventData)
    {
        //parentAfterDrag = transform.parent;
        //아이템의 위치를 마우스 커서 위치로 변경 -> 따라서 마우스가 움직이면 아이템도 따라옴.
        if (copyItem != null)
        {
            copyItem.transform.position = Input.mousePosition;
        }
    }

    //마우스 버튼을 놓을 때 호출.(드래그 종료)
    public void OnEndDrag(PointerEventData eventData)
    {
        
        GameObject dropTarget = eventData.pointerEnter;

        if (dropTarget != null && dropTarget.GetComponent<InventorySlot>())
        {
            copyItem.transform.SetParent(dropTarget.transform);
            parentAfterDrag = dropTarget.transform;
        }
        else
        {
            copyItem.transform.SetParent(originalParent);
        }
        
        copyItem.GetComponent<Image>().raycastTarget = true;  
    }
    
}



/*
public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //필드 변수
    [Header("UI")]
    //UI에서 아이템의 이미지를 담당하는 변수
    //Image는 Unity의 ui요소로 아이템의 아이콘을 표시하는데 사용됨.(원본 아이템 이미지)
    public Image image;

    public GameObject copyItem;
    private Transform originalParent;
    
    //아이템이 원래 속해 있던 부모(슬롯)을 저장하는 변수.
    //HideInInspector -> Unity의 인스펙터에서 보이지 않도록 설정(코드에서는 접근 가능)
    [HideInInspector] public Transform parentAfterDrag; // 부모 저장 변수
    
    //Start()는 유니티에서 객체가 활성화될 때 한 번 호출됨.
    public void Awake()
    {
        //raycastTarget이 true이면 이 UI이미지가 마우스 클릭을 받을 수 있음. false면 클릭이 무시됨.
        //UI 이미지가 마우스 클릭을 받을 수 있도록 설정.
        image.raycastTarget = true;
    }
    

    //마우스를 클릭하고 드래그를 시작할 때 호출.
    //PointerEventData란? 마우스 클릭,터치 등의 입력 이벤트 정보를 포함하는 데이터 구조체.
    //eventData.position은 현재 마우스 커서의 위치
    public void OnBeginDrag(PointerEventData eventData)
    {
        //드래그 중에는 이미지가 클릭을 감지하지 않도록 설정.(이렇게 하면 UI드래그가 원활하게 이뤄짐)
        // image.raycastTarget = false;
        //현재 부모(슬롯)를 저장 -> 드래그가 끝나면 다시 원래 자리로 돌아오기 위해 필요.
        // parentAfterDrag = transform.parent;
        //드래그할 때 부모를 root로 설정하여 캔버스 맨 위로 이동.
        //이렇게 하면 드래그하는 아이템이 다른 UI 위에 표시됨.
        originalParent = transform.parent;
        //transform.SetParent(transform.root);
        //Debug.Log(eventData.position);
        // 아이템 복제하여 드래그 가능하도록 설정
        
        
        copyItem = Instantiate(gameObject, transform.position, Quaternion.identity, transform.root);
        copyItem.GetComponent<Image>().raycastTarget = false;
        copyItem.GetComponent<InventoryItem>().originalParent = null;

        
        if (copyItem.GetComponent<Image>()== true)
        {
            Debug.Log(copyItem.GetComponent<Image>().sprite.name);
            Debug.Log("아이콘 이미지가 복사되었습니다.");
        }
    }
    
    // public void StartDrag()
    // {
    //     copyItem.transform.SetParent(transform.root);
    //     transform.SetParent(parentAfterDrag);
    // }
    //
    //드래그 중 마우스가 움직일 때마다 계속 호출
    public void OnDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;
        //아이템의 위치를 마우스 커서 위치로 변경 -> 따라서 마우스가 움직이면 아이템도 따라옴.
        if (copyItem != null)
        {
            copyItem.transform.position = Input.mousePosition;
        }
    }

    //마우스 버튼을 놓을 때 호출.(드래그 종료)
    public void OnEndDrag(PointerEventData eventData)
    {
        // transform.SetParent(originalParent);
        if (copyItem != null)
        {
            //다시 클릭할 수 있도록 레이캐스트를 활성화.
            // image.raycastTarget = true;
            copyItem.GetComponent<Image>().raycastTarget = true;  
            //드래그가 끝나면 원래 부모(슬롯)로 되돌림.
            copyItem.transform.SetParent(parentAfterDrag);
        }
    }
    
}
*/



//드래그 앤 드롭 첫번째 코드
/*
public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //필드 변수
    [Header("UI")]
    //UI에서 아이템의 이미지를 담당하는 변수
    //Image는 Unity의 ui요소로 아이템의 아이콘을 표시하는데 사용됨.(원본 아이템 이미지)
    public Image image;
    
    //아이템이 원래 속해 있던 부모(슬롯)을 저장하는 변수.
    //HideInInspector -> Unity의 인스펙터에서 보이지 않도록 설정(코드에서는 접근 가능)
    [HideInInspector] public Transform parentAfterDrag; // 부모 저장 변수
    
    //Start()는 유니티에서 객체가 활성화될 때 한 번 호출됨.
    public void Awake()
    {
        //raycastTarget이 true이면 이 UI이미지가 마우스 클릭을 받을 수 있음. false면 클릭이 무시됨.
        //UI 이미지가 마우스 클릭을 받을 수 있도록 설정.
        image.raycastTarget = true;
    }

    //마우스를 클릭하고 드래그를 시작할 때 호출.
    //PointerEventData란? 마우스 클릭,터치 등의 입력 이벤트 정보를 포함하는 데이터 구조체.
    //eventData.position은 현재 마우스 커서의 위치
    public void OnBeginDrag(PointerEventData eventData)
    {
        //드래그 중에는 이미지가 클릭을 감지하지 않도록 설정.(이렇게 하면 UI드래그가 원활하게 이뤄짐)
        image.raycastTarget = false;
        //현재 부모(슬롯)를 저장 -> 드래그가 끝나면 다시 원래 자리로 돌아오기 위해 필요.
        parentAfterDrag = transform.parent;
        //드래그할 때 부모를 root로 설정하여 캔버스 맨 위로 이동.
        //이렇게 하면 드래그하는 아이템이 다른 UI 위에 표시됨.
        transform.SetParent(transform.root);
        //Debug.Log(eventData.position);
        
       
    }
    
    //드래그 중 마우스가 움직일 때마다 계속 호출
    public void OnDrag(PointerEventData eventData)
    {
        //아이템의 위치를 마우스 커서 위치로 변경 -> 따라서 마우스가 움직이면 아이템도 따라옴.
        transform.position = Input.mousePosition;
    }

    //마우스 버튼을 놓을 때 호출.(드래그 종료)
    public void OnEndDrag(PointerEventData eventData)
    {
        //다시 클릭할 수 있도록 레이캐스트를 활성화.
        image.raycastTarget = true;
        //드래그가 끝나면 원래 부모(슬롯)로 되돌림.
        transform.SetParent(parentAfterDrag);
    }
}
*/


/*
[Header("UI")]
public Image image;
    
[HideInInspector] public Item item;
[HideInInspector] public Transform parentAfterDrag; // 부모 저장 변수
    
public void InitializeItem(Item newItem)
{
    item = newItem;
    image.sprite = newItem.image;
}

public void OnBeginDrag(PointerEventData eventData)
{
    image.raycastTarget = false;
    parentAfterDrag = transform.parent;
    transform.SetParent(transform.root);
}

public void OnDrag(PointerEventData eventData)
{
    transform.position = Input.mousePosition;
}

public void OnEndDrag(PointerEventData eventData)
{
    image.raycastTarget = true;
    transform.SetParent(parentAfterDrag);
}
*/