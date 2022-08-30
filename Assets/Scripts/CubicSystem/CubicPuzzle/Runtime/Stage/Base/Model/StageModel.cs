using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UniRx;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Stage Data Model
     *  @detail Stage Data ���� �� Board ���� �� ���� ����
     *          Board Clear �� ��� NextBoard ����
     */
    public class StageModel
    {
        public string Name { get; private set; }

        //���� ���� ���� Level
        public int Level { get; private set; }


        //Board Object Parent
        private Transform parent;

        //Unitask List
        private List<UniTask> taskAction = new List<UniTask>();

        //���� �÷��� �� �� ���� ����Ʈ
        private List<BoardModel> activeBoards = new List<BoardModel>();

        private HashSet<string> clearBoardGuids = new HashSet<string>();

        [Inject] private CubicPuzzleStageData stageData;
        [Inject] private BoardModel.Factory boardFactory;
        [Inject] private CTSManager ctsManager;

        /**
         *  @brief  ��ȸ�� ������ Injection ���� �� ��ü ���� ����
         *  @param  presenterFactory : Stage GameObject Factory
         */
        [Inject]
        private void InjectDependencies(StagePresenter.Factory presenterFactory)
        {
            //Make Stage GameObject
            parent = presenterFactory.Create(this).transform;

            this.Level = 0;
            MakePhase(Level);
        }

        /**
         *  @brief  ������ �ܰ迡 �ش�Ǵ� ���� ���� ����
         *  @param  level(int) : ���� �� �ܰ�
         */
        private void MakePhase(int level)
        {
            //�� �̻� ������ �ܰ谡 ���� ��� Stage Clear
            if(stageData.phaseInfos.Count <= level) {
                ctsManager?.CancellationAll();
                SceneManager.LoadSceneAsync(0);
                return;
            }

            var boardInfos = stageData.boards;
            var phaseIndices = stageData.phaseInfos[Level].container;

            //activeBoards.Clear();

            //������ �ܰ��� ���带 ����
            foreach(var boardIndex in phaseIndices) {
                MakeBoard(boardInfos[boardIndex]);
            }
        }

        /**
         *  @brief  ���� �����ϱ�
         *  @param  boardInfo(PuzzleBoardInfo) : ���� �� ���� ����
         */
        private BoardModel MakeBoard(PuzzleBoardInfo boardInfo)
        {

            BoardModel newBoard = null;
            //������ �ʴ� BoardModel Instance�� ã�´�.
            foreach(var actBoard in activeBoards) {
                if(actBoard.State == BoardState.DESTROYED) {
                    newBoard = actBoard;
                    break;
                }
            }

            //������ �ʴ� BoardModel�� �ִ� ��� �ʱ�ȭ
            if(newBoard != null) {
                newBoard.Initialize(boardInfo);
            }
            //���� ��� ���� ����
            else {
                newBoard = boardFactory.Create(boardInfo, parent);

                //���� Ŭ���� �̺�Ʈ ���
                newBoard.StateObservable.Subscribe(state =>
                {
                    if(state == BoardState.CLEAR) {
                        ClearBoardEvent(newBoard).Forget();
                    }
                    else if(state == BoardState.GAME_OVER) {
                        newBoard.SetBoardState(BoardState.CLEAR);
                    }
                });

                activeBoards.Add(newBoard);
            }
            
            return newBoard;
        }


        /**
         *  @brief   Board Clear �� ���� Board �����ϱ�
         *  @param   clearBoard(BoardModel) : Clear�� Board
         */
        private async UniTask ClearBoardEvent(BoardModel clearBoard)
        {
            var nextBoardIndices = clearBoard.BoardInfo.nextBoardIndices;

            //�� �̻� ������ Board�� ���� ���
            //���� Phase�� ��� ���尡 Ŭ����Ǹ� ���� Phase�� ����
            if(nextBoardIndices == null || nextBoardIndices.Count <= 0) {
                await NextPhase();
            }
            //Next Board ������ �ִ� ��� ���� ��ü
            else {
                Debug.Log("NextBoard");
                await NextBoard(clearBoard);
            }
        }

        /**
         *  @brief  ���� Phase ����
         */
        private async UniTask NextPhase()
        {
            //��� ���尡 Ŭ���� �������� Ȯ��
            bool isNextPhase = true;
            foreach(var actBoard in activeBoards) {
                if(actBoard.State != BoardState.CLEAR 
                    && actBoard.State != BoardState.DESTROYED) {
                    isNextPhase = false;
                    break;
                }
            }

            //Next Phase Event
            if(isNextPhase) {
                Debug.Log("NextPhase");

                taskAction.Clear();
                //��� ���带 �ı��ϰ� ���� Phase�� Board�� ����
                foreach(var actBoard in activeBoards) {
                    taskAction.Add(actBoard.DestroyBoard(ctsManager.GetDefaultCancellationTokenSource().Token));
                }

                await UniTask.WhenAll(taskAction);

                MakePhase(++Level);
            }
        }

        /**
         *  @brief  ���� Board ����
         */
        private async UniTask NextBoard(BoardModel clearBoard)
        {
            var nextBoardIndices = clearBoard.BoardInfo.nextBoardIndices;

            //Destroy Clear Board
            await clearBoard.DestroyBoard(ctsManager.GetDefaultCancellationTokenSource().Token);

            //var clearBoardIndex = activeBoards.IndexOf(clearBoard);
            //activeBoards.RemoveAt(clearBoardIndex);
            clearBoardGuids.Add(clearBoard.BoardInfo.guid);
            
            var boardInfos = stageData.boards;
            foreach(var nextBoardIndex in nextBoardIndices) {
                //Next Board�� PrevBoard�� ���� Ŭ�������� Ȯ��
                bool isPrevBoardComplete = true;
                var prevBoardIndices = boardInfos[nextBoardIndex].prevBoardIndices;
                foreach(var prevBoardIndex in prevBoardIndices) {
                    //prev Board ������ Clear ��Ͽ� ���� ���...
                    if(!clearBoardGuids.Contains(boardInfos[prevBoardIndex].guid)) {
                        isPrevBoardComplete = false;
                        break;
                    }
                }

                //Clear�ؾ��� Board�� �����ִ�
                if(!isPrevBoardComplete) {
                    continue;
                }

                //Make Next Board
                MakeBoard(boardInfos[nextBoardIndex]);
            }
        }

        public class Factory :PlaceholderFactory<StageModel>
        {

        }
    }
}