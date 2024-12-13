/**
 * License: MIT No Attribution
 * Copyright 2022 Gizmhail
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software
 * without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
 */

using Fusion.XR.Shared;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public class ThreadedTask<TOutput> 
{
    public delegate TOutput ThreadedBlock();
    public bool taskStarted = false;
    public int threadCheckDelay = 1;
    public float maxMsExecutionTime = -1;
    readonly ThreadedBlock threadedBlock;
    Thread thread;
    public TOutput output;
    Stopwatch timeKeeper;

    public ThreadedTask(ThreadedBlock block, float maxMsExecutionTime = -1, TOutput defaultValue = default)
    {
        this.threadedBlock = block;
        this.maxMsExecutionTime = maxMsExecutionTime;
        this.output = defaultValue;
    }

    public void Start()
    {
        if(maxMsExecutionTime != -1) timeKeeper = Stopwatch.StartNew();
        thread = new Thread(ThreadRun);
        thread.Start();
        taskStarted = true;
    }

    public async Task WaitCompletion() {
        while (thread.IsAlive && (maxMsExecutionTime == -1 || timeKeeper.ElapsedMilliseconds < maxMsExecutionTime))
        {
            await AsyncTask.Delay(threadCheckDelay);
        }
        taskStarted = false;
    }

    public async Task<TOutput> WaitOutput()
    {
        Start();
        await WaitCompletion();
        return output;
    }

    protected virtual void ThreadRun()
    {
        output = threadedBlock();
    }
}
