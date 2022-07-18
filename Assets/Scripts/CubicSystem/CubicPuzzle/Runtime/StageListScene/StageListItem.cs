using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;


/**
 *  @brief  Stage 목록에 표시될 아이템의 abstract class
 *          Item은 해당 class에서 확장되는 Button이며 ClickEvent를 가진다.
 */
public abstract class StageListItem : MonoBehaviour
{
    [SerializeField] protected Button btnItem;

    public abstract void ClickEvent(Action actEvent);

}
