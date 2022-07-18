
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace CubicSystem.CubicPuzzle
{
    public abstract class BoardModel
    {
        public PuzzleBoardInfo BoardInfo { get; protected set; }

        //Board의 Size
        public int Row { get; protected set; }
        public int Col { get; protected set; }

        public virtual Vector2 CellSize => Vector2.one;

        //Board에 사용되는 Cell 정보 보관
        public List<CellModel> Cells { get; protected set; }

        //Board에 사용되는 Block 정보 보관
        public List<BlockModel> Blocks { get; protected set; }

        //전체 Block Count
        public int BlockCount => Blocks.Count;

        //현재 Board 상태 확인을 위한 Ractive용 변수
        //값 확인 및 이벤트 등록을 위해 Value와 Observable만 공개
        private ReactiveProperty<BoardState> boardState = new ReactiveProperty<BoardState>(BoardState.INITIALIZE);
        public BoardState State => boardState.Value;
        public IObservable<BoardState> StateObservable => boardState;


        protected ReactiveProperty<Vector2> position = new ReactiveProperty<Vector2>();
        public IObservable<Vector2> PositionObservable => position;

        //Board의 전체 이벤트 처리기(Swipe, Match Check, Destroy, Drop&Fill) 
        public BoardActManager ActManager { get; protected set; }

        //매치 가능한 블럭 정보를 기록
        public MatchHelpInfo MatchHelper { get; protected set; }

        //Board Clear Event 관리
        protected BoardQuest clearQuest;

        /**
         *  @brief  Board의 상태 변경
         *  @param  state : 변경할 상태
         */
        public void SetBoardState(BoardState state)
        {
            boardState.Value = state;
        }

        /**
         *  @brief  Check Board Clear Quest
         *  @return bool : true(clear), false(not clear)
         */
        public bool CheckClearQuest()
        {
            if(clearQuest == null) {
                return false;
            }

            if(clearQuest.IsComplete) {
                SetBoardState(BoardState.CLEAR);
                return true;
            }
            return false;
        }


        public async virtual UniTask DestroyBoard()
        {
            //모든 블럭 파괴
            Blocks.ForEach(x => {
                if(x.IsEnableBlock()) {
                    x.SetBlockState(BlockState.DESTROYED);
                }
            });

            //파괴될 때까지 대기
            bool isDestroyComplete = false;
            do {
                await UniTask.Yield();
                isDestroyComplete = false;
                Blocks.ForEach(x => isDestroyComplete |= x.IsEnableBlock());
            } while(isDestroyComplete);

            await UniTask.Delay(500);

            //모든 Cell 파괴
            Cells.ForEach(x => {
                if(x.IsEnableCell()) {
                    x.SetCellState(CellState.DESTROYED);
                }
            });

            //파괴될 때까지 대기
            do {
                await UniTask.Yield();
                isDestroyComplete = false;
                Cells.ForEach(x => isDestroyComplete |= x.IsEnableCell());
            } while(isDestroyComplete);

            SetBoardState(BoardState.DESTROYED);
        }

        /**
         *  @brief  지정된 Cell의 위치 정보 반환
         *  @param  idx(int) : Cell Index
         *  @return Vector2 : Cell Position
         */
        public Vector2 GetCellPosition(int idx)
        {
            return Cells[idx].Position;
        }

        /**
         *  @brief  Target Block이 현재 Board에 포함된 Block인지 판단
         *  @param  block : 확인할 Block
         *  @return true(포함) / false(현재 보드에 없는 Block)
         */
        public bool IsBoardBlock(BlockModel block)
        {
            return Blocks.IndexOf(block) >= 0 ? true : false;
        }

        /**
         *  @brief  Block의 이웃 Block 정보를 반환
         *  @param  target : target Block, neighType : 찾으려는 이웃 블럭 위치 정보
         *  @return BlockModel : 정보가 없는 경우 null / 있는 경우 BlockModel 정보 반환
         */
        public BlockModel GetNeighBlock(BlockModel target, BlockNeighType neighType)
        {
            BlockModel res = null;
            if(target != null) {
                var idx = GetNeighIndex(target.Idx, neighType);
                res = idx >= 0 ? Blocks[idx] : null;
            }

            return res;
        }

        /**
         *  @brief  Block Container의 Block 위치를 교환
         *  @detail 계산 및 탐색의 효율을 위해 Block Container의 Block은 순서를 유지하도록 규칙을 정함.
         *          Swipe시 두 Block이 교체되면 Container의 위치 정보 및 Block Index도 교체.
         *  @param  firstIdx & secondIdx : 교체할 Block의 Index 정보
         */
        public void SwapBlock(int firstIdx, int secondIdx)
        {
            //Block Container 정보 Swap
            var temp = Blocks[firstIdx];
            Blocks[firstIdx] = Blocks[secondIdx];
            Blocks[secondIdx] = temp;

            //Change Block Index
            Blocks[firstIdx]?.SetIndex(firstIdx);
            Blocks[secondIdx]?.SetIndex(secondIdx);
        }


        /**
         *  @brief      Board 정보 초기화
         */
        public virtual void Initialize(PuzzleBoardInfo boardInfo)
        {
        }


        /**
         *  @brief  Block의 이웃 Block Index 정보를 반환
         *  @param  targetIndex : target Block index, neighType : 찾으려는 이웃 블럭 위치 정보
         *  @return int : 정보가 없는 경우 -1 / 있는 경우 이웃 block index 정보 반환
         */
        public abstract int GetNeighIndex(int targetIndex, BlockNeighType neighType);

        /**
         *  @brief  Board에 사용될 Cell & Block 생성
         *  @param  boardItems : Cell & Block 정보
         */
        protected abstract void BuildBoard(List<BoardItemData> boardItems);

        /**
         *  @brief  더 이상 Match 가능한 Block이 없는 경우 
         *          호출하는 함수
         */
        public abstract UniTask NoMoreMatchEvent();
    }
}
