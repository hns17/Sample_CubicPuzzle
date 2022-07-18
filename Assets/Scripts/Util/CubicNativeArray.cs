using System.Collections;
using Unity.Burst;
using Unity.Collections;

namespace CubicSystem
{
    [BurstCompile]
    public struct CubicNativeArray<T> where T : struct
    {
        private const int defaultCapacity = 50;
        private NativeArray<T> datas;
        private Allocator allocator;
        public int Count { get; private set; }

        public CubicNativeArray(CubicNativeArray<T> cubicNative)
        {
            this.Count = cubicNative.Count;
            this.allocator = cubicNative.allocator;
            this.datas = new NativeArray<T>(cubicNative.datas.Length, this.allocator);
            this.datas.CopyFrom(cubicNative.datas);
        }

        public CubicNativeArray(int capacity, Allocator allocator)
        {
            this.Count = 0;
            this.allocator = allocator;
            this.datas = new NativeArray<T>(capacity, allocator);
        }

        public T this[int index]
        {
            get { return datas[index]; }
            set { datas[index] = value; }
        }


        public void Add(T data)
        {
            datas[Count] = data;
            Count++;
        }

        public void Insert(T data, int index)
        {
            if(index == 0 || Count == index) {
                Add(data);
            }
            else {
                for(int i = Count; i > index; i--) {
                    datas[i] = datas[i - 1];
                }
                datas[index] = data;
                Count++;
            }
        }


        public IEnumerator GetEnumerator()
        {
            for(int i = 0; i < Count; i++) {
                yield return (datas[i]);
            }
        }


        public void Dispose()
        {
            Count = -1;
            datas.Dispose();
        }

    }

}
