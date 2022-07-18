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
     *          Board�� Touch Event �� HelpLine ǥ�� ���� ó��
     */
    public class BoardPresenter :MonoBehaviour
    {
        //Touch�� ��ٸ� �ð�
        private float touchWaitTime = 0f;

        //Guide Ȱ��ȭ �ð�
        private float helpInfoShowTime = 10f;
        private HexBoardModel board;
        private ISwipeEvent swipeEvent;

        public Transform Cells { get; private set; }
        public Transform Blocks { get; private set; }

        //Touch Hit�� Block
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

            //Board �ı� �̺�Ʈ
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
            //Touch ���ð� ��� �� Guide ǥ��
            if(board.State == BoardState.READY) {
                touchWaitTime += Time.fixedDeltaTime;

                //�����ð� Touch�� ������ Guide Ȱ��ȭ
                if(touchWaitTime >= helpInfoShowTime) {
                    ShowHelpInfo(true);
                }
            }
        }

        /**
         *  @author hns17@naver.com
         *  @brief  ��ġ ���� �̺�Ʈ
         */
        private void TouchStart()
        {
            if(board.State == BoardState.READY) {
                //��ġ�� �� ���� ��������
                fromBlock = GetHitBlock(inputManager.GetTouchPoisition());

                if(fromBlock != null) {
                    //Touch �Է½� Guide ��Ȱ��ȭ
                    ShowHelpInfo(false);
                    inputManager.OnPerformed += TouchPosition_performed;
                }
            }
        }

        /**
         *  @author hns17@naver.com
         *  @brief  ��ġ ���� �̺�Ʈ
         */
        private void TouchCancel()
        {
            inputManager.OnPerformed -= TouchPosition_performed;
        }

        /**
         *  @author hns17@naver.com
         *  @brief  ��ġ �����̵� �̺�Ʈ
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
         *  @brief  Touch Block ��������
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

                //���� Board�� Block���� Ȯ���ϱ�
                if(!board.IsBoardBlock(res.Block)) {
                    res = null;
                }
            }

            return res;
        }


        /**
         *  @brief  Match ������ �� ȭ�鿡 ǥ��
         *  @param  isShow(true : ǥ��, false : ��Ȱ��ȭ)
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

