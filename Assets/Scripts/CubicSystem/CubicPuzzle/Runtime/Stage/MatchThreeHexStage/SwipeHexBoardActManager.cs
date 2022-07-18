using Cysharp.Threading.Tasks;
using Zenject;

namespace CubicSystem.CubicPuzzle
{
    class SwipeHexBoardActManager :BoardActManager, ISwipeEvent
    {
        public SwipeHexBoardActManager(BoardModel board, IMatchEvaluator matchEvaluator, IDropAndFillEvent eventDropNFill) 
            : base(board, matchEvaluator, eventDropNFill)
        {
        }

        /**
         *  @brief  Block Swipe Event
         *  @param  from : 선택된 Block, to : 교환될 Block
         */
        public async UniTask DoSwipeAction(BlockModel from, BlockModel to)
        {
            if(board.State == BoardState.READY) {
                //두 Block이 이웃 관계인지 확인
                for(BlockNeighType neighType = BlockNeighType.START;
                        neighType < BlockNeighType.NONE; neighType++) {

                    int fromIdx = from.Idx;
                    int toIdx = to.Idx;

                    //이웃 관계인 경우 SwipeEvent
                    if(board.GetNeighIndex(fromIdx, neighType) == toIdx) {
                        board.SetBoardState(BoardState.SWIPE);
                        board.SwapBlock(from.Idx, to.Idx);

                        //Match Check
                        bool isMatch = EvalNUpdateMatchBlock(from);
                        isMatch |= EvalNUpdateMatchBlock(to);

                        //Move Block
                        await (
                            from.MoveBlock(to.Position, 0.25f, !isMatch),
                            to.MoveBlock(from.Position, 0.25f, !isMatch)
                        );

                        //Match되지 않은 경우 Board Container의 Block 정보를 이전 상태로 
                        if(isMatch) {
                            await MatchEvent();
                        }
                        else {
                            board.SwapBlock(from.Idx, to.Idx);
                        }

                        board.SetBoardState(BoardState.READY);
                        break;
                    }
                }
            }
        }

        public class Factory :PlaceholderFactory<BoardModel, BoardActManager>
        {

        }
    }
}
