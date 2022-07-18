using CubicSystem.CubicPuzzle;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using Zenject;


/**
 *  @brief  Project Installer, ������Ʈ ���۽� �ε��� ������ �� ��ü
 */
public class ProjectMonoInstaller : MonoInstaller
{

    public override void InstallBindings()
    {
        DOTween.SetTweensCapacity(tweenersCapacity: 500, sequencesCapacity: 200);

        //Stage List ���� Load
        Container.BindInstance(LoadStageListData()).AsSingle();
    }


    /**
     *  @brief  Stage ��� ���� �ҷ�����
     */
    private List<StageListData> LoadStageListData()
    {
        LoadStageListData loadData = new LoadStageListData();
        return loadData.GetData();
    }
}