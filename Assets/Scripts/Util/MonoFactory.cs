using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

/**
 *  @brief  Zenject의 Injection 기능을 사용하기 위한 MonoBehaviour용 Generic 팩토리
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
     *  @brief  Object 생성 후 컴포넌트 반환
     */
    public virtual T Create()
    {
        GameObject newObj = container.InstantiatePrefab(prefab);
        return newObj.GetComponent<T>();
    }

    /*
     *  @brief  Object 생성 및 부모 지정 후 컴포넌트 반환
     *  @param  parent : Object Parent
     */
    public virtual T Create(Transform parent)
    {
        GameObject newObj = container.InstantiatePrefab(prefab);
        newObj.transform.SetParent(parent);

        return newObj.GetComponent<T>();
    }

    /*
     *  @brief  Object 생성, 부모, 위치 설정 후 컴포넌트 반환
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