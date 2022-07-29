using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UniRx;
using System;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  BoardModel Data 관리 및 처리
     *  @detail 보드의 모든 이벤트(Swipe, Block Drop, Destroy, Fill)를 처리하며
     *          보드에 사용되는 Block 및 Cell 정보도 관리
     */
    public class HexBoardModel : BoardModel
    {
        public override Vector2 CellSize => new Vector2(1f, 1.15f);

        //Cell & Block Creator
        private Func<BoardItemData, Vector2, CellModel> funcCreateCell;
        private Func<BoardItemData, Vector2, BlockModel> funcCreateBlock;

        //[Field Injection]
        //Cell & Block Model Factory
        [Inject] private CellModel.Factory cellFactory;
        [Inject] private BlockModel.Factory blockFactory;
        [Inject] private BoardQuestFactory questFactory;
        [Inject] private CTSManager ctsManager;


        [Inject]
        private void InjectDependencies(Transform parent,
                                    PuzzleBoardInfo boardInfo,
                                    BoardActManager.Factory actManagerFactory,
                                    BoardPresenter.Factory boardPresenterFactory)
        {
            ActManager = actManagerFactory.Create(this);

            //Board GameObject 생성
            var boardPresenter = boardPresenterFactory.Create(this, parent);

            //Cell Creator
            funcCreateCell = (boardItem, position) =>
            {
                return cellFactory.Create(boardItem, boardPresenter.Cells, position);
            };

            //Block Creator
            funcCreateBlock = (boardItem, position) =>
            {
                return blockFactory.Create(boardItem, boardPresenter.Blocks, position);
            };

            Initialize(boardInfo);
        }

        /**
         *  @brief      Board 정보 초기화
         */
        public override void Initialize(PuzzleBoardInfo boardInfo)
        {
            SetBoardState(BoardState.INITIALIZE);
            this.BoardInfo = boardInfo;
            this.position.Value = BoardInfo.boardData.position;

            var boardData = BoardInfo.boardData;
            this.Row = boardData.row;
            this.Col = boardData.col;

            //Make Board
            BuildBoard(boardData.items);

            //Make Board Clear Quest
            this.clearQuest = questFactory?.CreateBoardQuest(this, boardData.clearQuestData);

            ActManager.Initalize();
            SetBoardState(BoardState.READY);

            //MatchEvent
            UniTask.Action(async () =>
            {
                await ActManager.MatchEvent();
                SetBoardState(BoardState.READY);
            }).Invoke();
        }

        /**
         *  @brief  Board에 사용될 Cell & Block 생성
         *  @param  boardItems : Cell & Block 정보
         */
        protected override void BuildBoard(List<BoardItemData> boardItems)
        {
            Cells ??= new List<CellModel>();
            Blocks ??= new List<BlockModel>();


            Cells.ForEach((item) =>
            {
                item.SetCellType(CellType.NONE);
            });

            Blocks.ForEach((item) =>
            {
                item.SetBlockType(BlockType.NONE);
            });

            //Cell의 크기에 맞춰 초기 위치 정보 구성
            var cellSize = CellSize;
            Vector2 cellPos = new Vector2(-(Col / 2 * cellSize.x), Row / 2 * cellSize.y);

            int index = 0;
            for(int i = 0; i < Row; i++) {
                for(int j = 0; j < Col; j++) {
                    var boardItem = boardItems[index];

                    //Calc Cell & Block Position
                    Vector2 targetPos = new Vector2(cellPos.x + cellSize.x * j, cellPos.y - cellSize.y * i);
                    targetPos.y -= j % 2 * cellSize.y * 0.5f;

                    //Create Cell
                    CellModel newCellModel = null;
                    if(Cells.Count > index) {
                        newCellModel = Cells[index];
                        Cells[index].Initialize(boardItem, targetPos);
                    }
                    else {
                        newCellModel = funcCreateCell(boardItem, targetPos);
                        Cells.Add(newCellModel);
                    }

                    //Create Block
                    BlockModel newBlockModel = null;
                    if(Blocks.Count > index) {
                        newBlockModel = Blocks[index];
                        newBlockModel.Initialize(boardItem, targetPos);
                    }
                    else {
                        newBlockModel = funcCreateBlock(boardItem, targetPos); ;
                        Blocks.Add(newBlockModel);
                    }
                    
                    newBlockModel.SetIndex(index);

                    //사용하지 않는 Cell의 Block은 사용되지 않도록 상태 변경
                    //Editor Tool에서 Export 될 때 처리되도록 수정 필요
                    if(!newCellModel.IsEnableCell()) {
                        newBlockModel.SetBlockType(BlockType.NONE);
                    }

                    index++;
                }
            }
        }


        /**
         *  @brief  Block의 이웃 Block Index 정보를 반환
         *  @param  targetIndex : target Block index, neighType : 찾으려는 이웃 블럭 위치 정보
         *  @return int : 정보가 없는 경우 -1 / 있는 경우 이웃 block index 정보 반환
         */
        public override int GetNeighIndex(int targetIndex, BlockNeighType neighType)
        {
            int res = -1;
            int mod = targetIndex % Col;

            //Up Block
            if(neighType == BlockNeighType.UP) {
                res = targetIndex - Col;
            }
            //Down Block
            else if(neighType == BlockNeighType.DOWN) {
                res = targetIndex + Col;
            }
            //Left Side(LeftUp, LeftDown)
            else if(neighType == BlockNeighType.LEFT_UP || neighType == BlockNeighType.LEFT_DOWN) {
                if(mod != 0) {
                    res = targetIndex - 1;

                    //Left Up Block
                    if(neighType == BlockNeighType.LEFT_UP && mod % 2 == 0) {
                        res -= Col;
                    }
                    //Left Down Block
                    else if(neighType == BlockNeighType.LEFT_DOWN && mod % 2 != 0) {
                        res += Col;
                    }
                }
            }
            //Right Side(RightUp, RightDown)
            else if(neighType == BlockNeighType.RIGHT_UP || neighType == BlockNeighType.RIGHT_DOWN) {
                if((targetIndex + 1) % Col != 0) {
                    res = targetIndex + 1;

                    //Right Down Block
                    if(neighType == BlockNeighType.RIGHT_DOWN && mod % 2 != 0) {
                        res += Col;
                    }
                    //Right Up Block
                    else if(neighType == BlockNeighType.RIGHT_UP && mod % 2 == 0) {
                        res -= Col;
                    }
                }
            }

            //이웃 Block을 찾지 못한 경우
            if(res >= Blocks.Count || res < 0) {
                res = -1;
            }

            return res;
        }


    }
}

