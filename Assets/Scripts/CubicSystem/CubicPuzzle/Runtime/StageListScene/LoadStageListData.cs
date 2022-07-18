using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/**
 *  @brief  StageList ������ Load
 *          StageListFile�� Json Format ��� ��
 */
public class LoadStageListData :ILoadData<List<StageListData>>
{
    private List<StageListData> stageLists;

    public LoadStageListData()
    {
        Debug.Log("LoadStageListData");

        //Load StageList Data
        TextAsset textData = Resources.Load("ExternalFile/TestStageList") as TextAsset;
        Debug.Assert(textData, "not found stage list data");

        //Stage List Data�� Json File�� ��� ��...
        stageLists = JsonConvert.DeserializeObject<List<StageListData>>(textData.ToString());
    }

    /**
     *  @brief  Load�� Stage List Data ��ȯ
     *  @param  List<StageListData> : Stage List Data
     */
    public List<StageListData> GetData()
    {
        return stageLists;
    }
}
