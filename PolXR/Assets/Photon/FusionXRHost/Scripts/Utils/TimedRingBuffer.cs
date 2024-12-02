using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Fusion.Tools
{
    public interface ICopiable<T>
    {
        // Ensure that all fields from source are used to fill the values of this object
        public void CopyValuesFrom(T source) { }
    }

    public enum InterpolationStatus
    {
        ValidFromTo,
        ValidTo,
        ValidFrom,
        InvalidBoundaries
    }

    public struct InterpolationInfo<T>
    {
        public TimedRingBufferEntry<T>? fromEntry;
        public TimedRingBufferEntry<T>? toEntry;
        public float alpha;
        public InterpolationStatus status;

        public T from
        {
            get
            {
                if (fromEntry == null) throw new System.Exception("'From' unavailable. Status: " + status);
                return fromEntry.GetValueOrDefault().data;
            }
        }

        public T to
        {
            get
            {
                if (toEntry == null) throw new System.Exception("'To' unavailable. Status: " + status);
                return toEntry.GetValueOrDefault().data;
            }
        }
    }

    public struct TimedRingBufferEntry<T>
    {
        public float time;
        public T data;
    }

    // Simple ring buffer storing time alongside data in Add(), to be able to interpolate at a certain point in the past with InterpolateInfo()
    public struct TimedRingbuffer<T> where T: ICopiable<T>
    {
        TimedRingBufferEntry<T>[] entries;
        // Contains the orderered index list
        int[] indexes;
        int nextIndex;
        int firstIndex;
        int bufferSize;

        public TimedRingbuffer(int size = 10)
        {
            entries = new TimedRingBufferEntry<T>[size];
            bufferSize = size;
            nextIndex = 0;
            firstIndex = -1;
            indexes = new int[0];
        }

        public InterpolationInfo<T> InterpolateInfo(float atTime)
        {
            InterpolationInfo<T> interpolationInfo = default;
            TimedRingBufferEntry<T>? fromEntry = default;
            TimedRingBufferEntry<T>? toEntry = default;
            for(int i = 0; i < indexes.Length; i++)
            {
                if (entries[i].time < atTime)
                {
                    if (fromEntry == null || fromEntry.GetValueOrDefault().time < entries[i].time)
                    {
                        fromEntry = entries[i];
                    }
                }
                if (atTime < entries[i].time)
                {
                    if (toEntry == null || entries[i].time < toEntry.GetValueOrDefault().time )
                    {
                        // If the entries where properly entered (with increasing time), the first valid to should be the last check, and we could break here
                        toEntry = entries[i];
                    }
                }
            }
            if (fromEntry != null)
            {
                if(toEntry != null)
                {
                    interpolationInfo.status = InterpolationStatus.ValidFromTo;
                    float fromTime = fromEntry.GetValueOrDefault().time;
                    float toTime = toEntry.GetValueOrDefault().time;
                    if (fromTime == toTime)
                    {
                        Debug.LogError("Two entries at same time");
                        interpolationInfo.alpha = 0;
                    } 
                    else
                    {
                        interpolationInfo.alpha = (atTime - fromTime)/(toTime - fromTime);
                        if (interpolationInfo.alpha < 0 || interpolationInfo.alpha > 1)
                        {
                            Debug.LogError("Problem in stored times");
                        }
                    }
                } 
                else
                {
                    interpolationInfo.status = InterpolationStatus.ValidFrom;
                    interpolationInfo.alpha = 0;
                }
            }
            else
            {
                if (toEntry != null)
                {
                    interpolationInfo.status = InterpolationStatus.ValidTo;
                    interpolationInfo.alpha = 1;
                }
                else
                {
                    interpolationInfo.status = InterpolationStatus.InvalidBoundaries;
                }
            }
            interpolationInfo.fromEntry = fromEntry;
            interpolationInfo.toEntry = toEntry;

            return interpolationInfo;
        }

        public void Add(T data, float time)
        {
            entries[nextIndex].data.CopyValuesFrom(data);
            entries[nextIndex].time = time;
            UpdateIndexes();
        }

        void UpdateIndexes() { 
            // 2 initial cases: buffer never filled once (firstIndex == 0), and buffer filled once (firstIndex == nextIndex)
            bool bufferFilledOnce = firstIndex == nextIndex;
            if ((nextIndex + 1) >= bufferSize)
            {
                // Data will go other the max index
                bufferFilledOnce = true;
            }
            if (firstIndex == -1)
            {
                firstIndex = 0;
            }
            nextIndex = ((nextIndex + 1) % bufferSize);
            if (bufferFilledOnce)
            {
                // The buffer has already been filled once
                firstIndex = nextIndex;
            }
            int validIndexCount = (nextIndex > firstIndex)?(nextIndex - firstIndex):(nextIndex + bufferSize - firstIndex);
            if(indexes.Length != validIndexCount)
            {
                indexes = new int[validIndexCount];
            }
            int index = firstIndex;
            for(int i = 0; i < validIndexCount; i++)
            {
                indexes[i] = (firstIndex + i) % bufferSize;
            }
        }
    }
}