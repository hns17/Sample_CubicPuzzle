using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace CubicSystem.CubicPuzzle
{
    public class DropDownAndFillEventFactory :IFactory<BoardModel, IDropAndFillEvent>
    {
        public IDropAndFillEvent Create(BoardModel board)
        {
            if(board.BoardStyle == BoardType.HEX) {
                return new HexDropDownAndFillEvent(board);
            }
            else if(board.BoardStyle == BoardType.SQUARE) {
                return new SquareDropDownAndFillEvent(board);
            }

            return null;
        }
    }
}

