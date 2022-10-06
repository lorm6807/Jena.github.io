using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LottoProgram.Handlers
{
    public abstract class TaskHandler<T> : IDisposable
    {
        private CancellationTokenSource CancellationToken;
        private ConcurrentQueue<T> DataQueue;
        private Task Task;
        private ManualResetEventSlim Trigger;
        private TaskCreationOptions TaskCreationOptions;
        private bool isCreateCancelToken;

        private bool IsRunning = false;

        public virtual bool Add(T data, bool isOnTrigger = true)
        {
            if (DataQueue == null || Trigger == null)
                return false;

            DataQueue.Enqueue(data);

            if (isOnTrigger)
                Trigger.Set();

            return true;
        }

        public virtual bool AddRange(IEnumerable<T> dataList, bool isOnTrigger = true)
        {
            if (DataQueue == null || Trigger == null)
                return false;

            foreach (var data in dataList)
                DataQueue.Enqueue(data);

            if (isOnTrigger)
                Trigger.Set();

            return true;
        }

        public bool IsRunningTask()
        {
            return (DataQueue != null && !DataQueue.IsEmpty && !CancellationToken.IsCancellationRequested) || IsRunning;
        }

        public Task WaitTaskProcAsync(TaskCreationOptions taskCreationOptions = TaskCreationOptions.LongRunning)
        {
            return Task.Factory.StartNew(WaitTaskProc, taskCreationOptions);
        }

        private void WaitTaskProc()
        {
            while (IsRunningTask())
            {
                if (Task == null || Task.Status != TaskStatus.Running)
                    break;

                TaskTriggerOn();
                Thread.Sleep(10);
            }
        }

        private void TaskTriggerOn()
        {
            Trigger.Set();
        }

        protected void CreateTask(TaskCreationOptions taskCreationOptions = TaskCreationOptions.LongRunning, CancellationTokenSource cancellationToken = null)
        {
            if (DataQueue == null)
                DataQueue = new ConcurrentQueue<T>();

            if (Trigger == null)
                Trigger = new ManualResetEventSlim(false);

            if (Task == null)
            {
                isCreateCancelToken = cancellationToken == null;

                if (isCreateCancelToken)
                    CancellationToken = new CancellationTokenSource();
                else
                    CancellationToken = cancellationToken;

                Task = Task.Factory.StartNew(TaskProc, taskCreationOptions);

                TaskCreationOptions = taskCreationOptions;
            }

            Trigger.Reset();
        }

        private void TaskProc()
        {
            while (!CancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Set되기전까지 계속 대기탐 
                    Trigger.Wait();

                    IsRunning = true;

                    var list = new List<T>();

                    while (DataQueue.TryDequeue(out var data))
                    {
                        if (!CancellationToken.IsCancellationRequested)
                        {
                            SingleObjectProc(data);
                            list.Add(data);
                        }
                    }

                    if (CancellationToken != null && CancellationToken.IsCancellationRequested)
                        break;

                    MultiObjectProc(list);

                    IsRunning = false;

                    if (DataQueue.IsEmpty)
                        Trigger.Reset();
                }
                catch (Exception ex)
                {
                }
            }
        }

        public void StopTask()
        {
            Dispose();
            CreateTask(TaskCreationOptions, isCreateCancelToken ? null : CancellationToken);
        }

        public bool IsCancel()
        {
            return Task == null || (CancellationToken != null && CancellationToken.IsCancellationRequested);
        }

        public void Cancle()
        {
            if (CancellationToken != null)
                CancellationToken.Cancel();
        }

        public void Dispose()
        {
            if (Task != null)
            {
                try
                {
                    CancellationToken?.Cancel();
                    Trigger.Set();
                    Task.Wait();

                    if (isCreateCancelToken)
                        CancellationToken.Dispose();
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    Trigger.Dispose();
                    Task?.Dispose();

                    if (typeof(T) is IDisposable)
                    {
                        while (DataQueue.TryDequeue(out var data))
                        {
                            ((IDisposable)data)?.Dispose();
                        }
                    }
                }
            }

            CancellationToken = null;
            Trigger = null;
            Task = null;
        }

        /// <summary>
        /// 단일 데이터가 추출될때 실행 할 함수 구현
        /// </summary>
        /// <param name="data"></param>
        public abstract void SingleObjectProc(T data);

        /// <summary>
        /// 여러 데이터가 추출될때 실행 할 함수 구현
        /// </summary>
        /// <param name="data"></param>
        public abstract void MultiObjectProc(List<T> data);
    }
}
