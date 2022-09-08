using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System;
using UniRx;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  BoardModel Data ���� �� ó��
     *  @detail ������ ��� �̺�Ʈ(Swipe, Block Drop, Destroy, Fill)�� ó���ϸ�
     *          ���忡 ���Ǵ� Block �� Cell ������ ����
     */
    public class HexBoardModel : BoardModel
    {
        public override Vector2 CellSize => new Vector2(1f, 1.15f);
        public override BoardType BoardStyle => BoardType.HEX;

        /**
         *  @brief  Board�� ���� Cell & Block ����
         *  @param  boardItems : Cell & Block ����
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

            //Cell�� ũ�⿡ ���� �ʱ� ��ġ ���� ����
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

                    //������� �ʴ� Cell�� Block�� ������ �ʵ��� ���� ����
                    //Editor Tool���� Export �� �� ó���ǵ��� ���� �ʿ�
                    if(!newCellModel.IsEnableCell()) {
                        newBlockModel.SetBlockType(BlockType.NONE);
                    }

                    index++;
                }
            }
        }


        /**
         *  @brief  target Block�� �ֻ�ܿ� ��ġ�� Block Index ��ȯ
         *  @param  targetIndex : Target Block Index
         *  @return int : �ֻ�� Block Index
         */
        private int GetTopBlockIndex(int targetIndex)
        {
            return targetIndex % Col;
        }

        /**
         *  @brief  target Block�� ���ϴܿ� ��ġ�� Block Index ��ȯ
         *  @param  targetIndex : Target Block Index
         *  @return int : ���ϴ� Block Index
         */
        private int GetFirstBottomBlockIndex(int targetIndex)
        {
            return Col * Row - GetTopBlockIndex(targetIndex);
        }


        /**
         *  @brief  Block�� Line ��ġ�� ��ȯ
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

