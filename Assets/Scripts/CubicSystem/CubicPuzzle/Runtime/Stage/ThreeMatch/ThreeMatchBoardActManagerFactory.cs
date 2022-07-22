using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubicSystem.CubicPuzzle
{
    class ThreeMatchBoardActManagerFactory :IBoardActManagerFactory
    {
        public BoardActManager Create(BoardModel boardModel)
        {
            return new ThreeMatchBoardActManager(boardModel,
                new ThreeMatchHexEvaluator(boardModel),
                new HexDropDownAndFillEvent(boardModel));
        }
    }
}
