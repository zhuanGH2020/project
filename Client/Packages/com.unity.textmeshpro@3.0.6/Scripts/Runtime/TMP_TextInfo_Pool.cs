using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace TMPro
{
    public class TMP_TextInfo_Pool
    {
        //public int Count {
        //    get {
        //        return pools.Count;
        //    }
        //}
        private Stack<TMP_TextInfo>[] pools;
        private List<int> thresholdInfos;
        private List<int> stepMaxInfos;
        private int characterSizeThreshold = 512;
        public TMP_TextInfo_Pool(int levelNum, int[] arraylevelThreshold, int[] arrayInitialCapacity, int[] arrayMaxCapacity, int characterSizeThreshold)
        {
            if (arrayInitialCapacity == null || arrayInitialCapacity.Length != levelNum || arraylevelThreshold == null || arraylevelThreshold.Length != levelNum || arrayMaxCapacity == null || arrayMaxCapacity.Length != levelNum)
            {
                //Error
                return;
            }
            pools = new Stack<TMP_TextInfo>[levelNum];
            thresholdInfos = new List<int>(levelNum);
            stepMaxInfos = new List<int>(levelNum);
            for (int i = 0; i < levelNum; i++)
            {
                int initialCapacity = arrayInitialCapacity[i];
                int levelThreshold = arraylevelThreshold[i];
                thresholdInfos.Add(levelThreshold);
                stepMaxInfos.Add(arrayMaxCapacity[i]);
                Stack<TMP_TextInfo> stack = new Stack<TMP_TextInfo>(initialCapacity);
                pools[i] = stack;

                for (int j = 0; j < initialCapacity; j++)
                {
                    stack.Push(new TMP_TextInfo(levelThreshold));
                }
            }
            this.characterSizeThreshold = characterSizeThreshold;
        }


        private Stack<TMP_TextInfo> GetPoolByCharacterCount(int characterCount)
        {
            //超过最大的，就返回最后一个
            int index = thresholdInfos.Count - 1;
            for (int i = 0; i < thresholdInfos.Count; i++)
            {
                if (characterCount <= thresholdInfos[i])
                {
                    index = i;
                    break;
                }
            }
            return pools[index];
        }

        private int GetPoolIndexCharacterCount(int characterCount)
        {
            //超过最大的，就返回最后一个
            int index = thresholdInfos.Count - 1;
            for (int i = 0; i < thresholdInfos.Count; i++)
            {
                if (characterCount <= thresholdInfos[i])
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        //返回的值不能小于characterCount
        private int GetThresholdByCount(int characterCount)
        {
            //超过最大的，就返回最后一个
            int threshold = thresholdInfos[thresholdInfos.Count - 1];
            for (int i = 0; i < thresholdInfos.Count; i++)
            {
                if (characterCount <= thresholdInfos[i])
                {
                    threshold = thresholdInfos[i];
                    break;
                }
            }
            return Math.Max(threshold, characterCount);
        }

        //返回对象的characterinfo.length必须大于等于characterCount
        public TMP_TextInfo Get(int characterCount)
        {
            //超过内存池的大小阈值后，直接不适用内存池功能
            if (characterCount > characterSizeThreshold)
            {
                return new TMP_TextInfo(characterCount);
            }
            Stack<TMP_TextInfo> pool = GetPoolByCharacterCount(characterCount);
            if (pool.Count > 0)
            {
                TMP_TextInfo ret = pool.Pop();
                if (ret.characterInfo.Length < characterCount)
                {
                    //最大的池出来的characterInfo长度也不够用时，才会触发
                    TMP_TextInfo.Resize(ref ret.characterInfo, characterCount, false);
                }
                return ret;
            }
            else
            {
                //todo!
                pool.Push(new TMP_TextInfo(GetThresholdByCount(characterCount)));
                return pool.Pop();
            }
        }

        public void GetPoolInfo(ref List<int> thresholdInfo, ref List<int> poolCount)
        {
            thresholdInfo.Clear();
            poolCount.Clear();
            for (int i = 0; i < this.thresholdInfos.Count; i++)
            {
                thresholdInfo.Add(this.thresholdInfos[i]);
                poolCount.Add(pools[i].Count);
            }
            return;
        }

        public void Release(TMP_TextInfo obj, int characterCount)
        {
            //超过回收阈值后，直接丢弃
            if (characterCount > characterSizeThreshold)
            {
                return;
            }
            int poolIndex = GetPoolIndexCharacterCount(characterCount);
            var pool = pools[poolIndex];
            var maxCount = stepMaxInfos[poolIndex];
            if (pool.Count >= maxCount)
            {
                return;
            }
            pool.Push(obj);
        }

        public void Clear()
        {
            for (int i = 0; i < pools.Length; i++)
            {
                pools[i].Clear();
            }
            pools = null;
        }
        /*
        public void Resize(int newSize) {
            if (newSize > pools.Count) {
                // 扩大池的容量
                int itemsToAdd = newSize - pools.Count;
                for (int i = 0; i < itemsToAdd; i++) {
                    pools.Push(new T());
                }
            } else if (newSize < pools.Count) {
                // 缩小池的容量
                int itemsToRemove = pools.Count - newSize;
                for (int i = 0; i < itemsToRemove; i++) {
                    pools.Pop();
                }
            }
        }
        */
    }
}