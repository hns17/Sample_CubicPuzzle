using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace CubicSystem.CubicPuzzle
{
    public interface IMatchEvaluator
    {
        public bool Evaluator(BlockModel block, HashSet<int> matchIndices);
    }
}
