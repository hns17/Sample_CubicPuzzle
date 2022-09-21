using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace CubicSystem.CubicPuzzle
{
    public class ThreeMatchStageMonoInstaller :StageMonoInstaller
    {

        [Header("[Pattern Data]", order = 1)]
        //Extra 패턴 정보
        [SerializeField] private CubicPuzzlePatternData extraPattenData;

        public override void InstallBindings()
        {
            base.InstallBindings();

            Container.BindInstance<CubicPuzzlePatternData>(extraPattenData)
                .AsSingle();
            Container.BindFactory<BoardModel, BoardActManager, BoardActManager.Factory>()
                .To<ThreeMatchBoardActManager>()
                .AsSingle();

            Container.BindFactory<BoardModel, IMatchEvaluator, MatchEvaluatorFactory>()
                .To<ThreeMatchEvaluator>()
                .AsSingle();
        }
    }
}