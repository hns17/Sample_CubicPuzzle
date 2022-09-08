using Zenject;

namespace CubicSystem.CubicPuzzle
{
    public class ThreeMatchEvaluatorFactory :IFactory<BoardModel, IMatchEvaluator>
    {
        public IMatchEvaluator Create(BoardModel board)
        {
            if(board.BoardStyle == BoardType.HEX) {
                return new ThreeMatchHexEvaluator(board);
            }
            else if(board.BoardStyle == BoardType.SQUARE) {
                return new ThreeMatchSquareEvaluator(board);
            }

            return null;
        }
    }
}
