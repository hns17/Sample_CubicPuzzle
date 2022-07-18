
using Cysharp.Threading.Tasks;
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
    public class HexMatchEvaluator : IMatchEvaluator
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
        };


        private BoardModel board;

        //Match Block Index 기록
        private Dictionary<MatchColorType, List<List<int>>> matchTable;
        private HashSet<int> memoMatchIndices;

        //Board 평가 함수 Set
        private List<Func<BlockModel, bool>> boardEvaluator;

        //Block 평가 함수 Set
        private List<Func<BlockModel, bool>> blockEvaluator;
        //private List<Func<BlockModel, bool, bool>> possibleEvaluator;

        public HexMatchEvaluator(BoardModel board)
        {
            this.board = board;
            memoMatchIndices = new HashSet<int>();

            //var aa = UniTaskHelper.Func<BlockModel, UniTask<bool>>
            //    (async (block) => await EvalLineMatched(block, neighTypes));

            boardEvaluator = new List<Func<BlockModel, bool>>();
            //Make BoardLineEvaluator
            foreach(var matchNeighTypes in BoardLineMatchNeighTypes) {
                boardEvaluator.Add((block) =>
                {
                    return EvalLineMatched(block, matchNeighTypes);
                });
            }
            //Make Board SquareEvaluator
            foreach(var matchNeighTypes in BoardSquareMatchNeighTypes) {
                boardEvaluator.Add((block) =>
                {
                    return EvalSquareMatched(block, matchNeighTypes);
                });
            }


            blockEvaluator = new List<Func<BlockModel, bool>>();
            //Make Block LineEvaluator
            foreach(var matchNeighTypes in BlockLineMatchNeighTypes) {
                blockEvaluator.Add((block) =>
                {
                    return EvalLineMatched(block, matchNeighTypes);
                });
            }
            //Make Block SquareEvaluator
            foreach(var matchNeighTypes in BlockSquareMatchNeighTypes) {
                blockEvaluator.Add((block) =>
                {
                    bool res = EvalSquareMatched(block, matchNeighTypes);
                    for(BlockNeighType i = BlockNeighType.START + 1; i<BlockNeighType.NONE; i++) {
                        var neighBlock = board.GetNeighBlock(block, i);
                        res |= EvalSquareMatched(neighBlock, matchNeighTypes);
                    }
                    return res;
                });
            }
        }


        /**
        *  @brief  Board의 매치 Block을 확인 후 상태 업데이트
        *  @return true(매치 블럭이 있는 경우), false(매치 블럭이 없는 경우)
        */
        public bool EvalNUpdateMatchBoard()
        {
            bool res = false;
            var blocks = board.Blocks;

            foreach(var block in blocks) {
                if(!block.IsEnableBlock()) {
                    continue;
                }

                foreach(var eval in boardEvaluator) {
                    res |= eval(block);
                }
            }

            UpdateMatchBlockState();
            return res;
        }

        /**
       *  @brief  Block의 매치 상태 확인 후 업데이트
       *  @param  block(확인 할 블럭)
       *  @return true(매치 블럭이 있는 경우), false(매치 블럭이 없는 경우)
       */
        public bool EvalNUpdateMatchBlock(BlockModel block)
        {
            bool res = false;
            if(block.IsEnableBlock()) {
                foreach(var eval in blockEvaluator) {
                    res |= eval(block);
                }
            }

            UpdateMatchBlockState();

            return res;
        }


        /**
        *  @brief  게임 진행이 가능한 보드인지 확인한다.
        *  @return true(가능한 경우), false(불가능한 경우)
        */
        public UniTask<bool> EvalPossibleBoard()
        {
            return CreateEvalPossibleBoard();
        }

        public async UniTask<bool> CreateEvalPossibleBoard()
        {
            var blocks = board.Blocks;
            bool isMatched = false;
            board.MatchHelper.Clear();

            //Board의 전체 Block을 대상으로 Block 단위 평가
            for(int i = 0; i < blocks.Count; i++) {
                if(!blocks[i].IsEnableBlock()) {
                    continue;
                }

                var blockIndex = blocks[i].Idx;
                
                //대상 Block을 전체 Neigh Block과 Swipe
                for(BlockNeighType neighType = BlockNeighType.START + 1; neighType < BlockNeighType.NONE; neighType++) {
                    await UniTask.Yield();

                    var neighBlock = board.GetNeighBlock(blocks[i], neighType);
                    if(neighBlock == null || !neighBlock.IsEnableBlock()) {
                        continue;
                    }
                    var neighIndex = neighBlock.Idx;

                    //대상 블럭과 이웃 블럭 위치 Swap
                    board.SwapBlock(blockIndex, neighIndex);
                    
                    memoMatchIndices.Clear();

                    //대상 Block Match 평가
                    foreach(var eval in blockEvaluator) {
                        if(eval(blocks[i])) {
                            isMatched = true;
                        }
                    }

                    //Match 정보 Update
                    if(memoMatchIndices.Count > 0) {
                        board.MatchHelper.UpdateMatchHelpInfo(blocks[blockIndex], blocks[neighIndex], memoMatchIndices);
                    }

                    //평가 후 원래 위치로
                    board.SwapBlock(blockIndex, neighIndex);
                }
            }

            memoMatchIndices.Clear();
            return isMatched;
        }


        /**
         *  @brief  Line 평가 함수
         *  @block  target Block, neighTypes : 평가할 이웃 Block 리스트
         */
        private bool EvalLineMatched(BlockModel block, List<BlockNeighType> neighTypes)
        {
            bool res = false;
            
            //Initialize Match index List
            CubicNativeArray<int> indices = new CubicNativeArray<int>(board.BlockCount, Allocator.Temp);
            indices.Add(block.Idx);

            //Make Match index List
            for(int i = 0; i < neighTypes.Count; i++) {
                RecursiveEvalLineBlockMatched(block, neighTypes[i], ref indices);
            }

            //Is Pass Match Condition Count
            if(indices.Count >= LineMatchCount) {
                res = true;
                foreach(int index in indices) {
                    memoMatchIndices.Add(index);
                }
            }

            //Release Index List
            indices.Dispose();
            return res;
        }

        /**
         *  @brief  지정된 이웃 방향으로 Match가 되지 않을때까지 반복적으로 평가
         *  @param  block : 평가 Block, blockNeighType : 비교할 이웃 Block 위치, indices : 현재까지 매치된 Block Index List
         */
        private void RecursiveEvalLineBlockMatched(BlockModel block, BlockNeighType blockNeighType, ref CubicNativeArray<int> indices)
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
        private bool EvalSquareMatched(BlockModel block, List<BlockNeighType> neighTypes)
        {
            if(block == null || !block.IsEnableBlock()) {
                return false;
            }

            bool res = false;
            CubicNativeArray<int> indices = new CubicNativeArray<int>(board.BlockCount, Allocator.Temp);

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
                foreach(int index in indices) {
                    memoMatchIndices.Add(index);
                }
            }
            indices.Dispose();

            return res;
        }

        /**
         *  @brief  기록된 Match Block의 상태를 변경
         */
        public void UpdateMatchBlockState()
        {
            var blocks = board.Blocks;
            foreach(int index in memoMatchIndices) {
                blocks[index].SetBlockState(BlockState.MATCH);
            }
            memoMatchIndices.Clear();
        }
    }
}