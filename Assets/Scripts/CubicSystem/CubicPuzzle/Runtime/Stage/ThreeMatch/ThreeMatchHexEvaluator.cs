using System;
using System.Collections.Generic;
using Unity.Collections;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Board와 Block의 Match 상태를 평가
     *  @detail Line(세로, 대각선, 역대각선) Mattch, Square Match(인접한 4개의 블럭)가 있는지 
     *          평가하고 블럭의 상태를 Update
     */
    public class ThreeMatchHexEvaluator : IMatchEvaluator
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
            new List<BlockNeighType>{ BlockNeighType.RIGHT_DOWN},
            new List<BlockNeighType>{ BlockNeighType.DOWN},
            new List<BlockNeighType>{ BlockNeighType.LEFT_DOWN}
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
            new List<BlockNeighType>{ BlockNeighType.LEFT_UP, BlockNeighType.RIGHT_DOWN},
            new List<BlockNeighType>{ BlockNeighType.UP, BlockNeighType.DOWN},
            new List<BlockNeighType>{ BlockNeighType.RIGHT_UP, BlockNeighType.LEFT_DOWN}
        };


        /**
        *  @brief  Board의 전체 Block을 대상으로 Square Match 평가에 사용될 NeighType List
        *  @detail {
        *      RightUp, RightDown, Down
        *      RightDown, Down, LeftDown
        *      Down, LeftDown, LeftUp
        *  }
        */
        private readonly List<List<BlockNeighType>> BoardSquareMatchNeighTypes = new List<List<BlockNeighType>>{
             new List<BlockNeighType>{ BlockNeighType.RIGHT_UP, BlockNeighType.RIGHT_DOWN, BlockNeighType.DOWN},
             new List<BlockNeighType>{ BlockNeighType.RIGHT_DOWN, BlockNeighType.DOWN, BlockNeighType.LEFT_DOWN},
             new List<BlockNeighType>{ BlockNeighType.DOWN, BlockNeighType.LEFT_DOWN, BlockNeighType.LEFT_UP},
        };

        /**
         *  @brief  Block을 대상으로 Square Match 평가에 사용될 NeighType List
         *  @detail {
         *      LeftUp, Up, RightUp
         *      Up, RightUp, RightDown
         *      RightUp, RightDown, Down
         *      RightDown, Down, LeftDown
         *      Down, LeftDown, LeftUp
         *  }
         */
        private readonly List<List<BlockNeighType>> BlockSquareMatchNeighTypes = new List<List<BlockNeighType>>{
             new List<BlockNeighType>{ BlockNeighType.LEFT_UP, BlockNeighType.UP, BlockNeighType.RIGHT_UP},
             new List<BlockNeighType>{ BlockNeighType.UP, BlockNeighType.RIGHT_UP, BlockNeighType.RIGHT_DOWN},
             new List<BlockNeighType>{ BlockNeighType.RIGHT_UP, BlockNeighType.RIGHT_DOWN, BlockNeighType.DOWN},
             new List<BlockNeighType>{ BlockNeighType.RIGHT_DOWN, BlockNeighType.DOWN, BlockNeighType.LEFT_DOWN},
             new List<BlockNeighType>{ BlockNeighType.DOWN, BlockNeighType.LEFT_DOWN, BlockNeighType.LEFT_UP},
             new List<BlockNeighType>{ BlockNeighType.LEFT_DOWN, BlockNeighType.LEFT_UP, BlockNeighType.UP},
        };


        private BoardModel board;

        //Board 평가 함수 Set
        private List<Func<BlockModel, HashSet<int>, bool>> boardEvaluator;

        //Block 평가 함수 Set
        private List<Func<BlockModel, HashSet<int>, bool>> blockEvaluator;
        //private List<Func<BlockModel, bool, bool>> possibleEvaluator;

        public ThreeMatchHexEvaluator(BoardModel board)
        {
            this.board = board;

            //var aa = UniTaskHelper.Func<BlockModel, UniTask<bool>>
            //    (async (block) => await EvalLineMatched(block, neighTypes));

            boardEvaluator = new List<Func<BlockModel, HashSet<int>, bool>>();
            //Make BoardLineEvaluator
            foreach(var matchNeighTypes in BoardLineMatchNeighTypes) {
                boardEvaluator.Add((block, matchIndices) =>
                {
                    return EvalLineMatched(block, matchNeighTypes, matchIndices);
                });
            }
            //Make Board SquareEvaluator
            foreach(var matchNeighTypes in BoardSquareMatchNeighTypes) {
                boardEvaluator.Add((block, matchIndices) =>
                {
                    return EvalSquareMatched(block, matchNeighTypes, matchIndices);
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
            //Make Block SquareEvaluator
            foreach(var matchNeighTypes in BlockSquareMatchNeighTypes) {
                blockEvaluator.Add((block, matchIndices) =>
                {
                    bool res = EvalSquareMatched(block, matchNeighTypes, matchIndices);
                    for(BlockNeighType i = BlockNeighType.START + 1; i<BlockNeighType.NONE; i++) {
                        var neighBlock = board.GetNeighBlock(block, i);
                        res |= EvalSquareMatched(neighBlock, matchNeighTypes, matchIndices);
                    }
                    return res;
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
         *  @brief  Square Match 평가하기
         *  @param  block : targetBlock, neighTypes : 평가할 이웃 목록
         *  @return bool(true : Match, false : No Match)
         */
        private bool EvalSquareMatched(BlockModel block, 
                                        List<BlockNeighType> neighTypes,
                                        HashSet<int> matchIndices)
        {
            if(block == null || !block.IsEnableBlock()) {
                return false;
            }

            bool res = false;
            CubicSpanArray<int> indices = new CubicSpanArray<int>(stackalloc int[board.BlockCount]);

            //Target Block과 이웃 Block 평가하기
            foreach(var neighType in neighTypes) {
                var neighBlock = board.GetNeighBlock(block, neighType);
                if(neighBlock == null || !neighBlock.IsEnableBlock()) {
                    break;
                }

                if(!block.IsCompareColor(neighBlock.Color)) {
                    break;
                }

                indices.Add(neighBlock.Idx);
            }

            //Match된 경우 Block 기록
            if(neighTypes.Count == indices.Count) {
                res = true;
                indices.Add(block.Idx);
                for(int i = 0; i <indices.Count; i++) {
                    matchIndices?.Add(indices[i]);
                }
            }

            return res;
        }

        public bool Evaluator(BlockModel block, HashSet<int> matchIndices)
        {
            matchIndices?.Clear();

            if(block == null) {
                return EvalMatchBoard(matchIndices);
            }
            else {
                return EvalMatchBlock(block, matchIndices);
            }
        }
    }
}