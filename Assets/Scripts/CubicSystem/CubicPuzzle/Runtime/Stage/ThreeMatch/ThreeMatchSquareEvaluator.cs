using System;
using System.Collections.Generic;
using Unity.Collections;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Board와 Block의 Match 상태를 평가
     *  @detail Line(세로, 대각선, 역대각선) Match, Square Match(인접한 4개의 블럭)가 있는지 
     *          평가하고 블럭의 상태를 Update
     */
    public class ThreeMatchSquareEvaluator : IMatchEvaluator
    {
        private const int LineMatchCount = 3;

        /**
        *  @brief  Board의 전체 Block을 대상으로 Line Match 평가에 사용될 NeighType List
        *  @detail  neighType {
        *       RIGHT_DOWN(대각선)
        *       DOWN(세로)
        *       LEFT_DOWN(역 대각선)
        *  }
        */
        private readonly List<List<BlockNeighType>> BoardLineMatchNeighTypes = new List<List<BlockNeighType>>{
            new List<BlockNeighType>{ BlockNeighType.RIGHT},
            new List<BlockNeighType>{ BlockNeighType.DOWN}
        };


        /**
        *  @brief  Block 기준으로 Line Match 평가에 사용될 NeighType List
        *  @detail  neighType {
        *       LEFT_UP, RIGHT_DOWN(대각선)
        *       UP, DOWN(세로)
        *       RIGHT_UP, LEFT_DOWN(역 대각선)
        *  }
        */
        private readonly List<List<BlockNeighType>> BlockLineMatchNeighTypes = new List<List<BlockNeighType>>{
            new List<BlockNeighType>{ BlockNeighType.LEFT, BlockNeighType.RIGHT},
            new List<BlockNeighType>{ BlockNeighType.UP, BlockNeighType.DOWN}
        };


        private BoardModel board;

        //Board 평가 함수 Set
        private List<Func<BlockModel, HashSet<int>, bool>> boardEvaluator;

        //Block 평가 함수 Set
        private List<Func<BlockModel, HashSet<int>, bool>> blockEvaluator;
        //private List<Func<BlockModel, bool, bool>> possibleEvaluator;

        public ThreeMatchSquareEvaluator(BoardModel board)
        {
            this.board = board;

            boardEvaluator = new List<Func<BlockModel, HashSet<int>, bool>>();
            //Make BoardLineEvaluator
            foreach(var matchNeighTypes in BoardLineMatchNeighTypes) {
                boardEvaluator.Add((block, matchIndices) =>
                {
                    return EvalLineMatched(block, matchNeighTypes, matchIndices);
                });
            }
           


            blockEvaluator = new List<Func<BlockModel, HashSet<int>, bool>>();
            //Make Block LineEvaluator
            foreach(var matchNeighTypes in BlockLineMatchNeighTypes) {
                blockEvaluator.Add((block, matchIndices) =>
                {
                    return EvalLineMatched(block, matchNeighTypes, matchIndices);
                });
            }
        }


        /**
        *  @brief  Board의 매치 Block을 확인 후 상태 업데이트
        *  @return true(매치 블럭이 있는 경우), false(매치 블럭이 없는 경우)
        */
        public bool EvalMatchBoard(HashSet<int> matchIndices)
        {
            bool res = false;
            var blocks = board.Blocks;
            
            foreach(var block in blocks) {
                if(!block.IsEnableBlock()) {
                    continue;
                }

                foreach(var eval in boardEvaluator) {
                    res |= eval(block, matchIndices);
                }
            }

            return res;
        }

        /**
       *  @brief  Block의 매치 상태 확인 후 업데이트
       *  @param  block(확인 할 블럭)
       *  @return true(매치 블럭이 있는 경우), false(매치 블럭이 없는 경우)
       */
        public bool EvalMatchBlock(BlockModel block, HashSet<int> matchIndices)
        {
            bool res = false;

            if(block.IsEnableBlock()) {
                foreach(var eval in blockEvaluator) {
                    res |= eval(block, matchIndices);
                }
            }
            
            return res;
        }

        /**
         *  @brief  Line 평가 함수
         *  @block  target Block, neighTypes : 평가할 이웃 Block 리스트
         */
        private bool EvalLineMatched(BlockModel block, 
                                        List<BlockNeighType> neighTypes,
                                        HashSet<int> matchIndices)
        {
            bool res = false;
            
            //Initialize Match index List
            CubicSpanArray<int> indices = new CubicSpanArray<int>(stackalloc int[board.BlockCount]);
            indices.Add(block.Idx);

            //Make Match index List
            for(int i = 0; i < neighTypes.Count; i++) {
                RecursiveEvalLineBlockMatched(block, neighTypes[i], ref indices);
            }

            //Is Pass Match Condition Count
            if(indices.Count >= LineMatchCount) {
                res = true;
                for(int i = 0; i < indices.Count; i++) {
                    matchIndices?.Add(indices[i]);
                }
            }

            return res;
        }

        /**
         *  @brief  지정된 이웃 방향으로 Match가 되지 않을때까지 반복적으로 평가
         *  @param  block : 평가 Block, blockNeighType : 비교할 이웃 Block 위치, indices : 현재까지 매치된 Block Index List
         */
        private void RecursiveEvalLineBlockMatched(BlockModel block, BlockNeighType blockNeighType, ref CubicSpanArray<int> indices)
        {
            var neighBlock = board.GetNeighBlock(block, blockNeighType);
            if(neighBlock != null && neighBlock.IsEnableBlock()) {
                if(neighBlock.IsCompareColor(block.Color)) {
                    indices.Add(neighBlock.Idx);
                    RecursiveEvalLineBlockMatched(neighBlock, blockNeighType, ref indices);
                }
            }
        }

        /**
         *  @brief  3-Match 최소 단위(3-Line) 체크
         *  @param  block : target block, null인 경우 Board 전체 체크
         *  @param  matchIndices : Match된 Block List
         *  @return true : Match Block이 있는 경우 / false : 없는 경우
         */
        public bool Evaluator(BlockModel block, HashSet<int> matchIndices)
        {
            matchIndices?.Clear();

            //모든 블럭을 대상으로 매치 검사
            if(block == null) {
                return EvalMatchBoard(matchIndices);
            }
            //지정된 블럭을 대상으로 매치 검사
            else {
                return EvalMatchBlock(block, matchIndices);
            }
        }
    }
}