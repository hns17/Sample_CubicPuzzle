
using System.Threading;
using UnityEngine;
using Zenject;

namespace CubicSystem.CubicPuzzle
{
    public class ThreeMatchStageMonoInstaller :MonoInstaller
    {
        [SerializeField] private GameObject pfStage;
        [SerializeField] private GameObject pfBoard;
        [SerializeField] private GameObject pfCell;
        [SerializeField] private GameObject pfBlock;

        [SerializeField] private PuzzleStageData stageData; 
        [SerializeField] private StageInputManager stageInputManager;

        public override void InstallBindings()
        {
            //ModelFactory
            Container.BindFactory<StageModel, StageModel.Factory>()
                .AsSingle();

            Container.BindFactory<PuzzleBoardInfo, Transform,BoardModel, BoardModel.Factory>()
                .To<HexBoardModel>()
                .AsSingle();

            Container.BindFactory<BoardItemData, Transform, Vector2, CellModel, CellModel.Factory>()
                .AsSingle();

            Container.BindFactory<BoardItemData, Transform, Vector2, BlockModel, BlockModel.Factory>()
                .AsSingle();


            //Presenter Factory
            Container.BindFactory<StageModel, StagePresenter, StagePresenter.Factory>()
                .FromComponentInNewPrefab(pfStage)
                .AsSingle();

            Container.BindFactory<BoardModel, Transform, BoardPresenter, BoardPresenter.Factory>()
                .FromComponentInNewPrefab(pfBoard)
                .AsSingle();

            Container.BindFactory<CellModel, Transform, CellPresenter, CellPresenter.Factory>()
                .FromComponentInNewPrefab(pfCell)
                .AsSingle();

            Container.BindFactory<BlockModel, Transform, BlockPresenter, BlockPresenter.Factory>()
                .FromComponentInNewPrefab(pfBlock)
                .AsSingle();


            //Board QuestFactory
            Container.Bind<BoardQuestFactory>().AsSingle();

            //Input Manager
            Container.BindInstance(stageInputManager);

            //Load StageData
            Container.BindInstance<PuzzleStageData>(stageData);


            //Container.BindFactory<BoardModel, BoardActManager, BoardActManager.Factory>()
            //    .To<ThreeMatchBoardActManager>()
            //    .AsSingle();

            Container.BindFactory<BoardModel, BoardActManager, BoardActManager.Factory>()
                .FromFactory<ThreeMatchBoardActManager.Factory>();

            Container.Bind<CTSManager>().AsSingle();
        }
    }
}