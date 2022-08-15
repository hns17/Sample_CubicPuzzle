using System;
using Unity.Burst;
using UnityEngine;

namespace CubicSystem
{
    [BurstCompile]
    public ref struct CubicSpanArray<T>
    {
        public Span<T> datas;
        
        public int Count { get; private set; }
        public int Capacity => datas.Length;
        

        public CubicSpanArray(Span<T> datas)
        {
            this.Count = 0;
            this.datas = datas;

        }

        public T this[int index]
        {
            get {
                return !IsOutOfRange(index) ? datas[index] : default;
            }
            set {
                if(!IsOutOfRange(index)) {
                    datas[index] = value;
                }
            }
        }

        public void Add(T data)
        {
            if(!IsOutOfRange(Count)) {
                datas[Count] = data;
                Count++;
            }
        }

        public void Insert(T data, int index)
        {
            if(index == 0 || Count == index) {
                Add(data);
            }
            else if(!IsOutOfRange(Count)){
                for(int i = Count; i > index; i--) {
                    datas[i] = datas[i - 1];
                }
                datas[index] = data;
                Count++;
            }
        }

        public bool IsOutOfRange(int index)
        {
            if(datas.Length <= index) {
                Debug.LogError("data out of range");
                return true;
            }

            return false;
        }

        public void Clear()
        {
            datas.Clear();
            Count = 0;
        }
    }
}
