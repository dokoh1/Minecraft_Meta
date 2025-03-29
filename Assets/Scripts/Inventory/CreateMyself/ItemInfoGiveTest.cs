using UnityEngine;

public class ItemInfoGiveTest : MonoBehaviour
{
    //ItemTypeData2 클래스의 생성자를 만듬
    ItemTypeData2 ItemType = new();
    //ItemData2 클래스 스크립트를 넣을 변수 선언
    public ItemData2 itemData;
    
    private void Start()
    {
        
    }
    
    public void Update()
    {
        ClickItemImage();
    }
    
    public void ClickItemImage()
    {
        //아이템 이미지를 클릭 했을 때 딕셔너리의 정보를 불러와 저장된 정보와 같은지 확인한 후 
        //디버그 창에 출력하여 해당 아이템 정보와 일치하는지 확인
        // if (Input.GetMouseButtonDown(0))
        // {
        //     Debug.Log("마우스 좌측 클릭 성공");
        //     Debug.Log(itemData.StoneData);
        //     //만약 클릭한 아이템 이미지가 stone이미지이면 디버그 창에 stone블록이라고 출력
        //     
        //     if( == itemData.StoneSprite)
        //     {
        //         //Debug.Log(it);
        //     }
        //     else if (item.itemSprite == itemData.DirtSprite)
        //     {
        //         Debug.Log("Dirt 블록입니다.");
        //     }
        //     else if (item.itemSprite == itemData.GrassSprite)
        //     {
        //         Debug.Log("Grass 블록입니다.");
        //     }
        //     else if (item.itemSprite == itemData.OakWoodSprite)
        //     {
        //         Debug.Log("OakWood 블록입니다.");
        //     }
        //     else
        //     {
        //         Debug.Log("아이템 이미지를 클릭하지 않았습니다.");
        //     }
        //     
        //     
        // }
        
    }
}
