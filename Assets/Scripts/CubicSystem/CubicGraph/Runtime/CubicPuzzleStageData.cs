using System;
using System.Collections.Generic;
using UnityEngine;


namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Game에 사용될 Stage 정보
     */
    //[CreateAssetMenu(fileName = "StageData", menuName = "CubicGraph/StageData", order = 1)]
    public class CubicPuzzleStageData :ScriptableObject
    {
        public string stageName = "Stage";
        public List<PuzzleBoardInfo> boards = new List<PuzzleBoardInfo>();

        //단계별 Board Index 정보
        public List<ListContainer<int>> phaseInfos = new List<ListContainer<int>>();
    }

    /**
     *  @brief  Stage에 사용되는 Board 및 연결된 Link Board 정보
     */
    [Serializable]
    public class PuzzleBoardInfo
    {
        public string guid;

        //연결된 prev board indices
        public List<int> prevBoardIndices = new List<int>();

        //연결된 next board indices
        public List<int> nextBoardIndices = new List<int>();

        //Board Data
        public BoardData boardData = null;
    }


    /**
     *  @brief  Board 구성에 필요한 Data
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
     *  @brief  Board의 Cell & Block 정보
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

        //채워지는 Block의 색상 Rate
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