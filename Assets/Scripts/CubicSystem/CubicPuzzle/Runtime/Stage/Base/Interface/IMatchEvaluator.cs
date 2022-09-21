using System.Collections.Generic;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Match Evaluator Interface
     */
    public interface IMatchEvaluator
    {
        /**
         *  @param block : 평가할 targetBlock
         *  @param matchIndices : Matched Block Indices
         *  @return bool : 매치된 블럭이 있는 경우(true) / 없는 경우(false)
         */
        public bool Evaluator(BlockModel block, HashSet<int> matchIndices);
    }
}
