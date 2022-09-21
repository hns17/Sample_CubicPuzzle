using UnityEngine;
using Zenject;

namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Stage GameObject
     */
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