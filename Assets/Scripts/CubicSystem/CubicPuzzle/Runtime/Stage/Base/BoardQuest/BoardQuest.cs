using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;


namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  Abstract Board Quest Class
     */
    public abstract class BoardQuest
    {
        //Quest �Ϸ�� Reactive ����(true : �Ϸ�, false : �̿Ϸ�)
        //Value Getter, IObeservable ����
        protected ReactiveProperty<bool> isComplete = new ReactiveProperty<bool>(false);
        public bool IsComplete => isComplete.Value;
        public IObservable<bool> IsCompleteObservable => isComplete;
    }

}
