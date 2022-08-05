using Cysharp.Threading.Tasks;


namespace CubicSystem.CubicPuzzle
{
    public interface IDropAndFillEvent
    {
        public UniTask StartDropAndFill();
    }
}
