using UnityEngine;
using Zenject;

namespace CubicSystem.CubicPuzzle
{
    public class OneTouchStageMonoInstaller :MonoInstaller
    {
        [SerializeField] private GameObject pfStage;
        [SerializeField] private GameObject pfBoard;
        [SerializeField] private GameObject pfCell;
        [SerializeField] private GameObject pfBlock;

        [SerializeField] private CubicPuzzleStageData stageData;
        [SerializeField] private StageInputManager stageInputManager;

        public override void InstallBindings()
        {
            //ModelFactory
            Container.BindFactory<StageModel, StageModel.Factory>()
                .AsSingle();

            Container.BindFactory<PuzzleBoardInfo, Transform, BoardModel, BoardModel.Factory>()
                .To<HexBoardModel>();

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
            Container.BindInstance<CubicPuzzleStageData>(stageData);


            //Container.BindFactory<BoardModel, BoardActManager, BoardActManager.Factory>()
            //    .To<OneTouchBoardActManager>()
            //    .AsSingle();

            Container.BindFactory<BoardModel, BoardActManager, BoardActManager.Factory>()
                .FromFactory<OneTouchBoardActManager.Factory>();

            Container.Bind<CTSManager>().AsSingle();
        }
    }
}