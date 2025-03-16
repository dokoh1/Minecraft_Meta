using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//이 스크립트는 아이템 개별관리, 드래그 및 드롭을 지원한다.
//인벤토리 슬롯 안에 들어갈 아이템 개체를 나타내며, 드래그와 드롭이 가능한 구조이다.
//주요역할: 아이템 정보를 담고 있고, 
public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
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
}

