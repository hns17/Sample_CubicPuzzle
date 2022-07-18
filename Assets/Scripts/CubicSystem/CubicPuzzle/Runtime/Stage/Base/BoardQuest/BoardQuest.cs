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
        //Quest 완료용 Reactive 변수(true : 완료, false : 미완료)
        //Value Getter, IObeservable 공개
        protected ReactiveProperty<bool> isComplete = new ReactiveProperty<bool>(false);
        public bool IsComplete => isComplete.Value;
        public IObservable<bool> IsCompleteObservable => isComplete;
    }

}
