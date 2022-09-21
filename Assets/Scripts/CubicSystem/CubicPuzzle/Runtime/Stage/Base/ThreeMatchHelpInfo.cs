using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Three Match Guide를 위한 정보 관리
     *  @detail 장시간 입력이 없는 경우 매치 가능한 블럭의 
     *          이동 방향 및 매치 리스트를 안내하기 위해 필요한 정보 관리 및 외곽선 계산 처리
     */
    public class ThreeMatchHelpInfo
    {
        //Swipe Target Block
        public BlockModel FromBlock { get; private set; }

        //Swipe 방향
        public BlockModel ToBlock { get; private set; }

        //Swipe시 Match될 Block List Indices
        public HashSet<int> MatchIndices { get; private set; }

        public int MatchCount => MatchIndices.Count;

        private BoardModel board;

        //외곽선 만들기에 사용되는 이웃의 위치 정보
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
         *  @brief  Guide 정보 Update
         *  @param  from : target block, to : dest block, indices : match block index list
         */
        public void UpdateMatchHelpInfo(BlockModel from, BlockModel to, HashSet<int> indices)
        {
            //매치 수가 현재 가진 정보의 수보다 높은 경우만 업데이트
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
         *  @brief  외곽선 위치 정보 만들기
         *  @return List<Vector2> : OutLine Vertex Position
         */
        public List<Vector2> CalcOutLineVertexHex()
        {
            //정보 가져오기
            List<CellModel> cells = board.Cells;
            List<Vector2> outLineVertexPositions = ListPool<Vector2>.Get();

            if(MatchCount <= 0) {
                return outLineVertexPositions;
            }

            int vertexCnt = 1;
            int targetBlockIdx = -1;

            BlockNeighType[] detectOrderNeighType = detectOrderNeighs[board.BoardType];
            int offsetCnt = board.BoardType == BoardType.HEX ? 2 : 1;

            //외곽선을 그리기 위한 시작 블럭 선택하기
            foreach(int index in MatchIndices) {
                int neighIdx = board.GetNeighIndex(index, detectOrderNeighType[vertexCnt]);

                if(!MatchIndices.Contains(neighIdx)) {
                    targetBlockIdx = index;
                    break;
                }
            }

            //외곽선 정보 만들기
            int startBlockIdx = targetBlockIdx;
            do {
                int neighIdx = board.GetNeighIndex(targetBlockIdx, detectOrderNeighType[vertexCnt]);

                if(!MatchIndices.Contains(neighIdx)) {
                    //vertex 정보 추가하고 vertexCnt 변경 
                    outLineVertexPositions.Add(cells[targetBlockIdx].GetVertexPosition(vertexCnt++));
                }
                //외곽선에 해당되지 않는 경우
                else {
                    //블럭 변경 후 변경된 블럭 기준으로 VertextCnt 수정
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
