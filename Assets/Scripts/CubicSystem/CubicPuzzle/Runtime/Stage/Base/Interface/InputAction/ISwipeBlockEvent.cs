using Cysharp.Threading.Tasks;

namespace CubicSystem.CubicPuzzle
{
    public interface ISwipeBlockEvent
    {
        public ThreeMatchHelpInfo GetMatchHelpInfo();
        public UniTask DoSwipeAction(BlockModel from, BlockModel to);
    }
}
