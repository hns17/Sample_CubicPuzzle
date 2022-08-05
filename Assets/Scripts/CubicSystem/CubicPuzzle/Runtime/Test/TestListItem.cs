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
     *  @brief  Ǯ���� ������ ���� ��Ȱ��ȭ
     */
    public void Hide()
    {
        gameObject.SetActive(false);
    }


    /**
     *  @brief  Item ���� Ȱ��ȭ �� ���� ����
     *  @param  object(data) : Item�� ���Ǵ� Data
     */
    public void UpdateItemData(object data)
    {
        var stageListData = data as StageListData;
        txtItem.text = stageListData?.StageName;

        //Click Event ����
        ClickEvent(() =>
        {
            //Path�� Exit�� Application ����
            if(stageListData.StagePath.Equals("Exit")) {
                Application.Quit();
            }
            //�� �ε�
            else {
                SceneManager.LoadSceneAsync(stageListData?.StageName);
            }
        });

        gameObject.SetActive(true);
    }
}
