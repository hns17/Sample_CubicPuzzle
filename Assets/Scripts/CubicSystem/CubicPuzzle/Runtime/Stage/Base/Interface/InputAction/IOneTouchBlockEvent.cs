using Cysharp.Threading.Tasks;


namespace CubicSystem.CubicPuzzle
{
    public interface IOneTouchBlockEvent
    {
        public UniTask DoTouchAction(BlockModel target);
    }

}
