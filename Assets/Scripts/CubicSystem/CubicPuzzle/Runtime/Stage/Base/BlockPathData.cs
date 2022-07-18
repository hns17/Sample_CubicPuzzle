using Unity.Burst;
using UnityEngine;


/**
 *  @brief  Block�� �̵� ��ο� ���� ������ ���
 */
namespace CubicSystem.CubicPuzzle
{
    [BurstCompile]
    public struct BlockPathData
    {
        //��� ����(Native Array)
        public CubicNativeArray<Vector2> PathData;
        public int Count => PathData.Count;
        public BlockNeighType PrevMoveDirection;

        //���� ��ġ
        public Vector2 FromPosition { get; private set; }

        /**
         *  @brief  BlockPathData ������
         *  @param  capacity : ��� ���� �ִ� ��Ϸ�, fromPosition : ��� ���� ��ġ
         */
        public BlockPathData(int capacity, Vector2 fromPosition)
        {
            FromPosition = fromPosition;
            PrevMoveDirection = BlockNeighType.NONE;
            PathData = new CubicNativeArray<Vector2>(capacity, Unity.Collections.Allocator.Persistent);
        }

        /**
         *  @brief  BlockPathData ������(���� ��� �� ����)
         *  @param  blockPath : �����Ϸ��� ��� ����
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
         *  @brief  �̵� ��� ���� �߰��ϱ�
         *  @param  position(�̵� �� ��ġ), index(������� ��ϵ� �ִ� �̵� ���)
         *  @detail Debugging ���Ǹ� ���� ��θ� �߰��� �� ���ݱ��� ��ϵ� �ٸ� ���� �ִ� ��ο� �����.
         *          (��� Ž��, ���� �Ͻ����� ��� ��)
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
