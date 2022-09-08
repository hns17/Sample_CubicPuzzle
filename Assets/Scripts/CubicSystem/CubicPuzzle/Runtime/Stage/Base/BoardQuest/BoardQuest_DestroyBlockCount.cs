using UniRx;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  지정된 량의 Block을 파괴하는 미션
     */
    public class BoardQuest_DestroyBlockCount :BoardQuest
    {
        //목표량
        private int clearValue = 0;

        //달성량
        private int archiveValue = 0;

        public BoardQuest_DestroyBlockCount(BoardModel board, int clearValue)
        {
            this.clearValue = clearValue;

            //Board Block의 상태가 Destroy로 변할때 달성 수치 +1
            var blocks = board.Blocks;
            blocks.ForEach(x => x.StateObservable.Subscribe(state=> {
                if(state == BlockState.DESTROYED) {
                    RunQuest(1);
                }
            }));

        }

        /**
         *  @brief  달성 수치 증가
         *  @param  archive : 증가량
         */
        public void RunQuest(int archive)
        {
            archiveValue += archive;


            //목표량을 채우면 Complete를 true로 변경
            if(clearValue <= archiveValue) {
                isComplete.Value = true;
            }
        }
    }

}
