using Scrmizu;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class StageListPresenter : MonoBehaviour
{
    [Inject] private InfiniteScrollRect rtInfiniteScroll;

    [Inject]
    public void InjectDependencies(List<StageListData> stageListDatas)
    {
        Debug.Assert(stageListDatas != null, "StageListData is null");
        rtInfiniteScroll.SetItemData(stageListDatas.AsEnumerable());
    }
}