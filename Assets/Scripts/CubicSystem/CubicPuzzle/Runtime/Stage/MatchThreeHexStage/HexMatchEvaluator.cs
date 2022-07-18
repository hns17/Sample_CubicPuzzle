
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Unity.Collections;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Board�� Block�� Match ���¸� ��
     *  @detail Line(����, �밢��, ���밢��) Mattch, Square Match(������ 4���� ��)�� �ִ��� 
     *          ���ϰ� ���� ���¸� Update
     */
    public class HexMatchEvaluator : IMatchEvaluator
    {
        private const int LineMatchCount = 3;

        /**
        *  @brief  Board�� ��ü Block�� ������� Line Match �򰡿� ���� NeighType List
        *  @detail  neighType {
        *       RIGHT_DOWN(�밢��)
        *       DOWN(����)
        *       LEFT_DOWN(�� �밢��)
        *  }
        */
        private readonly List<List<BlockNeighType>> BoardLineMatchNeighTypes = new List<List<BlockNeighType>>{
            new List<BlockNeighType>{ BlockNeighType.RIGHT_DOWN},
            new List<BlockNeighType>{ BlockNeighType.DOWN},
            new List<BlockNeighType>{ BlockNeighType.LEFT_DOWN}
        };


        /**
        *  @brief  Block �������� Line Match �򰡿� ���� NeighType List
        *  @detail  neighType {
        *       LEFT_UP, RIGHT_DOWN(�밢��)
        *       UP, DOWN(����)
        *       RIGHT_UP, LEFT_DOWN(�� �밢��)
        *  }
        */
        private readonly List<List<BlockNeighType>> BlockLineMatchNeighTypes = new List<List<BlockNeighType>>{
            new List<BlockNeighType>{ BlockNeighType.LEFT_UP, BlockNeighType.RIGHT_DOWN},
            new List<BlockNeighType>{ BlockNeighType.UP, BlockNeighType.DOWN},
            new List<BlockNeighType>{ BlockNeighType.RIGHT_UP, BlockNeighType.LEFT_DOWN}
        };


        /**
        *  @brief  Board�� ��ü Block�� ������� Square Match �򰡿� ���� NeighType List
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
         *  @brief  Block�� ������� Square Match �򰡿� ���� NeighType List
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

        //Match Block Index ���
        private Dictionary<MatchColorType, List<List<int>>> matchTable;
        private HashSet<int> memoMatchIndices;

        //Board �� �Լ� Set
        private List<Func<BlockModel, bool>> boardEvaluator;

        //Block �� �Լ� Set
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
        *  @brief  Board�� ��ġ Block�� Ȯ�� �� ���� ������Ʈ
        *  @return true(��ġ ���� �ִ� ���), false(��ġ ���� ���� ���)
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
       *  @brief  Block�� ��ġ ���� Ȯ�� �� ������Ʈ
       *  @param  block(Ȯ�� �� ��)
       *  @return true(��ġ ���� �ִ� ���), false(��ġ ���� ���� ���)
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
        *  @brief  ���� ������ ������ �������� Ȯ���Ѵ�.
        *  @return true(������ ���), false(�Ұ����� ���)
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

            //Board�� ��ü Block�� ������� Block ���� ��
            for(int i = 0; i < blocks.Count; i++) {
                if(!blocks[i].IsEnableBlock()) {
                    continue;
                }

                var blockIndex = blocks[i].Idx;
                
                //��� Block�� ��ü Neigh Block�� Swipe
                for(BlockNeighType neighType = BlockNeighType.START + 1; neighType < BlockNeighType.NONE; neighType++) {
                    await UniTask.Yield();

                    var neighBlock = board.GetNeighBlock(blocks[i], neighType);
                    if(neighBlock == null || !neighBlock.IsEnableBlock()) {
                        continue;
                    }
                    var neighIndex = neighBlock.Idx;

                    //��� ���� �̿� �� ��ġ Swap
                    board.SwapBlock(blockIndex, neighIndex);
                    
                    memoMatchIndices.Clear();

                    //��� Block Match ��
                    foreach(var eval in blockEvaluator) {
                        if(eval(blocks[i])) {
                            isMatched = true;
                        }
                    }

                    //Match ���� Update
                    if(memoMatchIndices.Count > 0) {
                        board.MatchHelper.UpdateMatchHelpInfo(blocks[blockIndex], blocks[neighIndex], memoMatchIndices);
                    }

                    //�� �� ���� ��ġ��
                    board.SwapBlock(blockIndex, neighIndex);
                }
            }

            memoMatchIndices.Clear();
            return isMatched;
        }


        /**
         *  @brief  Line �� �Լ�
         *  @block  target Block, neighTypes : ���� �̿� Block ����Ʈ
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
         *  @brief  ������ �̿� �������� Match�� ���� ���������� �ݺ������� ��
         *  @param  block : �� Block, blockNeighType : ���� �̿� Block ��ġ, indices : ������� ��ġ�� Block Index List
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
         *  @brief  Square Match ���ϱ�
         *  @param  block : targetBlock, neighTypes : ���� �̿� ���
         *  @return bool(true : Match, false : No Match)
         */
        private bool EvalSquareMatched(BlockModel block, List<BlockNeighType> neighTypes)
        {
            if(block == null || !block.IsEnableBlock()) {
                return false;
            }

            bool res = false;
            CubicNativeArray<int> indices = new CubicNativeArray<int>(board.BlockCount, Allocator.Temp);

            //Target Block�� �̿� Block ���ϱ�
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

            //Match�� ��� Block ���
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
         *  @brief  ��ϵ� Match Block�� ���¸� ����
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