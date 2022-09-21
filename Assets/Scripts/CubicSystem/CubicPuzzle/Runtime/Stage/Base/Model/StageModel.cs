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
     *  @detail Stage Data 관리 및 Board 생성 및 정보 관리
     *          Board Clear 할 경우 NextBoard 진행
     */
    public class StageModel
    {
        public string Name { get; private set; }

        //현재 진행 중인 Level
        public int Level { get; private set; }

        //Unitask List
        private List<UniTask> taskAction = new List<UniTask>();

        //현재 플레이 중 인 보드 리스트
        private List<BoardModel> activeBoards = new List<BoardModel>();

        private HashSet<string> clearBoardGuids = new HashSet<string>();

        private Func<PuzzleBoardInfo, BoardModel> createBoard;

        [Inject] private CubicPuzzleStageData stageData;
        [Inject] private BoardModel.Factory boardFactory;
        [Inject] private CTSManager ctsManager;

        /**
         *  @brief  일회성 정보를 Injection 받은 후 객체 정보 구성
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
         *  @brief  지정된 단계에 해당되는 보드 정보 생성
         *  @param  level(int) : 생성 할 단계
         */
        private void MakePhase(int level)
        {
            //더 이상 진행할 단계가 없는 경우 Stage Clear
            if(stageData.phaseInfos.Count <= level) {
                ctsManager?.CancellationAll();
                SceneManager.LoadSceneAsync(0);
                return;
            }

            List<PuzzleBoardInfo> boardInfos = stageData.boards;
            List<int> phaseIndices = stageData.phaseInfos[Level].container;

            //activeBoards.Clear();

            //지정된 단계의 보드를 구성
            foreach(int boardIndex in phaseIndices) {
                MakeBoard(boardInfos[boardIndex]);
            }
        }

        /**
         *  @brief  보드 생성하기
         *  @param  boardInfo(PuzzleBoardInfo) : 생성 할 보드 정보
         */
        private BoardModel MakeBoard(PuzzleBoardInfo boardInfo)
        {

            BoardModel newBoard = null;
            //사용되지 않는 BoardModel Instance를 찾는다.
            foreach(BoardModel actBoard in activeBoards) {
                if(actBoard.State == BoardState.DESTROYED
                    && actBoard.BoardType == boardInfo.boardData.boardType) {
                    newBoard = actBoard;
                    break;
                }
            }

            //사용되지 않는 BoardModel이 있는 경우 초기화
            if(newBoard != null) {
                newBoard.Initialize(boardInfo);
            }
            //없는 경우 새로 생성
            else {
                newBoard = createBoard?.Invoke(boardInfo);

                //보드 클리어 이벤트 등록
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
         *  @brief   Board Clear 후 다음 Board 생성하기
         *  @param   clearBoard(BoardModel) : Clear한 Board
         */
        private async UniTask ClearBoardEvent(BoardModel clearBoard)
        {
            List<int> nextBoardIndices = clearBoard.BoardInfo.nextBoardIndices;

            //더 이상 진행할 Board가 없는 경우
            //현재 Phase의 모든 보드가 클리어되면 다음 Phase로 진행
            if(nextBoardIndices == null || nextBoardIndices.Count <= 0) {
                await NextPhase();
            }
            //Next Board 정보가 있는 경우 보드 교체
            else {
                Debug.Log("NextBoard");
                await NextBoard(clearBoard);
            }
        }

        /**
         *  @brief  다음 Phase 진행
         */
        private async UniTask NextPhase()
        {
            //모든 보드가 클리어 상태인지 확인
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
                //모든 보드를 파괴하고 다음 Phase의 Board를 생성
                foreach(BoardModel actBoard in activeBoards) {
                    taskAction.Add(actBoard.DestroyBoard(ctsManager.GetDefaultCancellationTokenSource().Token));
                }

                await UniTask.WhenAll(taskAction);

                MakePhase(++Level);
            }
        }

        /**
         *  @brief  다음 Board 구성
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
                //Next Board의 PrevBoard가 전부 클리어인지 확인
                bool isPrevBoardComplete = true;
                List<int> prevBoardIndices = boardInfos[nextBoardIndex].prevBoardIndices;
                foreach(int prevBoardIndex in prevBoardIndices) {
                    //prev Board 정보가 Clear 목록에 없는 경우...
                    if(!clearBoardGuids.Contains(boardInfos[prevBoardIndex].guid)) {
                        isPrevBoardComplete = false;
                        break;
                    }
                }

                //Clear해야할 Board가 남아있다
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