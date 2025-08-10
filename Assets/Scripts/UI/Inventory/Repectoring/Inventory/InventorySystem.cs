using System;
using UnityEngine;
using System.Collections.Generic;

//인벤토리에 있는 모든 슬롯을 관리하는 클래스
public class InventorySystem : MonoBehaviour
{
    //다른 클래스에서 하나의 인벤토리 시스템 객체에 접근할 수 있도록 싱글톤 선언
    public static InventorySystem instance;

    private void Awake()
    {
        //만약 instance가 비어있다면 InventorySystem 자기 자실을 할당
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            //만약 instance가 이미 존재하는데 또 다른 InventorySystem이 생성되면 새로 생긴 것을 파괴
            //단 하나만 존재하도록 보장
            Destroy(gameObject);
        }
    }

    //45개의 모든 슬롯을 담을 리스트
    public List<InventorySlot> Slots;

}
