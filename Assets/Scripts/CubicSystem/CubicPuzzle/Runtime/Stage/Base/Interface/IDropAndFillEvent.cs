using Cysharp.Threading.Tasks;


namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  DropAndFillEvent Interface
     */
    public interface IDropAndFillEvent
    {
        public UniTask StartDropAndFill();
    }
}
