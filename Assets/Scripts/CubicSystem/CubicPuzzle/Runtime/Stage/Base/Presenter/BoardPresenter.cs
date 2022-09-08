using Cysharp.Threading.Tasks;
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
        
        protected BoardModel board;
        //Touch Hit�� Block
        protected BlockPresenter fromBlock = null;

        public Transform Cells { get; private set; }
        public Transform Blocks { get; private set; }

        [Inject] protected StageInputManager inputManager;

        [Inject]
        private void InjectDependencies(BoardModel boardModel, Transform parent)
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

        /**
         *  @brief  Awake Initalize
         */
        public virtual void InitalizeAwake()
        {
            Cells = new GameObject("Cells").transform;
            Cells.SetParent(this.transform);
            Cells.localPosition = Vector2.zero;

            Blocks = new GameObject("Blocks").transform;
            Blocks.SetParent(this.transform);
            Blocks.localPosition = Vector2.zero;
        }


        /**
         *  @brief  Start Initalize
         */
        public virtual void InitalizeStart()
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

        /**
         *  @author hns17@naver.com
         *  @brief  ��ġ ���� �̺�Ʈ
         */
        protected virtual void TouchStart()
        {
            if(board.State == BoardState.READY) {
                //��ġ�� �� ���� ��������
                fromBlock = GetHitBlock(inputManager.GetTouchPoisition());

                if(fromBlock != null) {
                    inputManager.OnPerformed += TouchPosition_performed;
                }
            }
        }

        /**
         *  @author hns17@naver.com
         *  @brief  ��ġ ���� �̺�Ʈ
         */
        protected virtual void TouchCancel()
        {
            inputManager.OnPerformed -= TouchPosition_performed;
        }

        /**
         *  @author hns17@naver.com
         *  @brief  ��ġ �����̵� �̺�Ʈ
         */
        protected virtual void TouchPosition_performed()
        {
            
        }

        /**
         *  @brief  Touch Block ��������
         *  @param  pos : Touch Position
         */
        protected BlockPresenter GetHitBlock(Vector2 pos)
        {
            BlockPresenter res = null;

            //Simulation : �� ������ �������� Collider ��ġ ���߱�
            Physics2D.Simulate(Time.fixedDeltaTime);


            Vector2 worldPos = Camera.main.ScreenToWorldPoint(pos);
            Ray2D ray2D = new Ray2D(worldPos, Vector2.zero);
            RaycastHit2D hit = Physics2D.Raycast(ray2D.origin, ray2D.direction);

            //Simulation : RayCast Hit
            Physics2D.Simulate(Time.fixedDeltaTime);
            if(hit) {
                res = hit.collider.gameObject.GetComponent<BlockPresenter>();

                //���� Board�� Block���� Ȯ���ϱ�
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

