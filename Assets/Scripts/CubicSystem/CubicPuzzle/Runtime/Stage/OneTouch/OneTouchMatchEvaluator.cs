using System.Collections.Generic;


namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  OneTouch Match Evaluator
     */
    public class OneTouchMatchEvaluator :IMatchEvaluator
    {
        private BoardModel board;
        private const int MatchCount = 2;
        
        //BFS 기반 이웃 탐색에 사용될 자료구조
        private Queue<BlockModel> searchs = new Queue<BlockModel>();

        //탐색할 이웃 블록 정보
        private List<BlockNeighType> blockNeighs = new List<BlockNeighType>();

        public OneTouchMatchEvaluator(BoardModel board)
        {
            this.board = board;
            blockNeighs = CubicPuzzleUtility.GetBlockNeighTypes(board.BoardType);
        }

        /**
         *  @brief  Create Match List
         *  @target : target block, matchIndices : Match List Index Container
         */
        private void SearchMatchIndices(BlockModel target, HashSet<int> matchIndices)
        {
            searchs.Clear();
            searchs.Enqueue(target);

            while(searchs.Count > 0) {
                BlockModel popItem = searchs.Dequeue();
                matchIndices.Add(popItem.Idx);

                MatchColorType matchColor = popItem.Color;

                foreach(BlockNeighType neigh in blockNeighs) {
                    BlockModel neighBlock = board.GetNeighBlock(popItem, neigh);

                    if(neighBlock == null || !neighBlock.IsEnableBlock()) {
                        continue;
                    }
                    else if(neighBlock.IsCompareColor(matchColor)) {
                        if(!matchIndices.Contains(neighBlock.Idx)) {
                            searchs.Enqueue(neighBlock);
                        }
                    }
                }
            }
        }

        /**
        *  @brief  Board의 매치 Block을 확인 후 상태 업데이트
        *  @return true(매치 블럭이 있는 경우), false(매치 블럭이 없는 경우)
        */
        private bool EvalMatchBoard(HashSet<int> matchIndices)
        {
            List<BlockModel> blocks = board.Blocks;

            for(int i = 0; i < blocks.Count; i++) {
                if(EvalMatchBlock(blocks[i], matchIndices)) {
                    return true;
                }
            }

            return false;
        }

        /**
       *  @brief  Block의 매치 상태 확인 후 업데이트
       *  @param  block(확인 할 블럭)
       *  @return true(매치 블럭이 있는 경우), false(매치 블럭이 없는 경우)
       */
        private bool EvalMatchBlock(BlockModel block, HashSet<int> matchIndices)
        {
            if(!block.IsEnableBlock()) {
                return false;
            }

            matchIndices.Clear();
            SearchMatchIndices(block, matchIndices);

            return matchIndices.Count >= MatchCount;
        }

        public bool Evaluator(BlockModel block, HashSet<int> matchIndices)
        {
            bool isMatched = false;
            if(block == null) {
                isMatched = EvalMatchBoard(matchIndices);
            }
            else {
                isMatched = EvalMatchBlock(block, matchIndices);
            }

            return isMatched;
        }
    }

}
