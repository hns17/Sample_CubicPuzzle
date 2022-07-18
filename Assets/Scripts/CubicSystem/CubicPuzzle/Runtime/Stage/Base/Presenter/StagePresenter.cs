using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace CubicSystem.CubicPuzzle
{
    public class StagePresenter :MonoBehaviour
    {
        private StageModel stageModel;

        [Inject]
        private void InjectDependencies(StageModel stageModel)
        {
            this.stageModel = stageModel;
        }

        public class Factory :PlaceholderFactory<StageModel, StagePresenter>
        {

        }
    }
}