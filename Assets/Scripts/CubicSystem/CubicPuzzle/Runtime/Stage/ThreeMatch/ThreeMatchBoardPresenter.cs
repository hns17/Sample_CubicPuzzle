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
        //Touch�� ��ٸ� �ð�
        private float touchWaitTime = 0f;
        //Guide Ȱ��ȭ �ð�
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
            //Touch ���ð� ��� �� Guide ǥ��
            if(board.State == BoardState.READY) {
                touchWaitTime += Time.fixedDeltaTime;

                //�����ð� Touch�� ������ Guide Ȱ��ȭ
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
                    //Touch �Է½� Guide ��Ȱ��ȭ
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
        *  @brief  Match ������ �� ȭ�鿡 ǥ��
        *  @param  isShow(true : ǥ��, false : ��Ȱ��ȭ)
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
