using System;
using UnityEngine;
using UnityEngine.UI;


/**
 *  @brief  Stage ��Ͽ� ǥ�õ� �������� abstract class
 *          Item�� �ش� class���� Ȯ��Ǵ� Button�̸� ClickEvent�� ������.
 */
public abstract class StageListItem : MonoBehaviour
{
    [SerializeField] protected Button btnItem;

    public abstract void ClickEvent(Action actEvent);

}
