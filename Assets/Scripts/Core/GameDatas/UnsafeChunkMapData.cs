using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
 
using System.Runtime.InteropServices;
using UnityEngine;

public unsafe class UnsafeChunkMapData<T> :IDisposable where T : struct
{

    public void* header { get; private set; }

    public int elementCount { get; private set; }

    private int elementSize;

    public int elementCountDim0 { get; private set; }
    public int elementCountDim1 { get; private set; }
    public int elementCountDim2 { get; private set; }

    public UnsafeChunkMapData(int dim0Count, int dim1Count, int dim2Count,bool clearMem=true)
    {
        if (dim0Count < 0 || dim0Count >= Int32.MaxValue || dim1Count < 0 || dim1Count >= Int32.MaxValue ||
            dim2Count < 0 || dim2Count >= Int32.MaxValue)
        {
            throw new ArgumentOutOfRangeException();
        }
        elementCountDim0= dim0Count;
        elementCountDim1= dim1Count;
        elementCountDim2= dim2Count;
        this.elementCount= dim0Count*dim1Count*dim2Count;
        this.elementSize = Marshal.SizeOf<T>();
        long size= (long)UnsafeUtility.SizeOf<T>()*(long)elementCount;
        header = UnsafeUtility.MallocTracked(size, UnsafeUtility.AlignOf<T>(),Allocator.Persistent,0);
        if (clearMem == true)
        {
            UnsafeUtility.MemClear(header,size);
        }

    }
    public UnsafeChunkMapData(T[,,] sourceArray)
    {
        int dim0Count = sourceArray.GetLength(0);
        int dim1Count = sourceArray.GetLength(1);
        int dim2Count = sourceArray.GetLength(2);
        elementCountDim0 = dim0Count;
        elementCountDim1 = dim1Count;
        elementCountDim2 = dim2Count;
        this.elementCount = dim0Count * dim1Count * dim2Count;
        this.elementSize = UnsafeUtility.SizeOf<T>();
        long size = (long)UnsafeUtility.SizeOf<T>() * (long)elementCount;
        header = UnsafeUtility.MallocTracked(size, UnsafeUtility.AlignOf<T>(), Allocator.Persistent,0);
         
        UnsafeUtility.MemClear(header, size);

        for (int i = 0; i < dim0Count; i++)
        {
            for (int j = 0; j < dim1Count; j++)
            {
                for (int k = 0; k < dim2Count; k++)
                {
                    int indexFlattened = i * elementCountDim1 * elementCountDim2 + j * elementCountDim2 + k;
                    UnsafeUtility.WriteArrayElement<T>(header, indexFlattened, sourceArray[i,j,k]);
                }
            }
        }
    }

    public T this[int index0, int index1, int index2]
    {
        get
        {
            if (index0 < 0 || index0 >= elementCountDim0)
            {
                throw new ArgumentOutOfRangeException(index0.ToString(), "index0 out of range");
            }
            if (index1 < 0 || index1 >= elementCountDim1)
            {
                throw new ArgumentOutOfRangeException(index1.ToString(), "index1 out of range");
            }
            if (index2 < 0 || index2 >= elementCountDim2)
            {
                throw new ArgumentOutOfRangeException(index2.ToString(), "index2 out of range");
            }

            int indexFlattened = index0 * elementCountDim1 * elementCountDim2 + index1 * elementCountDim2 + index2;
            return UnsafeUtility.ReadArrayElement<T>(header,indexFlattened);
        }
        set
        {
            if (index0 < 0 || index0 >= elementCountDim0)
            {
                throw new ArgumentOutOfRangeException(index0.ToString(), "index0 out of range");
            }
            if (index1 < 0 || index1 >= elementCountDim1)
            {
                throw new ArgumentOutOfRangeException(index1.ToString(), "index1 out of range");
            }
            if (index2 < 0 || index2 >= elementCountDim2)
            {
                throw new ArgumentOutOfRangeException(index2.ToString(), "index2 out of range");
            }

            int indexFlattened = index0 * elementCountDim1 * elementCountDim2 + index1 * elementCountDim2 + index2;
            UnsafeUtility.WriteArrayElement<T>(header, indexFlattened,value);
        }
    }

    public T[,,] ToArray()
    {
        T[,,] retValue = new T[elementCountDim0, elementCountDim1, elementCountDim2];
        for (int i = 0; i < elementCountDim0; i++)
        {
            for (int j = 0; j < elementCountDim1; j++)
            {
                for (int k = 0; k < elementCountDim2; k++)
                {
                    retValue[i, j, k] = this[i, j, k];
                }
            }
        }

        return retValue;
    }
    
    protected bool disposed;

    private void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }



        if (disposing)
        {

        }
        //free unmanaged memory
         
        UnsafeUtility.FreeTracked(header,Allocator.Persistent);

        disposed = true;

    }

    public void Dispose()
    {
        Dispose(true);
    }

    ~UnsafeChunkMapData()
    {
        Dispose();
    }
}
