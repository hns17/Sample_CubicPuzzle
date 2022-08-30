using Cysharp.Threading.Tasks;
using Zenject;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  OneTouch Event Manager
     */
    public class OneTouchBoardActManager :BoardActManager, IOneTouchBlockEvent
    {
        /**
         *  @brief  User Touch Input
         *  @param  target(BlockModel) : Touch Block
         */
        public async UniTask DoTouchAction(BlockModel target)
        {
            if(target == null || !board.IsBoardBlock(target)) {
                return;
            }

            //Target 중심으로 Match 평가하기
            if(Evaluator(target, true)) {
                await MatchEvent();
            }
        }

        /**
         *  @brief  Match Event
         */
        public override async UniTask MatchEvent()
        {
            cts.Token.ThrowIfCancellationRequested();
            board.SetBoardState(BoardState.MATCH_EVENT);
            
            //Start Block Destory
            await DestroyMatchBlocks(board.GetMatchBlocks());

            //Start Drop And Fille
            await eventDropNFill.StartDropAndFill();

            if(!board.CheckClearQuest()) {
                if(!await IsPossibleBoard()) {
                    await NoMoreMatchEvent();
                }
            }
            board.SetBoardState(BoardState.READY);
        }


        protected override async UniTask<bool> IsPossibleBoard()
        {
            await UniTask.Yield();
            return Evaluator(null, false);
        }


        /**
         *  @brief   CustomFactoryClass
         */
        public new class Factory :IFactory<BoardModel, BoardActManager>
        {
            [Inject] private DiContainer diContainer;
            public BoardActManager Create(BoardModel param)
            {
                var newActManager = diContainer.Instantiate<OneTouchBoardActManager>(new object[] { param });

                newActManager.matchEvaluator = new OneTouchMatchEvaluator(param);
                newActManager.eventDropNFill = new HexDropDownAndFillEvent(param);

                return newActManager;
            }
        }
    }
}
