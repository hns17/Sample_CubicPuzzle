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
    public class HexBoardModel : BoardModel
    {
        public override Vector2 CellSize => new Vector2(1f, 1.15f);
        public override BoardType BoardType => BoardType.HEX;

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
                    boardItem.cellStyle = CellStyle.HEX;

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
         *  @brief  target Block의 최상단에 위치한 Block Index 반환
         *  @param  targetIndex : Target Block Index
         *  @return int : 최상단 Block Index
         */
        private int GetTopBlockIndex(int targetIndex)
        {
            return targetIndex % Col;
        }

        /**
         *  @brief  target Block의 최하단에 위치한 Block Index 반환
         *  @param  targetIndex : Target Block Index
         *  @return int : 최하단 Block Index
         */
        private int GetFirstBottomBlockIndex(int targetIndex)
        {
            return Col * Row - GetTopBlockIndex(targetIndex);
        }


        /**
         *  @brief  Block의 Line 위치를 반환
         *  @param  blockIdx : Target Block Index
         *  @return int : Line Index
         */
        public int GetLineIndex(int blockIdx)
        {
            int lineIdx = blockIdx / Col * 2;
            int mod = blockIdx % Col;
            if(mod % 2 != 0) {
                lineIdx += 1;
            }

            return lineIdx;
        }
    }
}

