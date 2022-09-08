using UniRx;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  ������ ���� Block�� �ı��ϴ� �̼�
     */
    public class BoardQuest_DestroyBlockCount :BoardQuest
    {
        //��ǥ��
        private int clearValue = 0;

        //�޼���
        private int archiveValue = 0;

        public BoardQuest_DestroyBlockCount(BoardModel board, int clearValue)
        {
            this.clearValue = clearValue;

            //Board Block�� ���°� Destroy�� ���Ҷ� �޼� ��ġ +1
            var blocks = board.Blocks;
            blocks.ForEach(x => x.StateObservable.Subscribe(state=> {
                if(state == BlockState.DESTROYED) {
                    RunQuest(1);
                }
            }));

        }

        /**
         *  @brief  �޼� ��ġ ����
         *  @param  archive : ������
         */
        public void RunQuest(int archive)
        {
            archiveValue += archive;


            //��ǥ���� ä��� Complete�� true�� ����
            if(clearValue <= archiveValue) {
                isComplete.Value = true;
            }
        }
    }

}
