
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Cell ���� �� ���� ����
     */
    public class CellModel
    {
        //Cell�� Vertex ��ġ ����
        private readonly Vector2[] vertexPosition = {
            new Vector2(-0.35f, 0.575f),   //LEFT_UP
            new Vector2(0.35f, 0.575f),    //RIGHT_UP
            new Vector2(0.7f, 0f),         //RIGHT
            new Vector2(0.35f, -0.575f),   //RIGHT_DOWN
            new Vector2(-0.35f, -0.575f),  //LEFT_DOWN
            new Vector2(-0.7f, 0f),        //LEFT
        };

        //Block�� �����ϴ� Cell�ΰ�?
        public bool IsFillBlock { get; private set; }

        //Cell�� ��ġ ����
        //Value �� Observable ����
        private ReactiveProperty<Vector2> position = new ReactiveProperty<Vector2>(Vector2.zero);
        public Vector2 Position => position.Value;
        public IObservable<Vector2> PositionObservable => position;

        
        private ReactiveProperty<CellType> cellType = new ReactiveProperty<CellType>();
        public IObservable<CellType> CellTypeObservable => cellType;
        public CellType CellType
        {
            get { return cellType.Value; }
            private set
            {
                //CellType None�� ��� ������� �ʴ� Cell�� ó��
                cellType.Value = value;
                if(value == CellType.NONE) {
                    state.Value = CellState.EMPTY;
                }
            }
        }

        //Cell�� ���� ����
        //CellState Getter, Observable ����
        private ReactiveProperty<CellState> state = new ReactiveProperty<CellState>(CellState.NORMAL);
        public CellState State => state.Value;
        public IObservable<CellState> CellStateObservable => state;

        //���� �� �� ���� Ȯ�� ����
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
         *  @brief  Cell ���� �ʱ�ȭ
         */
        public void Initialize(BoardItemData itemData, Vector2 position)
        {
            this.position.Value = position;
            this.IsFillBlock = itemData.isFill;
            this.state.Value = CellState.NORMAL;
            this.CellType = itemData.cellType;

            foreach(var rateItem in itemData.fillRate) {
                if(rateItem.Value > 0f) {
                    BlockFillRate[rateItem.Key] = rateItem.Value;
                }
            }
        }

        /**
         *  @brief  vertex ��ġ ���� ��ȯ
         *  @param vertexIndex : ��ȯ�� vertex index
         *  @return Vector3 : Vertex Position
         */
        public Vector3 GetVertexPosition(int vetexIndex)
        {
            return Position + vertexPosition[vetexIndex];
        }

        /**
         *  @brief  Cell�� ���¸� Ȯ��
         *  @state  ���Ϸ��� ���� ����
         *  @return ������ ���(true) / �ٸ� ���(false)
         */
        public bool IsCompareState(CellState state)
        {
            if(this.state.Value == state) {
                return true;
            }
            return false;
        }

        /**
         *  @brief  Ȱ��ȭ �� Cell���� Ȯ��
         *  @return Ȱ��ȭ(true) / ��Ȱ��ȭ(false)
         */
        public bool IsEnableCell()
        {
            var state = this.State;

            if(CellType == CellType.NONE || state == CellState.EMPTY) {
                return false;
            }
            return true;
        }

        /**
         *  @brief  Cell ���� ����
         *  @param  cellState : ������ ����
         */
        public void SetCellState(CellState cellState)
        {
            this.state.Value = cellState;
        }

        /**
         *  @brief  Cell Type ����
         *  @param  cellState : ������ ����
         */
        public void SetCellType(CellType cellType)
        {
            this.cellType.Value = cellType;
        }

        public class Factory :PlaceholderFactory<BoardItemData, Transform, Vector2, CellModel>
        {

        }
    }
}

