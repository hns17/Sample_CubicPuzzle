using Cysharp.Threading.Tasks;

namespace CubicSystem.CubicPuzzle
{
    public interface IMatchEvaluator
    {
        public bool EvalNUpdateMatchBoard();
        public bool EvalNUpdateMatchBlock(BlockModel block);
        public UniTask<bool> EvalPossibleBoard();
    }
}
