using System.Collections.Generic;
using UnityEngine;


/**
 *  @brief  Block의 이동 경로에 대한 정보를 기록
 */
namespace CubicSystem.CubicPuzzle
{
    public class BlockPathData
    {
        //경로 정보(Native Array)
        public List<Vector2> PathData;
        public int Count => PathData.Count;
        public BlockNeighType PrevMoveDirection;

        //시작 위치
        public Vector2 FromPosition { get; set; }

        /**
         *  @brief  BlockPathData 생성자
         *  @param  capacity : 경로 정보 최대 기록량, fromPosition : 경로 시작 위치
         */
        public BlockPathData(Vector2 fromPosition)
        {
            FromPosition = fromPosition;
            PrevMoveDirection = BlockNeighType.NONE;
            PathData = new List<Vector2>(100);
        }

        /**
         *  @brief  이동 경로 정보 추가하기
         *  @param  position(이동 할 위치), index(현재까지 기록된 최대 이동 경로)
         *  @detail Debugging 편의를 위해 경로를 추가할 때 지금까지 기록된 다른 블럭의 최대 경로와 맞춘다.
         *          (경로 탐색, 블럭의 일시적인 대기 등)
         */
        public void InsertData(Vector2 position, int maxPathCount, BlockNeighType moveDirection)
        {
            int interval = maxPathCount - Count;
            Vector2 insertData = Count == 0 ? FromPosition : PathData[Count - 1];
            for(int i = 0; i < interval; i++) {
                PathData.Add(insertData);
            }

            PrevMoveDirection = moveDirection;

            PathData.Add(position);
        }

        /**
         *  @brief  Release BlockPathData Instance
         */
        public void Clear()
        {
            FromPosition = Vector2.zero;
            PathData.Clear();
            PrevMoveDirection = BlockNeighType.NONE;
        }
    }
}
