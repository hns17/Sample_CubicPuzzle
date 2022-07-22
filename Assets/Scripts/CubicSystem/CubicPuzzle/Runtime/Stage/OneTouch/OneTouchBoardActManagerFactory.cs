using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CubicSystem.CubicPuzzle
{
    public class OneTouchBoardActManagerFactory :IBoardActManagerFactory
    {
        public BoardActManager Create(BoardModel boardModel)
        {
            return new OneTouchBoardActManager(boardModel,
                        new OneTouchMatchEvaluator(boardModel),
                        new HexDropDownAndFillEvent(boardModel));
        }
    }

}
