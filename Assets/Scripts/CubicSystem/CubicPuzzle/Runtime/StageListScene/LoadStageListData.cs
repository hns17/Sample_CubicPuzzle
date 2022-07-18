using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/**
 *  @brief  StageList 정보를 Load
 *          StageListFile은 Json Format 사용 중
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

        //Stage List Data는 Json File을 사용 중...
        stageLists = JsonConvert.DeserializeObject<List<StageListData>>(textData.ToString());
    }

    /**
     *  @brief  Load한 Stage List Data 반환
     *  @param  List<StageListData> : Stage List Data
     */
    public List<StageListData> GetData()
    {
        return stageLists;
    }
}
