using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Three Match Guide�� ���� ���� ����
     *  @detail ��ð� �Է��� ���� ��� ��ġ ������ ���� 
     *          �̵� ���� �� ��ġ ����Ʈ�� �ȳ��ϱ� ���� �ʿ��� ���� ���� �� �ܰ��� ��� ó��
     */
    public class ThreeMatchHelpInfo
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
        private readonly Dictionary<BoardType, BlockNeighType[]> detectOrderNeighs
            = new Dictionary<BoardType, BlockNeighType[]>() {
                { 
                    BoardType.HEX, new BlockNeighType[]{
                        BlockNeighType.LEFT_UP, BlockNeighType.UP, BlockNeighType.RIGHT_UP,
                        BlockNeighType.RIGHT_DOWN, BlockNeighType.DOWN, BlockNeighType.LEFT_DOWN
                    }
                },
                {
                    BoardType.SQUARE, new BlockNeighType[] {
                        BlockNeighType.LEFT, BlockNeighType.UP, BlockNeighType.RIGHT, BlockNeighType.DOWN
                    }
                }
        };

        public ThreeMatchHelpInfo(BoardModel board)
        {
            this.board = board;
            MatchIndices = new HashSet<int>();
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
            foreach(int index in indices) {
                MatchIndices.Add(index);
            }
        }



        public List<Vector2> CalcOutLineVertex()
        {
            return CalcOutLineVertexHex();
        }

        /**
         *  @brief  �ܰ��� ��ġ ���� �����
         *  @return List<Vector2> : OutLine Vertex Position
         */
        public List<Vector2> CalcOutLineVertexHex()
        {
            //���� ��������
            List<CellModel> cells = board.Cells;
            List<Vector2> outLineVertexPositions = ListPool<Vector2>.Get();

            if(MatchCount <= 0) {
                return outLineVertexPositions;
            }

            int vertexCnt = 1;
            int targetBlockIdx = -1;

            BlockNeighType[] detectOrderNeighType = detectOrderNeighs[board.BoardType];
            int offsetCnt = board.BoardType == BoardType.HEX ? 2 : 1;

            //�ܰ����� �׸��� ���� ���� �� �����ϱ�
            foreach(int index in MatchIndices) {
                int neighIdx = board.GetNeighIndex(index, detectOrderNeighType[vertexCnt]);

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
                    vertexCnt -= offsetCnt;
                }

                vertexCnt %= detectOrderNeighType.Length;
                if(vertexCnt < 0) {
                    vertexCnt = detectOrderNeighType.Length + vertexCnt;
                }

            } while(startBlockIdx != targetBlockIdx || vertexCnt != 1);

            return outLineVertexPositions;
        }

        public CellStyle GetCellStyle()
        {
            return board.Cells[FromBlock.Idx].Style;
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
