using System;
using System.Collections.Generic;
using Unity.Collections;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Board�� Block�� Match ���¸� ��
     *  @detail Line(����, �밢��, ���밢��) Match, Square Match(������ 4���� ��)�� �ִ��� 
     *          ���ϰ� ���� ���¸� Update
     */
    public class ThreeMatchEvaluator :IMatchEvaluator
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
        private readonly Dictionary<BoardType, List<List<BlockNeighType>>> BoardLineMatchNeighTypes =
            new Dictionary<BoardType, List<List<BlockNeighType>>> {
                {
                    BoardType.HEX, new List<List<BlockNeighType>> {
                        new List<BlockNeighType>{ BlockNeighType.RIGHT_DOWN},
                        new List<BlockNeighType>{ BlockNeighType.DOWN},
                        new List<BlockNeighType>{ BlockNeighType.LEFT_DOWN}
                    }
                },
                {
                    BoardType.SQUARE, new List<List<BlockNeighType>>{
                        new List<BlockNeighType>{ BlockNeighType.RIGHT},
                        new List<BlockNeighType>{ BlockNeighType.DOWN}
                    }
                }
            };


        /**
        *  @brief  Block �������� Line Match �򰡿� ���� NeighType List
        *  @detail  neighType {
        *       LEFT_UP, RIGHT_DOWN(�밢��)
        *       UP, DOWN(����)
        *       RIGHT_UP, LEFT_DOWN(�� �밢��)
        *  }
        */
        private readonly Dictionary<BoardType, List<List<BlockNeighType>>> BlockLineMatchNeighTypes =
            new Dictionary<BoardType, List<List<BlockNeighType>>> {
                {
                    BoardType.HEX, new List<List<BlockNeighType>> {
                        new List<BlockNeighType>{ BlockNeighType.LEFT_UP, BlockNeighType.RIGHT_DOWN},
                        new List<BlockNeighType>{ BlockNeighType.UP, BlockNeighType.DOWN},
                        new List<BlockNeighType>{ BlockNeighType.RIGHT_UP, BlockNeighType.LEFT_DOWN}
                    }
                },
                {
                    BoardType.SQUARE, new List<List<BlockNeighType>> {
                        new List<BlockNeighType>{ BlockNeighType.LEFT, BlockNeighType.RIGHT},
                        new List<BlockNeighType>{ BlockNeighType.UP, BlockNeighType.DOWN}
                    }
                }
            };


        /**
        *  @brief  Board�� ��ü Block�� ������� Square Match �򰡿� ���� NeighType List
        *  @detail {
        *      RightUp, RightDown, Down
        *      RightDown, Down, LeftDown
        *      Down, LeftDown, LeftUp
        *  }
        */
        private readonly Dictionary<BoardType, List<List<BlockNeighType>>> BoardSquareMatchNeighTypes =
            new Dictionary<BoardType, List<List<BlockNeighType>>> {
                {
                    BoardType.HEX, new List<List<BlockNeighType>>{
                         new List<BlockNeighType>{ BlockNeighType.RIGHT_UP, BlockNeighType.RIGHT_DOWN, BlockNeighType.DOWN},
                         new List<BlockNeighType>{ BlockNeighType.RIGHT_DOWN, BlockNeighType.DOWN, BlockNeighType.LEFT_DOWN},
                         new List<BlockNeighType>{ BlockNeighType.DOWN, BlockNeighType.LEFT_DOWN, BlockNeighType.LEFT_UP}
                    }
                },
                {
                    BoardType.SQUARE, new List<List<BlockNeighType>>()
                }
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
        private readonly Dictionary<BoardType, List<List<BlockNeighType>>> BlockSquareMatchNeighTypes =
            new Dictionary<BoardType, List<List<BlockNeighType>>> {
                {
                    BoardType.HEX, new List<List<BlockNeighType>>{
                         new List<BlockNeighType>{ BlockNeighType.LEFT_UP, BlockNeighType.UP, BlockNeighType.RIGHT_UP},
                         new List<BlockNeighType>{ BlockNeighType.UP, BlockNeighType.RIGHT_UP, BlockNeighType.RIGHT_DOWN},
                         new List<BlockNeighType>{ BlockNeighType.RIGHT_UP, BlockNeighType.RIGHT_DOWN, BlockNeighType.DOWN},
                         new List<BlockNeighType>{ BlockNeighType.RIGHT_DOWN, BlockNeighType.DOWN, BlockNeighType.LEFT_DOWN},
                         new List<BlockNeighType>{ BlockNeighType.DOWN, BlockNeighType.LEFT_DOWN, BlockNeighType.LEFT_UP},
                         new List<BlockNeighType>{ BlockNeighType.LEFT_DOWN, BlockNeighType.LEFT_UP, BlockNeighType.UP}
                    }
                },
                {
                    BoardType.SQUARE, new List<List<BlockNeighType>>()
                }
            };


        private BoardModel board;


        public ThreeMatchEvaluator(BoardModel board)
        {
            this.board = board;
        }


        /**
        *  @brief  Board�� ��ġ Block�� Ȯ�� �� ���� ������Ʈ
        *  @return true(��ġ ���� �ִ� ���), false(��ġ ���� ���� ���)
        */
        public bool EvalMatchBoard(HashSet<int> matchIndices)
        {
            bool res = false;
            var blocks = board.Blocks;

            List<List<BlockNeighType>> boardLineMatchNeighTypes = null;
            BoardLineMatchNeighTypes.TryGetValue(board.BoardStyle, out boardLineMatchNeighTypes);

            List<List<BlockNeighType>> boardSquareMatchNeighTypes = null;
            BoardSquareMatchNeighTypes.TryGetValue(board.BoardStyle, out boardSquareMatchNeighTypes);

            foreach(var block in blocks) {
                if(!block.IsEnableBlock()) {
                    continue;
                }

                //eval line match
                foreach(var matchNeighTypes in boardLineMatchNeighTypes) {
                    res |= EvalLineMatched(block, matchNeighTypes, matchIndices);
                }

                //eval square match
                foreach(var matchNeighTypes in boardSquareMatchNeighTypes) {
                    res |= EvalSquareMatched(block, matchNeighTypes, matchIndices);
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
            List<List<BlockNeighType>> blockLineMatchNeighTypes = null;
            BlockLineMatchNeighTypes.TryGetValue(board.BoardStyle, out blockLineMatchNeighTypes);

            List<List<BlockNeighType>> blockSquareMatchNeighTypes = null;
            BlockSquareMatchNeighTypes.TryGetValue(board.BoardStyle, out blockSquareMatchNeighTypes);


            if(block.IsEnableBlock()) {
                //eval line match
                foreach(var matchNeighTypes in blockLineMatchNeighTypes) {
                    res |= EvalLineMatched(block, matchNeighTypes, matchIndices);
                }

                //eval square match
                foreach(var matchNeighTypes in blockSquareMatchNeighTypes) {
                    res |= EvalSquareMatched(block, matchNeighTypes, matchIndices);
                    for(BlockNeighType i = BlockNeighType.START; i < BlockNeighType.NONE; i++) {
                        var neighBlock = board.GetNeighBlock(block, i);
                        res |= EvalSquareMatched(neighBlock, matchNeighTypes, matchIndices);
                    }
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
                for(int i = 0; i < indices.Count; i++) {
                    matchIndices?.Add(indices[i]);
                }
            }

            return res;
        }

        /**
         *  @brief  3-Match �ּ� ����(3-Line, 4-Square) üũ
         *  @param  block : target block, null�� ��� Board ��ü üũ
         *  @param  matchIndices : Match�� Block List
         *  @return true : Match Block�� �ִ� ��� / false : ���� ���
         */
        public bool Evaluator(BlockModel block, HashSet<int> matchIndices)
        {
            matchIndices?.Clear();

            //��� ���� ������� ��ġ �˻�
            if(block == null) {
                return EvalMatchBoard(matchIndices);
            }
            //������ ���� ������� ��ġ �˻�
            else {
                return EvalMatchBlock(block, matchIndices);
            }
        }
    }
}