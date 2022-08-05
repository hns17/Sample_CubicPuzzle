using UnityEngine;
using Zenject;



namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Start Create Stage
     */
    public class StageBuilder :MonoBehaviour
    {

        [Inject]
        private void InjectDependices(StageModel.Factory factory)
        {
            var stage = factory.Create();
        }

    }
}