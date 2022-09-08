using System;
using System.Collections.Generic;
using UnityEngine;


namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Game�� ���� Stage ����
     */
    //[CreateAssetMenu(fileName = "StageData", menuName = "CubicGraph/StageData", order = 1)]
    public class CubicPuzzleStageData :ScriptableObject
    {
        public string stageName = "Stage";
        public List<PuzzleBoardInfo> boards = new List<PuzzleBoardInfo>();

        //�ܰ躰 Board Index ����
        public List<ListContainer<int>> phaseInfos = new List<ListContainer<int>>();
    }

    /**
     *  @brief  Stage�� ���Ǵ� Board �� ����� Link Board ����
     */
    [Serializable]
    public class PuzzleBoardInfo
    {
        public string guid;

        //����� prev board indices
        public List<int> prevBoardIndices = new List<int>();

        //����� next board indices
        public List<int> nextBoardIndices = new List<int>();

        //Board Data
        public BoardData boardData = null;
    }


    /**
     *  @brief  Board ������ �ʿ��� Data
     */
    [Serializable]
    public class BoardData
    {
        public Vector3 position;

        [HideInInspector] public BoardType boardType;
        [HideInInspector] public BoardClearQuestData clearQuestData;

        [HideInInspector] public int col;
        [HideInInspector] public int row;

        //Cell & Block Info
        [HideInInspector] public List<BoardItemData> items;


        public void Initialize(BoardType boardType, int col, int row)
        {
            this.col = col;
            this.row = row;

            this.boardType = boardType;

            this.clearQuestData ??= new BoardClearQuestData();

            this.items ??= new List<BoardItemData>();
            this.items.Clear();

            CellStyle cStyle = boardType == BoardType.HEX ? CellStyle.HEX : CellStyle.SQUARE;

            //Create Default Board ItemData
            for(int i = 0; i < col * row; i++) {
                this.items.Add(new BoardItemData() { 
                    cellStyle = cStyle
                });
            }
        }
    }


    /**
     *  @brief  Board�� Cell & Block ����
     */
    [System.Serializable]
    public class BoardItemData
    {
        //Cell Info
        public CellStyle cellStyle = CellStyle.NONE;
        public CellType cellType = CellType.NORMAL;

        //Block Info
        public BlockType blockType = BlockType.NORMAL;
        public MatchColorType color = MatchColorType.RED;

        //Enable Fill Block
        public bool isFill = false;

        //ä������ Block�� ���� Rate
        public SerializeDictionary<MatchColorType, float> fillRate
             = new SerializeDictionary<MatchColorType, float>();
    }

    [Serializable]
    public class BoardClearQuestData
    {
        public int clearValue = 0;
        public BoardQuestType clearType = BoardQuestType.NONE;
    }
}