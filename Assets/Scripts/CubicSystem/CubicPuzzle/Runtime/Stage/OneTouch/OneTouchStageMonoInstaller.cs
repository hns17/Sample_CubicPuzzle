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

            Container.BindFactory<BoardModel, IMatchEvaluator, MatchEvaluatorFactory>()
                .To<OneTouchMatchEvaluator>()
                .AsSingle();
        }
    }
}