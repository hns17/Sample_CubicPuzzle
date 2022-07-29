using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace CubicSystem.CubicPuzzle
{
    public class OneTouchBoardPresenter :BoardPresenter
    {
        private IOneTouchBlockEvent oneTouchEvent;

        public override void InitalizeStart()
        {
            base.InitalizeStart();
            oneTouchEvent = board.ActManager as OneTouchBoardActManager;
        }

        protected override void TouchStart()
        {
            if(board.State == BoardState.READY) {
                base.TouchStart();
                if(fromBlock != null) {
                    //Call TouchEvent
                    oneTouchEvent.DoTouchAction(fromBlock.Block).Forget();
                }
            }
        }
    }
}
