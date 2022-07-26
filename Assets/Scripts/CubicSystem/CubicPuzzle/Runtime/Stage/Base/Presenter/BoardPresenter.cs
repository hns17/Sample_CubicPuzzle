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
        
        protected BoardModel board;
        //Touch Hit된 Block
        protected BlockPresenter fromBlock = null;

        public Transform Cells { get; private set; }
        public Transform Blocks { get; private set; }

        [Inject] protected StageInputManager inputManager;

        [Inject]
        private void InjectDependencies(HexBoardModel boardModel, Transform parent)
        {
            this.board = boardModel;
            transform.SetParent(parent);
        }

        private void Awake()
        {
            InitalizeAwake();
        }

        private void Start()
        {
            InitalizeStart();
        }

        public virtual void InitalizeAwake()
        {
            Cells = new GameObject("Cells").transform;
            Cells.SetParent(this.transform);
            Cells.localPosition = Vector2.zero;

            Blocks = new GameObject("Blocks").transform;
            Blocks.SetParent(this.transform);
            Blocks.localPosition = Vector2.zero;
        }

        public virtual void InitalizeStart()
        {
            Debug.Assert(inputManager != null, "InputManager is null");

            //Regist Touch Input Event
            inputManager.OnPressDown += TouchStart;
            inputManager.OnPressUp += TouchCancel;

            //Board 파괴 이벤트
            board.StateObservable.Subscribe(state =>
            {
                Physics2D.simulationMode = SimulationMode2D.Script;
                if(state == BoardState.DESTROYED) {
                    gameObject.SetActive(false);
                    //Destroy(gameObject);
                }
                else if(state == BoardState.READY) {
                    Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
                    gameObject.SetActive(true);
                }
            }).AddTo(this);

            board.PositionObservable.Subscribe(position =>
            {
                transform.localPosition = position;
            });
        }

        /**
         *  @author hns17@naver.com
         *  @brief  터치 시작 이벤트
         */
        protected virtual void TouchStart()
        {
            if(board.State == BoardState.READY) {
                //터치된 블럭 정보 가져오기
                fromBlock = GetHitBlock(inputManager.GetTouchPoisition());

                if(fromBlock != null) {
                    inputManager.OnPerformed += TouchPosition_performed;
                }
            }
        }

        /**
         *  @author hns17@naver.com
         *  @brief  터치 종료 이벤트
         */
        protected virtual void TouchCancel()
        {
            inputManager.OnPerformed -= TouchPosition_performed;
        }

        /**
         *  @author hns17@naver.com
         *  @brief  터치 슬라이드 이벤트
         */
        protected virtual void TouchPosition_performed()
        {
            
        }

        /**
         *  @brief  Touch Block 가져오기
         *  @param  pos : Touch Position
         */
        protected BlockPresenter GetHitBlock(Vector2 pos)
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

        public class Factory : PlaceholderFactory<BoardModel, Transform, BoardPresenter>
        {

        }
    }
}

