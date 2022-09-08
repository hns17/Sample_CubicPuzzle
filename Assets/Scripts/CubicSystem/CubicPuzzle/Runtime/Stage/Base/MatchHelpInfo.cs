using System.Collections.Generic;
using UnityEngine;


namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Block Match Guide�� ���� ���� ����
     *  @detail ��ð� �Է��� ���� ��� ��ġ ������ ���� 
     *          �̵� ���� �� ��ġ ����Ʈ�� �ȳ��ϱ� ���� �ʿ��� ���� ���� �� �ܰ��� ��� ó��
     */
    public class MatchHelpInfo
    {
        //Swipe Target Block
        public BlockModel FromBlock { get; private set; }

        //Swipe ����
        public BlockModel ToBlock { get; private set; }

        //Swipe�� Match�� Block List Indices
        public HashSet<int> MatchIndices { get; private set; }

        public int MatchCount => MatchIndices.Count;

        private BoardModel board;

        //�ܰ��� ����⿡ ���Ǵ� �̿��� ��ġ ����
        private readonly BlockNeighType[] detectOrderNeighType = {
            BlockNeighType.LEFT_UP, BlockNeighType.UP, BlockNeighType.RIGHT_UP,
            BlockNeighType.RIGHT_DOWN, BlockNeighType.DOWN, BlockNeighType.LEFT_DOWN
        };

        private List<Vector2> outLineVertexPositions;

        public MatchHelpInfo(BoardModel board)
        {
            this.board = board;
            MatchIndices = new HashSet<int>();
            outLineVertexPositions = new List<Vector2>();
        }

        /**
         *  @brief  Guide ���� Update
         *  @param  from : target block, to : dest block, indices : match block index list
         */
        public void UpdateMatchHelpInfo(BlockModel from, BlockModel to, HashSet<int> indices)
        {
            //��ġ ���� ���� ���� ������ ������ ���� ��츸 ������Ʈ
            if(indices.Count <= MatchIndices.Count) {
                return;
            }

            this.FromBlock = from;
            this.ToBlock = to;

            MatchIndices.Clear();
            foreach(var index in indices) {
                MatchIndices.Add(index);
            }
        }



        public List<Vector2> CalcOutLineVertex()
        {
            outLineVertexPositions.Clear();

            if(board.BoardStyle == BoardType.HEX) {
                return CalcOutLineVertexHex();
            }

            return outLineVertexPositions;
        }

        /**
         *  @brief  �ܰ��� ��ġ ���� �����
         *  @return List<Vector2> : OutLine Vertex Position
         */
        public List<Vector2> CalcOutLineVertexHex()
        {
            //���� ��������
            var cells = board.Cells;
            

            if(MatchCount <= 0) {
                return outLineVertexPositions;
            }

            int vertexCnt = 1;
            int targetBlockIdx = -1;

            //�ܰ����� �׸��� ���� ���� �� �����ϱ�
            foreach(var index in MatchIndices) {
                var neighIdx = board.GetNeighIndex(index, detectOrderNeighType[vertexCnt]);

                if(!MatchIndices.Contains(neighIdx)) {
                    targetBlockIdx = index;
                    break;
                }
            }

            //�ܰ��� ���� �����
            int startBlockIdx = targetBlockIdx;
            do {
                int neighIdx = board.GetNeighIndex(targetBlockIdx, detectOrderNeighType[vertexCnt]);

                if(!MatchIndices.Contains(neighIdx)) {
                    //vertex ���� �߰��ϰ� vertexCnt ���� 
                    outLineVertexPositions.Add(cells[targetBlockIdx].GetVertexPosition(vertexCnt++));
                }
                //�ܰ����� �ش���� �ʴ� ���
                else {
                    //�� ���� �� ����� �� �������� VertextCnt ����
                    targetBlockIdx = neighIdx;
                    vertexCnt -= 2;
                }

                vertexCnt %= detectOrderNeighType.Length;
                if(vertexCnt < 0) {
                    vertexCnt = detectOrderNeighType.Length + vertexCnt;
                }

            } while(startBlockIdx != targetBlockIdx || vertexCnt != 1);

            return outLineVertexPositions;
        }

        /**
         *  @brief  Reset MatchHelpInfo
         */
        public void Clear()
        {
            FromBlock = null;
            ToBlock = null;
            MatchIndices.Clear();
        }
    }

}
