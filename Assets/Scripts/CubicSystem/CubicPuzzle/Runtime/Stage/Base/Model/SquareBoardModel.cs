using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System;
using UniRx;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  BoardModel Data 관리 및 처리
     *  @detail 보드의 모든 이벤트(Swipe, Block Drop, Destroy, Fill)를 처리하며
     *          보드에 사용되는 Block 및 Cell 정보도 관리
     */
    public class SquareBoardModel : BoardModel
    {
        public override Vector2 CellSize => new Vector2(1f, 1f);
        public override BoardType BoardStyle => BoardType.SQUARE;

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
            Vector2 cellSize = CellSize;
            Vector2 cellPos = new Vector2(-(Col / 2 * cellSize.x), Row / 2 * cellSize.y);

            int index = 0;
            for(int i = 0; i < Row; i++) {
                for(int j = 0; j < Col; j++) {
                    BoardItemData boardItem = boardItems[index];
                    boardItem.cellStyle = CellStyle.SQUARE;

                    //Calc Cell & Block Position
                    Vector2 targetPos = new Vector2(cellPos.x + cellSize.x * j, cellPos.y - cellSize.y * i);
                    
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
    }
}

