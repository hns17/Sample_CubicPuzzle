using Cysharp.Threading.Tasks;

namespace CubicSystem.CubicPuzzle
{
    interface ISwipeEvent
    {
        public UniTask DoSwipeAction(BlockModel from, BlockModel to);
    }
}
