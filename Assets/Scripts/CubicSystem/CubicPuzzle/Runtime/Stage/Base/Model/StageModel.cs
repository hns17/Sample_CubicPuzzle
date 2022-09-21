using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UniRx;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using System;

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

        //Unitask List
        private List<UniTask> taskAction = new List<UniTask>();

        //���� �÷��� �� �� ���� ����Ʈ
        private List<BoardModel> activeBoards = new List<BoardModel>();

        private HashSet<string> clearBoardGuids = new HashSet<string>();

        private Func<PuzzleBoardInfo, BoardModel> createBoard;

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
            Transform parent = presenterFactory.Create(this).transform;

            createBoard = (boardInfo) =>
            {
                return boardFactory.Create(boardInfo, parent);
            };

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

            List<PuzzleBoardInfo> boardInfos = stageData.boards;
            List<int> phaseIndices = stageData.phaseInfos[Level].container;

            //activeBoards.Clear();

            //������ �ܰ��� ���带 ����
            foreach(int boardIndex in phaseIndices) {
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
            foreach(BoardModel actBoard in activeBoards) {
                if(actBoard.State == BoardState.DESTROYED
                    && actBoard.BoardType == boardInfo.boardData.boardType) {
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
                newBoard = createBoard?.Invoke(boardInfo);

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
            List<int> nextBoardIndices = clearBoard.BoardInfo.nextBoardIndices;

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
            foreach(BoardModel actBoard in activeBoards) {
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
                foreach(BoardModel actBoard in activeBoards) {
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
            List<int> nextBoardIndices = clearBoard.BoardInfo.nextBoardIndices;

            //Destroy Clear Board
            await clearBoard.DestroyBoard(ctsManager.GetDefaultCancellationTokenSource().Token);

            //var clearBoardIndex = activeBoards.IndexOf(clearBoard);
            //activeBoards.RemoveAt(clearBoardIndex);
            clearBoardGuids.Add(clearBoard.BoardInfo.guid);
            
            List<PuzzleBoardInfo> boardInfos = stageData.boards;
            foreach(int nextBoardIndex in nextBoardIndices) {
                //Next Board�� PrevBoard�� ���� Ŭ�������� Ȯ��
                bool isPrevBoardComplete = true;
                List<int> prevBoardIndices = boardInfos[nextBoardIndex].prevBoardIndices;
                foreach(int prevBoardIndex in prevBoardIndices) {
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