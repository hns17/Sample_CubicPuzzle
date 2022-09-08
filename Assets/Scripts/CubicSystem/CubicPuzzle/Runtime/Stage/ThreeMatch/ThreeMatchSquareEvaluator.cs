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
    public class ThreeMatchSquareEvaluator : IMatchEvaluator
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
            new List<BlockNeighType>{ BlockNeighType.RIGHT},
            new List<BlockNeighType>{ BlockNeighType.DOWN}
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
            new List<BlockNeighType>{ BlockNeighType.LEFT, BlockNeighType.RIGHT},
            new List<BlockNeighType>{ BlockNeighType.UP, BlockNeighType.DOWN}
        };


        private BoardModel board;

        //Board �� �Լ� Set
        private List<Func<BlockModel, HashSet<int>, bool>> boardEvaluator;

        //Block �� �Լ� Set
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
         *  @brief  3-Match �ּ� ����(3-Line) üũ
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