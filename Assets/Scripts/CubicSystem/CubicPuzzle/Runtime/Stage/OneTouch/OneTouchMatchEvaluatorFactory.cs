using Zenject;

namespace CubicSystem.CubicPuzzle
{
    public class OneTouchMatchEvaluatorFactory :IFactory<BoardModel, IMatchEvaluator>
    {
        public IMatchEvaluator Create(BoardModel board)
        {
            return new OneTouchMatchEvaluator(board);
        }
    }
}
