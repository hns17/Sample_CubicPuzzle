using Cysharp.Threading.Tasks;
using System.Threading;

namespace CubicSystem.CubicPuzzle
{
    public interface ISwipeBlockEvent
    {
        public MatchHelpInfo GetMatchHelpInfo();
        public UniTask DoSwipeAction(BlockModel from, BlockModel to);
    }
}
