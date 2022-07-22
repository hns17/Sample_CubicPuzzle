using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CubicSystem.CubicPuzzle
{
    public class OneTouchMatchEvaluator :IMatchEvaluator
    {
        private BoardModel board;
        private const int MatchCount = 2;
        
        private Queue<BlockModel> searchs = new Queue<BlockModel>();
        private List<BlockNeighType> blockNeighs = new List<BlockNeighType>();

        public OneTouchMatchEvaluator(BoardModel board)
        {
            this.board = board;

            for(var neigh = BlockNeighType.START; neigh < BlockNeighType.NONE; neigh++) {
                this.blockNeighs.Add(neigh);
            }
        }


        private void SearchMatchIndices(BlockModel target, HashSet<int> matchIndices)
        {
            searchs.Clear();
            searchs.Enqueue(target);

            while(searchs.Count > 0) {
                var popItem = searchs.Dequeue();
                matchIndices.Add(popItem.Idx);

                var matchColor = popItem.Color;

                foreach(var neigh in blockNeighs) {
                    var neighBlock = board.GetNeighBlock(popItem, neigh);

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
        public bool EvalMatchBoard(HashSet<int> matchIndices)
        {
            var blocks = board.Blocks;

            for(var i = 0; i < blocks.Count; i++) {
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
        public bool EvalMatchBlock(BlockModel block, HashSet<int> matchIndices)
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
                string print = isMatched + " ";

                foreach(var index in matchIndices) {
                    print += index + ", ";
                }
                print += "End";
                Debug.Log(print);
            }
            else {
                isMatched = EvalMatchBlock(block, matchIndices);
            }

            return isMatched;
        }
    }

}
