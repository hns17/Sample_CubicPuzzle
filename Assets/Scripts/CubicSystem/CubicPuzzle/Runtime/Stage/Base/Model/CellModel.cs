
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Cell 상태 및 정보 관리
     */
    public class CellModel
    {
        private const int destroyDelay = 500;

        //Cell의 Hex Vertex 위치 정보
        private readonly Vector2[] vertexPositionHex = {
            new Vector2(-0.35f, 0.575f),   //LEFT_UP
            new Vector2(0.35f, 0.575f),    //RIGHT_UP
            new Vector2(0.7f, 0f),         //RIGHT
            new Vector2(0.35f, -0.575f),   //RIGHT_DOWN
            new Vector2(-0.35f, -0.575f),  //LEFT_DOWN
            new Vector2(-0.7f, 0f),        //LEFT
        };

        //Cell의 Square Vertex 위치 정보
        private readonly Vector2[] vertexPositionSquare = {
            new Vector2(-0.5f, 0.475f),   //LEFT_UP
            new Vector2(0.5f, 0.475f),    //RIGHT_UP
            new Vector2(0.5f, -0.475f),   //RIGHT_DOWN
            new Vector2(-0.5f, -0.475f),  //LEFT_DOWN
        };

        //Block을 생성하는 Cell인가?
        public bool IsFillBlock { get; private set; }

        //Cell의 위치 정보
        //Value 및 Observable 공개
        private ReactiveProperty<Vector2> position = new ReactiveProperty<Vector2>(Vector2.zero);
        public Vector2 Position => position.Value;
        public IObservable<Vector2> PositionObservable => position;

        //Cell의 Style 정보, Sprite에 대응
        //Value 및 Observable 공개
        private ReactiveProperty<CellStyle> cellStyle = new ReactiveProperty<CellStyle>(CellStyle.NONE);
        public CellStyle Style => cellStyle.Value;
        public IObservable<CellStyle> CellStyleObservable => cellStyle;

        //Cell의 Type 정보
        //Value 및 Observable 공개
        private ReactiveProperty<CellType> cellType = new ReactiveProperty<CellType>();
        public IObservable<CellType> CellTypeObservable => cellType;
        public CellType CellType
        {
            get { return cellType.Value; }
            private set
            {
                //CellType None인 경우 사용하지 않는 Cell로 처리
                cellType.Value = value;
                if(value == CellType.NONE) {
                    state.Value = CellState.EMPTY;
                }
            }
        }

        //Cell의 상태 정보
        //CellState Getter, Observable 공개
        private ReactiveProperty<CellState> state = new ReactiveProperty<CellState>(CellState.NORMAL);
        public CellState State => state.Value;
        public IObservable<CellState> CellStateObservable => state;

        //색상 별 블럭 생성 확률 정보
        public Dictionary<MatchColorType, float> BlockFillRate { get; private set; }

        [Inject]
        private void InjectDependencies(BoardItemData itemData, Transform parent, Vector2 position, CellPresenter.Factory factory)
        {
            //Make Presenter
            factory.Create(this, parent);

            BlockFillRate = new Dictionary<MatchColorType, float>();
            Initialize(itemData, position);
        }

        /**
         *  @brief  Cell 정보 초기화
         *  @param  itemData : 생성된 Board 정보
         *  @param  position : 초기 위치
         */
        public void Initialize(BoardItemData itemData, Vector2 position)
        {
            this.position.Value = position;
            this.IsFillBlock = itemData.isFill;
            this.state.Value = CellState.NORMAL;
            this.CellType = itemData.cellType;
            this.cellStyle.Value = itemData.cellStyle;

            foreach(var rateItem in itemData.fillRate) {
                if(rateItem.Value > 0f) {
                    BlockFillRate[rateItem.Key] = rateItem.Value;
                }
            }
        }

        /**
         *  @brief  vertex 위치 정보 반환
         *  @param vertexIndex : 반환할 vertex index
         *  @return Vector3 : Vertex Position
         */
        public Vector3 GetVertexPosition(int vetexIndex)
        {
            Vector2 localPos = CellStyle.HEX == cellStyle.Value ? vertexPositionHex[vetexIndex] 
                                                : vertexPositionSquare[vetexIndex];
            return Position + localPos;
        }

        /**
         *  @brief  Cell의 상태를 확인
         *  @param  state : 비교하려는 상태 정보
         *  @return bool : 동일한 경우(true) / 다른 경우(false)
         */
        public bool IsCompareState(CellState state)
        {
            if(this.state.Value == state) {
                return true;
            }
            return false;
        }

        /**
         *  @brief  활성화 된 Cell인지 확인
         *  @return bool : 활성화(true) / 비활성화(false)
         */
        public bool IsEnableCell()
        {
            CellState state = this.State;

            if(CellType == CellType.NONE || state == CellState.EMPTY) {
                return false;
            }
            return true;
        }

        /**
         *  @brief  Cell 상태 변경
         *  @param  cellState : 변경할 상태
         */
        public void SetCellState(CellState cellState)
        {
            this.state.Value = cellState;
        }

        /**
         *  @brief  Cell Type 변경
         *  @param  cellState : 변경할 상태
         */
        public void SetCellType(CellType cellType)
        {
            this.cellType.Value = cellType;
        }


        /**
         *  @brief  Cell 파괴 이벤트
         */
        public async UniTask DestroyCell()
        {
            state.Value = CellState.DESTROYED;
            await UniTask.Delay(destroyDelay);
            state.Value = CellState.EMPTY;
        }


        public class Factory :PlaceholderFactory<BoardItemData, Transform, Vector2, CellModel>
        {

        }
    }
}

