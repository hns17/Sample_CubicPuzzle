using UnityEngine;
using Zenject;

namespace CubicSystem.CubicPuzzle
{
    public class OneTouchStageMonoInstaller :StageMonoInstaller
    {
        public override void InstallBindings()
        {
            base.InstallBindings();

            Container.BindFactory<BoardModel, BoardActManager, BoardActManager.Factory>()
                .To<OneTouchBoardActManager>()
                .AsSingle();

            Container.Bind<IFactory<BoardModel, IMatchEvaluator>>()
                .To<OneTouchMatchEvaluatorFactory>()
                .AsSingle();
        }
    }
}