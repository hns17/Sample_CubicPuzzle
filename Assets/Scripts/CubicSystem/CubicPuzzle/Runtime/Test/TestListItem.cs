using Scrmizu;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TestListItem :StageListItem, IInfiniteScrollItem
{
    [SerializeField] private Text txtItem;

    public override void ClickEvent(Action actEvent)
    {
        btnItem.onClick.RemoveAllListeners();
        btnItem.onClick.AddListener(() =>
        {
            actEvent?.Invoke();
        });

    }

    /**
     *  @brief  풀링시 아이템 정보 비활성화
     */
    public void Hide()
    {
        gameObject.SetActive(false);
    }


    /**
     *  @brief  Item 정보 활성화 및 정보 갱신
     *  @param  object(data) : Item에 사용되는 Data
     */
    public void UpdateItemData(object data)
    {
        var stageListData = data as StageListData;
        txtItem.text = stageListData?.StageName;

        //Click Event 구성
        ClickEvent(() =>
        {
            //Path가 Exit면 Application 종료
            if(stageListData.StagePath.Equals("Exit")) {
                Application.Quit();
            }
            //씬 로드
            else {
                SceneManager.LoadSceneAsync(stageListData?.StageName);
            }
        });

        gameObject.SetActive(true);
    }
}
