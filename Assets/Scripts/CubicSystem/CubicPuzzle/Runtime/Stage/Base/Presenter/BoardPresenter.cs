using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UniRx;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Board GameObject, 
     *          Board의 Touch Event 및 HelpLine 표시 등을 처리
     */
    public class BoardPresenter :MonoBehaviour
    {
        //Touch를 기다린 시간
        private float touchWaitTime = 0f;

        //Guide 활성화 시간
        private float helpInfoShowTime = 10f;
        private HexBoardModel board;
        private ISwipeEvent swipeEvent;

        public Transform Cells { get; private set; }
        public Transform Blocks { get; private set; }

        //Touch Hit된 Block
        private BlockPresenter fromBlock = null;

        [SerializeField] private BoardMatchHelper matchHelper;

        [Inject] private StageInputManager inputManager;

        [Inject]
        private void InjectDependencies(HexBoardModel boardModel, Transform parent)
        {
            this.board = boardModel;
            transform.SetParent(parent);

        }

        private void Awake()
        {
            Cells = new GameObject("Cells").transform;
            Cells.SetParent(this.transform);
            Cells.localPosition = Vector2.zero;

            Blocks = new GameObject("Blocks").transform;
            Blocks.SetParent(this.transform);
            Blocks.localPosition = Vector2.zero;

            swipeEvent = board.ActManager as SwipeHexBoardActManager;
        }

        private void Start()
        {
            Debug.Assert(inputManager != null, "InputManager is null");
            
            //Regist Touch Input Event
            inputManager.OnPressDown += TouchStart;
            inputManager.OnPressUp += TouchCancel;

            //Board 파괴 이벤트
            board.StateObservable.Subscribe(state =>
            {
                if(state == BoardState.DESTROYED) {
                    gameObject.SetActive(false);
                    //Destroy(gameObject);
                }
                else if(state == BoardState.READY) {
                    gameObject.SetActive(true);
                }
            }).AddTo(this);

            board.PositionObservable.Subscribe(position =>
            {
                transform.localPosition = position;
            });
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

        /**
         *  @author hns17@naver.com
         *  @brief  터치 시작 이벤트
         */
        private void TouchStart()
        {
            if(board.State == BoardState.READY) {
                //터치된 블럭 정보 가져오기
                fromBlock = GetHitBlock(inputManager.GetTouchPoisition());

                if(fromBlock != null) {
                    //Touch 입력시 Guide 비활성화
                    ShowHelpInfo(false);
                    inputManager.OnPerformed += TouchPosition_performed;
                }
            }
        }

        /**
         *  @author hns17@naver.com
         *  @brief  터치 종료 이벤트
         */
        private void TouchCancel()
        {
            inputManager.OnPerformed -= TouchPosition_performed;
        }

        /**
         *  @author hns17@naver.com
         *  @brief  터치 슬라이드 이벤트
         */
        private void TouchPosition_performed()
        {
            BlockPresenter toBlock = GetHitBlock(inputManager.GetTouchPoisition());

            if(toBlock != null && toBlock != fromBlock) {
                inputManager.OnPerformed -= TouchPosition_performed;

                //Do Swipe
                swipeEvent?.DoSwipeAction(fromBlock.Block, toBlock.Block).Forget();
            }
        }

        /**
         *  @brief  Touch Block 가져오기
         *  @param  pos : Touch Position
         */
        private BlockPresenter GetHitBlock(Vector2 pos)
        {
            BlockPresenter res = null;

            Vector2 worldPos = Camera.main.ScreenToWorldPoint(pos);
            Ray2D ray2D = new Ray2D(worldPos, Vector2.zero);
            RaycastHit2D hit = Physics2D.Raycast(ray2D.origin, ray2D.direction);

            if(hit) {
                res = hit.collider.gameObject.GetComponent<BlockPresenter>();

                //현재 Board의 Block인지 확인하기
                if(!board.IsBoardBlock(res.Block)) {
                    res = null;
                }
            }

            return res;
        }


        /**
         *  @brief  Match 가능한 블럭 화면에 표시
         *  @param  isShow(true : 표시, false : 비활성화)
         */
        private void ShowHelpInfo(bool isShow)
        {
            //Enable HelpInfo
            if(isShow) {
                matchHelper.ShowMatchHelper(board.MatchHelper);
            }
            //Disable Help Info
            else {
                touchWaitTime = 0;
                matchHelper.ShowMatchHelper(null);
            }
        }

        public class Factory : PlaceholderFactory<HexBoardModel, Transform, BoardPresenter>
        {

        }
    }
}

