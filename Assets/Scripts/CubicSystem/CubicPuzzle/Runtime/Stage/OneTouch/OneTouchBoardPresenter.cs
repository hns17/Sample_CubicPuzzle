using Cysharp.Threading.Tasks;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  OneTouch Board GameObject
     */
    public class OneTouchBoardPresenter :BoardPresenter
    {
        //Input Event
        private IOneTouchBlockEvent oneTouchEvent;

        public override void InitalizeStart()
        {
            base.InitalizeStart();
            oneTouchEvent = board.ActManager as OneTouchBoardActManager;
        }

        protected override void TouchStart()
        {
            if(board.State == BoardState.READY) {
                base.TouchStart();
                if(fromBlock != null) {
                    //Call TouchEvent
                    oneTouchEvent.DoTouchAction(fromBlock.Block).Forget();
                }
            }
        }
    }
}
