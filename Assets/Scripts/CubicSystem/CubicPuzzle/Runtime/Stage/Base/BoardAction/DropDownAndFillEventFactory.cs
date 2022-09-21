using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  DropDownAndFillEvent 객체 생성을 위한 Factory
     */
    public class DropDownAndFillEventFactory :IFactory<BoardModel, IDropAndFillEvent>
    {
        public IDropAndFillEvent Create(BoardModel board)
        {
            if(board.BoardType == BoardType.HEX) {
                return new HexDropDownAndFillEvent(board);
            }
            else if(board.BoardType == BoardType.SQUARE) {
                return new SquareDropDownAndFillEvent(board);
            }

            return null;
        }
    }
}

