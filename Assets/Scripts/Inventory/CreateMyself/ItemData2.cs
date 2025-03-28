using System.Collections.Generic;
using UnityEngine;

//Dictionary
public class ItemData2 : MonoBehaviour
{
   //딕셔너리 생성자를 선언하여 도감?을 실제로 형체화함
   //Key 값으로 실제 필드와 함께 공유하는 블록 enum값을 넣고, Value로 실제 게임 오브젝트와 매칭해줌
   //Dictionary<BlockTypeEnum,GameObject> itemSprite = new Dictionary<BlockTypeEnum,GameObject>();
   //Dictionary<BlockTypeEnum,BlockData> itemBlockDatas = new Dictionary<BlockTypeEnum,BlockData>();
   //딕셔너리의 Value 값으로 클래스를 불러온다.
   Dictionary<BlockTypeEnum,ItemTypeData2> item = new Dictionary<BlockTypeEnum,ItemTypeData2>();
   
   
}
