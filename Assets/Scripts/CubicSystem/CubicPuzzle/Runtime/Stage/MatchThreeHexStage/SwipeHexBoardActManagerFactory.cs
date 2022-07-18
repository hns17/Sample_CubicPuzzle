using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubicSystem.CubicPuzzle
{
    class SwipeHexBoardActManagerFactory :IBoardActManagerFactory
    {
        public BoardActManager Create(BoardModel boardModel)
        {
            return new SwipeHexBoardActManager(boardModel,
                new HexMatchEvaluator(boardModel),
                new HexDropAndFillEvent(boardModel));
        }
    }
}
