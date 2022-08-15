using System.Collections.Generic;
using UnityEngine;


/**
 *  @brief  Block�� �̵� ��ο� ���� ������ ���
 */
namespace CubicSystem.CubicPuzzle
{
    public class BlockPathData
    {
        //��� ����(Native Array)
        public List<Vector2> PathData;
        public int Count => PathData.Count;
        public BlockNeighType PrevMoveDirection;

        //���� ��ġ
        public Vector2 FromPosition { get; set; }

        /**
         *  @brief  BlockPathData ������
         *  @param  capacity : ��� ���� �ִ� ��Ϸ�, fromPosition : ��� ���� ��ġ
         */
        public BlockPathData(Vector2 fromPosition)
        {
            FromPosition = fromPosition;
            PrevMoveDirection = BlockNeighType.NONE;
            PathData = new List<Vector2>(100);
        }

        /**
         *  @brief  �̵� ��� ���� �߰��ϱ�
         *  @param  position(�̵� �� ��ġ), index(������� ��ϵ� �ִ� �̵� ���)
         *  @detail Debugging ���Ǹ� ���� ��θ� �߰��� �� ���ݱ��� ��ϵ� �ٸ� ���� �ִ� ��ο� �����.
         *          (��� Ž��, ���� �Ͻ����� ��� ��)
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
