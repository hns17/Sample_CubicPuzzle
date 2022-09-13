using UnityEngine;
using Zenject;

namespace CubicSystem.CubicPuzzle
{
    public abstract class StageMonoInstaller :MonoInstaller
    {
        [SerializeField] protected GameObject pfStage;
        [SerializeField] protected GameObject pfBoard;
        [SerializeField] protected GameObject pfCell;
        [SerializeField] protected GameObject pfBlock;

        [SerializeField] protected CubicPuzzleStageData stageData;
        [SerializeField] protected StageInputManager stageInputManager;

        public override void InstallBindings()
        {
            //ModelFactory
            Container.BindFactory<StageModel, StageModel.Factory>()
                .AsSingle();

            Container.BindFactory<PuzzleBoardInfo, Transform, BoardModel, BoardModel.Factory>()
                .FromFactory<BoardModelFactory>();

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

            Container.Bind<CTSManager>().AsSingle();
            Container.Bind<DropDownAndFillEventFactory>().AsSingle();
        }
    }
}
