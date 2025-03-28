using System.Collections.Generic;
using UnityEngine;

//Dictionary를 사용하여 마인크래프트 아이템에 대한 정보를 저장한다.
public class ItemData2 : MonoBehaviour
{
   //Key 값으로 실제 필드와 함께 공유하는 블록 enum값을 넣고, Value로 실제 게임 오브젝트와 매칭해줌
   //딕셔너리의 Value 값으로 클래스를 불러온다.
   public Dictionary<BlockTypeEnum,ItemTypeData2> Item = new Dictionary<BlockTypeEnum,ItemTypeData2>();

   public Sprite StoneSprite;
   public Sprite DirtSprite;
   public Sprite GrassSprite;
   public Sprite OakWoodSprite;
   public void Start()
   {
      //ItemData2 클래스에 있는 필드를 가져오기 위해 생성자를 활용하여 각 아이템마다 인스턴스 생성
      ItemTypeData2 Stone = new ItemTypeData2();
      ItemTypeData2 Dirt = new ItemTypeData2();
      ItemTypeData2 Grass = new ItemTypeData2();
      ItemTypeData2 OakWood = new ItemTypeData2();
      
      //ItemData2 클래스에 있는 itemName 필드를 가져와 각 아이템의 이름을 저장해준다. 
      Stone.itemName = "Stone";
      Dirt.itemName = "Dirt";
      Grass.itemName = "Grass";
      OakWood.itemName = "OakWood";
      
      //ItemData2 클래스에 있는 itemSprite 필드를 가져와 각 아이템의 이미지 정보를 저장해준다. 
      Stone.itemSprite = StoneSprite;
      Dirt.itemSprite = DirtSprite;
      Grass.itemSprite = GrassSprite;
      OakWood.itemSprite = OakWoodSprite;
      
      //위의 인스턴스에 저장한 정보를 기져와 딕셔너리에 아이템의 enum값과 value값을 매칭 시켜준다.
      Item.Add(BlockTypeEnum.Stone,Stone);
      Item.Add(BlockTypeEnum.Dirt,Dirt);
      Item.Add(BlockTypeEnum.Grass,Grass);
      Item.Add(BlockTypeEnum.OakWood,OakWood);
   }
}
