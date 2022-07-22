using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CubicSystem.CubicPuzzle
{
    public class OneTouchBoardActManager :BoardActManager, IOneTouchBlockEvent
    {
        public OneTouchBoardActManager(BoardModel board, IMatchEvaluator matchEvaluator, IDropAndFillEvent eventDropNFill) : base(board, matchEvaluator, eventDropNFill)
        {
        }

        public async UniTask DoTouchAction(BlockModel target)
        {
            Debug.Log("DoTouchAction : " + board.State);

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
    }

}
