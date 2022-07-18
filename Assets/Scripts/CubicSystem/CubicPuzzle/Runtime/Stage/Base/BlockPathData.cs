using Unity.Burst;
using UnityEngine;


/**
 *  @brief  Block의 이동 경로에 대한 정보를 기록
 */
namespace CubicSystem.CubicPuzzle
{
    [BurstCompile]
    public struct BlockPathData
    {
        //경로 정보(Native Array)
        public CubicNativeArray<Vector2> PathData;
        public int Count => PathData.Count;
        public BlockNeighType PrevMoveDirection;

        //시작 위치
        public Vector2 FromPosition { get; private set; }

        /**
         *  @brief  BlockPathData 생성자
         *  @param  capacity : 경로 정보 최대 기록량, fromPosition : 경로 시작 위치
         */
        public BlockPathData(int capacity, Vector2 fromPosition)
        {
            FromPosition = fromPosition;
            PrevMoveDirection = BlockNeighType.NONE;
            PathData = new CubicNativeArray<Vector2>(capacity, Unity.Collections.Allocator.Persistent);
        }

        /**
         *  @brief  BlockPathData 생성자(기존 경로 값 복사)
         *  @param  blockPath : 복사하려는 경로 정보
         */
        public BlockPathData(BlockPathData blockPath)
        {
            FromPosition = blockPath.FromPosition;
            PrevMoveDirection = blockPath.PrevMoveDirection;
            PathData = new CubicNativeArray<Vector2>(blockPath.PathData);
        }

        /**
         *  @brief  Release BlockPathData Instance
         */
        public void Clear()
        {
            PathData.Dispose();
        }

        /**
         *  @brief  이동 경로 정보 추가하기
         *  @param  position(이동 할 위치), index(현재까지 기록된 최대 이동 경로)
         *  @detail Debugging 편의를 위해 경로를 추가할 때 지금까지 기록된 다른 블럭의 최대 경로와 맞춘다.
         *          (경로 탐색, 블럭의 일시적인 대기 등)
         */
        public void InsertData(Vector2 position, int maxPathCount, BlockNeighType moveDirection)
        {
            var interval = maxPathCount - Count;
            var insertData = Count == 0 ? FromPosition : PathData[Count - 1];
            for(int i = 0; i < interval; i++) {
                PathData.Add(insertData);
            }

            PrevMoveDirection = moveDirection;

            PathData.Add(position);
        }
    }
}
