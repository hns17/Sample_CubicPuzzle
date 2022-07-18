using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

/**
 *  @brief  Zenject�� Injection ����� ����ϱ� ���� MonoBehaviour�� Generic ���丮
 *          IFactory <Transform : Parent, T : Component>
 *  @ref    https://qiita.com/snowhork/items/95dcf75dd4d4608e924e
 */
public class MonoFactory<T> :IFactory<T>
{
    protected DiContainer container;
    private UnityEngine.Object prefab;

    [Inject]
    public void Construct(
        UnityEngine.Object prefab,
        DiContainer container)
    {
        this.container = container;
        this.prefab = prefab;
    }

    /*
     *  @brief  Object ���� �� ������Ʈ ��ȯ
     */
    public virtual T Create()
    {
        GameObject newObj = container.InstantiatePrefab(prefab);
        return newObj.GetComponent<T>();
    }

    /*
     *  @brief  Object ���� �� �θ� ���� �� ������Ʈ ��ȯ
     *  @param  parent : Object Parent
     */
    public virtual T Create(Transform parent)
    {
        GameObject newObj = container.InstantiatePrefab(prefab);
        newObj.transform.SetParent(parent);

        return newObj.GetComponent<T>();
    }

    /*
     *  @brief  Object ����, �θ�, ��ġ ���� �� ������Ʈ ��ȯ
     *  @param  parent : Object Parent, position : Object Position
     */
    public virtual T Create(Transform parent, Vector3 position)
    {
        GameObject newObj = container.InstantiatePrefab(prefab);
        newObj.transform.SetParent(parent);
        newObj.transform.position = position;

        return newObj.GetComponent<T>();
    }
}