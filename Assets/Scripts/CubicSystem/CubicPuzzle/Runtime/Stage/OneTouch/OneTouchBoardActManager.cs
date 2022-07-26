using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Zenject;

namespace CubicSystem.CubicPuzzle
{
    public class OneTouchBoardActManager :BoardActManager, IOneTouchBlockEvent
    {

        public async UniTask DoTouchAction(BlockModel target)
        {

            if(target == null) {
                return;
            }

            if(!board.IsBoardBlock(target)) {
                return;
            }

            if(EvaluatorNUpdate(target)) {
                await MatchEvent();
            }
        }

        public override async UniTask MatchEvent()
        {
            cts.Token.ThrowIfCancellationRequested();
            board.SetBoardState(BoardState.MATCH_EVENT);
            
            //Start Block Destory
            await DestroyMatchBlocks();

            //Start Drop And Fille
            await eventDropNFill.StartDropAndFill();

            if(!board.CheckClearQuest()) {
                if(!await IsPossibleBoard()) {
                    await NoMoreMatchEvent();
                    //var blocks = board.Blocks;
                    //blocks.ForEach(block =>
                    //{
                    //    if(block.IsEnableBlock()) {
                    //        block.SetBlockState(BlockState.MATCH);
                    //        block.SetMatchColor(MatchColorType.NONE);
                    //    }
                    //});
                    //await MatchEvent();
                }
            }
            board.SetBoardState(BoardState.READY);
        }

        protected override async UniTask<bool> IsPossibleBoard()
        {
            await UniTask.Yield();
            return Evaluator(null);
        }

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
