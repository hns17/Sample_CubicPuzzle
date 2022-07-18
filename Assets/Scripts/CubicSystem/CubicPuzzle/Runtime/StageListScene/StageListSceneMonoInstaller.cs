using Scrmizu;
using UnityEngine;
using Zenject;

public class StageListSceneMonoInstaller : MonoInstaller
{
    [SerializeField] private InfiniteScrollRect rtInfiniteScroll;
    public override void InstallBindings()
    {
        Container.BindInstances(rtInfiniteScroll);
    }

}