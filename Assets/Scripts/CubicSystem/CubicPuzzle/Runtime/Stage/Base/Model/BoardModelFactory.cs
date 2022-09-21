using UnityEngine;
using Zenject;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  BoardModel CustomFactory
     */
    public class BoardModelFactory :IFactory<PuzzleBoardInfo, Transform, BoardModel>
    {
        [Inject] private DiContainer container;

        /**
         *  @brief  BoardModel 생성
         *  @param  boardInfo : board data
         *  @param  parent : presenter 생성에 사용될 parent 정보(StagePresenter Transform)
         *  @return BoardModel
         */
        public BoardModel Create(PuzzleBoardInfo boardInfo, Transform parent)
        {
            if(boardInfo.boardData.boardType == BoardType.HEX){
                return container.Instantiate<HexBoardModel>(new object[] { boardInfo, parent });
            }
            else if(boardInfo.boardData.boardType == BoardType.SQUARE) {
                return container.Instantiate<SquareBoardModel>(new object[] { boardInfo, parent });
            }

            return null;
        }
    }
}
