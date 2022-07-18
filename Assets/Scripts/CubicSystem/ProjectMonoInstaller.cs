using CubicSystem.CubicPuzzle;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using Zenject;


/**
 *  @brief  Project Installer, 프로젝트 시작시 로드할 데이터 및 객체
 */
public class ProjectMonoInstaller : MonoInstaller
{

    public override void InstallBindings()
    {
        DOTween.SetTweensCapacity(tweenersCapacity: 500, sequencesCapacity: 200);

        //Stage List 정보 Load
        Container.BindInstance(LoadStageListData()).AsSingle();
    }


    /**
     *  @brief  Stage 목록 정보 불러오기
     */
    private List<StageListData> LoadStageListData()
    {
        LoadStageListData loadData = new LoadStageListData();
        return loadData.GetData();
    }
}