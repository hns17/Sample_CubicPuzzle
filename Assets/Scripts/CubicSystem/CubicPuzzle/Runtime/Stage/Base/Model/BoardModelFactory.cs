using UnityEngine;
using Zenject;

namespace CubicSystem.CubicPuzzle
{
    public class BoardModelFactory :IFactory<PuzzleBoardInfo, Transform, BoardModel>
    {
        [Inject] private DiContainer container;
        public BoardModel Create(PuzzleBoardInfo param1, Transform param2)
        {
            if(param1.boardData.boardType == BoardType.HEX){
                return container.Instantiate<HexBoardModel>(new object[] { param1, param2 });
            }
            else if(param1.boardData.boardType == BoardType.SQUARE) {
                return container.Instantiate<SquareBoardModel>(new object[] { param1, param2 });
            }

            return null;
        }
    }
}
