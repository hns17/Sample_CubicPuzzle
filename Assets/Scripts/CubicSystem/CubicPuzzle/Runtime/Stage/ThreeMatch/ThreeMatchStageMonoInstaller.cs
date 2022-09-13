using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace CubicSystem.CubicPuzzle
{
    public class ThreeMatchStageMonoInstaller :StageMonoInstaller
    {
        [SerializeField] private CubicPuzzlePatternData extraPattenData;

        public override void InstallBindings()
        {
            base.InstallBindings();

            Container.BindInstance<CubicPuzzlePatternData>(extraPattenData);
            Container.BindFactory<BoardModel, BoardActManager, BoardActManager.Factory>()
                .To<ThreeMatchBoardActManager>()
                .AsSingle();

            Container.Bind<ThreeMatchEvaluatorFactory>().AsSingle();
        }
    }
}