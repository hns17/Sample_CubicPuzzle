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
        
        //BFS ��� �̿� Ž���� ���� �ڷᱸ��
        private Queue<BlockModel> searchs = new Queue<BlockModel>();

        //Ž���� �̿� ��� ����
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
        *  @brief  Board�� ��ġ Block�� Ȯ�� �� ���� ������Ʈ
        *  @return true(��ġ ���� �ִ� ���), false(��ġ ���� ���� ���)
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
       *  @brief  Block�� ��ġ ���� Ȯ�� �� ������Ʈ
       *  @param  block(Ȯ�� �� ��)
       *  @return true(��ġ ���� �ִ� ���), false(��ġ ���� ���� ���)
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
