using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Zenject;

namespace CubicSystem.CubicPuzzle
{
    public class ThreeMatchBoardPresenter :BoardPresenter
    {
        //Touch를 기다린 시간
        private float touchWaitTime = 0f;
        //Guide 활성화 시간
        private float helpInfoShowTime = 10f;

        private ISwipeBlockEvent swipeEvent;

        [SerializeField] private BoardMatchHelper matchHelper;

        public override void InitalizeStart()
        {
            base.InitalizeStart();
            swipeEvent = board.ActManager as ThreeMatchBoardActManager;
        }

        private void OnEnable()
        {
            touchWaitTime = 0;
        }

        private void LateUpdate()
        {
            //Touch 대기시간 계산 후 Guide 표시
            if(board.State == BoardState.READY) {
                touchWaitTime += Time.fixedDeltaTime;

                //오랜시간 Touch가 없으면 Guide 활성화
                if(touchWaitTime >= helpInfoShowTime) {
                    ShowHelpInfo(true);
                }
            }
        }

        protected override void TouchStart()
        {
            if(board.State == BoardState.READY) {
                base.TouchStart();

                if(fromBlock != null) {
                    //Touch 입력시 Guide 비활성화
                    ShowHelpInfo(false);
                }
            }
        }

        protected override void TouchPosition_performed()
        {
            BlockPresenter toBlock = GetHitBlock(inputManager.GetTouchPoisition());

            if(toBlock != null && toBlock != fromBlock) {
                inputManager.OnPerformed -= TouchPosition_performed;

                //Do Swipe
                swipeEvent?.DoSwipeAction(fromBlock.Block, toBlock.Block).Forget();
            }
        }

        /**
        *  @brief  Match 가능한 블럭 화면에 표시
        *  @param  isShow(true : 표시, false : 비활성화)
        */
        protected void ShowHelpInfo(bool isShow)
        {
            //Enable HelpInfo
            if(isShow) {
                matchHelper.ShowMatchHelper(swipeEvent.GetMatchHelpInfo());
            }
            //Disable Help Info
            else {
                touchWaitTime = 0;
                matchHelper.ShowMatchHelper(null);
            }
        }
    }
}
