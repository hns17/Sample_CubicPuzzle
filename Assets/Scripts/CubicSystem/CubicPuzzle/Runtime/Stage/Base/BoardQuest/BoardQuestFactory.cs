namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Board�� ���� �̼� ������
     */
    public class BoardQuestFactory
    {

        public BoardQuest CreateBoardQuest(HexBoardModel board, BoardClearQuestData conditionData)
        {
            BoardQuest quest = null;

            switch(conditionData.clearType){
                //������ �� ��ŭ Block �ı�
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
