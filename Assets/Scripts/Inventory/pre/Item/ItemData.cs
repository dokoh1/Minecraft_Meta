using UnityEngine;
using System.Collections.Generic;

public class ItemData : MonoBehaviour
{
    public readonly Dictionary<BlockTypeEnum, ItemTypeData> ItemTypeDictionary = new();
    
    //각 블럭의 이미지를 가져올 변수
    public Sprite StoneSprite;
    public Sprite GrassSprite;
    public Sprite OakWoodSprite;
    public Sprite DirtSprite;
    
    //아이템 딕셔너리에 블럭 이름에 따른 이미지를 불러올 함수
    private void Awake()
    {
        ItemTypeData ItemInfo1 = new ItemTypeData();
        ItemInfo1.itemName = "Stone";
        ItemInfo1.itemSprite = StoneSprite;
        ItemTypeData ItemInfo2 = new ItemTypeData();
        ItemInfo2.itemName = "Grass";
        ItemInfo2.itemSprite = GrassSprite;
        ItemTypeData ItemInfo3 = new ItemTypeData();
        ItemInfo3.itemName = "OakWood";
        ItemInfo3.itemSprite = OakWoodSprite;
        ItemTypeData ItemInfo4 = new ItemTypeData();
        ItemInfo4.itemName = "Dirt";
        ItemInfo4.itemSprite = DirtSprite;
        
        ItemTypeDictionary.Add(BlockTypeEnum.Stone, ItemInfo1);
        ItemTypeDictionary.Add(BlockTypeEnum.Grass, ItemInfo2);
        ItemTypeDictionary.Add(BlockTypeEnum.OakWood, ItemInfo3);
        ItemTypeDictionary.Add(BlockTypeEnum.Dirt, ItemInfo4);
    }
    
    //만약에 사용자가 해당 블럭을 캐서 먹었을시 어떤 아이템인지 foreach를 돌며 
    //해당되는 아이템 이미지를 PlayerToolbar(9개의 슬롯)의 빈 슬롯에 추가되도록 하는 함수

    private void GetItem()
    {
        //만약 어떠한 오브젝트가 사용자에게 들어왔을 시
        
        // if (ItemTypeDictionary.TryGetValue())
        // {
        //     //foreach로 슬롯 9개를 돌며 이름이 같은 아이템이 있는지 확인
        // }
            
    }
    
    
    //만약에 사용자가 자신의 PlayerToolbar에 있는 아이템을 키보드로 선택을 해서 
    //필드 바닥을 클릭했을 시 그 아이템 아이콘이 어떤 아이템 블록인지 확인을 해서 설치하도록 하는 함수
    
}


