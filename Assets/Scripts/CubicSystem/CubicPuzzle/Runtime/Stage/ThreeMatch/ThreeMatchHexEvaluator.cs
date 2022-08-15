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
    public class ThreeMatchHexEvaluator : IMatchEvaluator
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
             new List<BlockNeighType>{ BlockNeighType.LEFT_DOWN, BlockNeighType.LEFT_UP, BlockNeighType.UP},
        };


        private BoardModel board;

        //Board �� �Լ� Set
        private List<Func<BlockModel, HashSet<int>, bool>> boardEvaluator;

        //Block �� �Լ� Set
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
        *  @brief  Board�� ��ġ Block�� Ȯ�� �� ���� ������Ʈ
        *  @return true(��ġ ���� �ִ� ���), false(��ġ ���� ���� ���)
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
       *  @brief  Block�� ��ġ ���� Ȯ�� �� ������Ʈ
       *  @param  block(Ȯ�� �� ��)
       *  @return true(��ġ ���� �ִ� ���), false(��ġ ���� ���� ���)
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
         *  @brief  Line �� �Լ�
         *  @block  target Block, neighTypes : ���� �̿� Block ����Ʈ
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
         *  @brief  ������ �̿� �������� Match�� ���� ���������� �ݺ������� ��
         *  @param  block : �� Block, blockNeighType : ���� �̿� Block ��ġ, indices : ������� ��ġ�� Block Index List
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
         *  @brief  Square Match ���ϱ�
         *  @param  block : targetBlock, neighTypes : ���� �̿� ���
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