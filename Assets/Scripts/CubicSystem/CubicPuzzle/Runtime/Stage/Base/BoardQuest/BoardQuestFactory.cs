namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Board에 사용될 미션 생성기
     */
    public class BoardQuestFactory
    {

        public BoardQuest CreateBoardQuest(HexBoardModel board, BoardClearQuestData conditionData)
        {
            BoardQuest quest = null;

            switch(conditionData.clearType){
                //지정된 량 만큼 Block 파괴
                case BoardQuestType.DESTROY_COUNT: {
                    quest = new BoardQuest_DestroyBlockCount(board, conditionData.clearValue);
                    break;
                }
                case BoardQuestType.SCORE: {
                    break;
                }
                case BoardQuestType.NO_MATCH: {
                    break;
                }
            }

            return quest;
        }
    }
}
