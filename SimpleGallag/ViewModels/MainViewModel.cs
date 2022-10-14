using Common.Interfaces;
using SimpleGallag.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace SimpleGallag.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        enum TaskType
        {
            StartRockDrop,
            CheckRockCrush,
            CheckGameOver,
        }

        enum LevelType
        {
            Normal,
            Hard,
            MoreHard,
            Bonus,
        }

        public MainViewModel()
        {
            var xInterval = CanvasWidth / ColumnCount;
            var yInterval = CanvasHeight / (RowCount + 1);

            var tank = new Tank();
            tank.X = xInterval * (ColumnCount / 2);
            tank.Y = CanvasHeight - yInterval;
            tank.Width = xInterval;
            tank.Height = yInterval;

            Tank = tank;

            BindingOperations.EnableCollectionSynchronization(NormalRockItems, NormalRockItems);
            BindingOperations.EnableCollectionSynchronization(HardRockItems, HardRockItems);
            BindingOperations.EnableCollectionSynchronization(MoreHardRockItems, HardRockItems);
            BindingOperations.EnableCollectionSynchronization(BonusRockItems, HardRockItems);

            TaskPauseMap = new Dictionary<TaskType, SemaphoreSlim>();

            TaskPauseMap[TaskType.StartRockDrop] = new SemaphoreSlim(1);
            TaskPauseMap[TaskType.CheckRockCrush] = new SemaphoreSlim(1);
            TaskPauseMap[TaskType.CheckGameOver] = new SemaphoreSlim(1);

            RockMap = new Dictionary<LevelType, ObservableCollection<Rock>>();
            RockMap.Add(LevelType.Normal, NormalRockItems);
            RockMap.Add(LevelType.Hard, HardRockItems);
            RockMap.Add(LevelType.MoreHard, MoreHardRockItems);
            RockMap.Add(LevelType.Bonus, BonusRockItems);
        }

        private int canvasWidth = 700;
        public int CanvasWidth
        {
            get => canvasWidth;
            set => Set(ref canvasWidth, value);
        }

        private int canvasHeight = 500;
        public int CanvasHeight
        {
            get => canvasHeight;
            set => Set(ref canvasHeight, value);
        }

        private bool isGaming = false;
        public bool IsGaming
        {
            get => isGaming;
            set => Set(ref isGaming, value);
        }

        private bool isGameOver = false;
        public bool IsGameOver
        {
            get => isGameOver;
            set => Set(ref isGameOver, value);
        }

        private bool isPause;
        public bool IsPause
        {
            get => isPause;
            set => Set(ref isPause, value);
        }

        private int score;
        public int Score
        {
            get => score;
            set => Set(ref score, value);
        }

        private int columnCount = 5;
        public int ColumnCount
        {
            get => columnCount;
            set => Set(ref columnCount, value);
        }

        private int rowCount = 10;
        public int RowCount
        {
            get => rowCount;
            set => Set(ref rowCount, value);
        }

        private Tank tank;
        public Tank Tank
        {
            get => tank;
            set => Set(ref tank, value);
        }

        private Dictionary<TaskType, SemaphoreSlim> TaskPauseMap;
        private Dictionary<LevelType, ObservableCollection<Rock>> RockMap;

        public ObservableCollection<Rock> NormalRockItems { get; } = new ObservableCollection<Rock>();
        public ObservableCollection<Rock> HardRockItems { get; } = new ObservableCollection<Rock>();
        public ObservableCollection<Rock> MoreHardRockItems { get; } = new ObservableCollection<Rock>();
        public ObservableCollection<Rock> BonusRockItems { get; } = new ObservableCollection<Rock>();

        private ICommand startCommand;
        public ICommand StartCommand => startCommand ?? (startCommand = new RelayCommand(StartAction));

        private void CheckGameOverTask(TaskType taskType, ObservableCollection<Rock> rocks)
        {
            Task.Factory.StartNew(async () =>
            {
                while (IsGaming)
                {
                    await TaskPauseMap[taskType].WaitAsync();
                    TaskPauseMap[taskType].Release();

                    lock (rocks)
                    {
                        foreach (var item in rocks)
                        {
                            if (item.Y + item.Height >= CanvasHeight)
                            {
                                IsGaming = false;
                                IsGameOver = true;
                            }
                        }
                    }
                }
                Thread.Sleep(100);
            }, TaskCreationOptions.LongRunning);
        }

        private void CheckCrushTask(TaskType taskType, ObservableCollection<Rock> rocks)
        {
            Task.Factory.StartNew(async () =>
            {
                while (IsGaming)
                {
                    await TaskPauseMap[taskType].WaitAsync();
                    TaskPauseMap[taskType].Release();

                    var removeList = new List<Rock>();
                    lock (rocks)
                    {
                        foreach (var item in rocks)
                        {
                            if (Tank.Laser.Y != 0 && item.Y >= Tank.Laser.Y && Tank.X == item.X)
                                removeList.Add(item);
                        }

                        foreach (var item in removeList)
                        {
                            rocks.Remove(item);
                            Score += item.Score;
                        }
                    }

                    Thread.Sleep(100);
                }
            }, TaskCreationOptions.LongRunning);
        }

        private void StartRockDropTask(TaskType taskType, ObservableCollection<Rock> rocks, LevelType levelType)
        {
            var xInterval = CanvasWidth / ColumnCount;
            var yInterval = CanvasHeight / (RowCount + 1);

            var xList = new List<int>();
            for (int i = 0; i < ColumnCount; i++)
                xList.Add(i * xInterval);

            var random = new Random(DateTime.Now.Millisecond);
            int index = 0;

            int sleepTime = 1000;
            var brush = Brushes.Black;
            int score = 10;
            switch (levelType)
            {
                case LevelType.Normal:
                    sleepTime = 1000;
                    brush = Brushes.Black;
                    score = 10;
                    break;
                case LevelType.Hard:
                    sleepTime = 700;
                    brush = Brushes.DarkGray;
                    score = 20;
                    break;
                case LevelType.MoreHard:
                    sleepTime = 200;
                    brush = Brushes.DimGray;
                    score = 30;
                    break;
                case LevelType.Bonus:
                    brush = Brushes.Blue;
                    sleepTime = 100;
                    score = 100;
                    break;
            }

            Task.Factory.StartNew(async () =>
            {
                Stopwatch sw = new Stopwatch();

                while (IsGaming)
                {
                    sw.Start();

                    await TaskPauseMap[taskType].WaitAsync();
                    TaskPauseMap[taskType].Release();

                    //lock (rocks)
                    {
                        var rock = new Rock();

                        while (sw.ElapsedMilliseconds > sleepTime * 10)
                        {
                            index = random.Next(xList.Count);
                            rock.X = xList[index];
                            rock.Score = score;
                            rock.Y = 0;
                            rocks.Add(rock);

                            sw.Restart();
                        }

                        foreach (var item in rocks)
                        {
                            if (rock != item)
                                item.Y += yInterval;

                            item.Width = xInterval;
                            item.Height = yInterval;
                            item.Brush = brush;
                        }
                    }

                    Thread.Sleep(sleepTime);
                }

            }, TaskCreationOptions.LongRunning);
        }

        private void StartAction()
        {
            IsGaming = true;
            IsGameOver = false;
            Score = 0;

            foreach (var rock in RockMap.Values)
                rock.Clear();

            Parallel.For(0, RockMap.Count, index =>
            {
                StartRockDropTask(TaskType.StartRockDrop, RockMap[(LevelType)index], (LevelType)index);
                CheckCrushTask(TaskType.CheckRockCrush, RockMap[(LevelType)index]);
                CheckGameOverTask(TaskType.CheckRockCrush, RockMap[(LevelType)index]);
            });
        }

        private ICommand pauseCommand;
        public ICommand PauseCommand => pauseCommand ?? (pauseCommand = new RelayCommand(PauseAction));

        private async void PauseAction()
        {
            if (IsPause)
            {
                foreach (var pause in TaskPauseMap.Values)
                    await pause.WaitAsync();
            }
            else
            {
                foreach (var pause in TaskPauseMap.Values)
                    pause.Release();
            }
        }

        private ICommand stopCommand;
        public ICommand StopCommand => stopCommand ?? (stopCommand = new RelayCommand(StopAction));

        private void StopAction()
        {
            IsGaming = false;
            Score = 0;
            foreach (var pause in TaskPauseMap.Values)
            {
                if (pause.CurrentCount == 0)
                    pause.Release();
            }

            NormalRockItems.Clear();
            HardRockItems.Clear();
        }

        private ICommand leftDownCommand;
        public ICommand LeftDownCommand => leftDownCommand ?? (leftDownCommand = new RelayCommand(LeftDownAction));
        public void LeftDownAction()
        {
            var xInterval = CanvasWidth / ColumnCount;

            if (Tank.X > 0)
                Tank.X -= xInterval;
        }

        private ICommand rightDownCommand;
        public ICommand RightDownCommand => rightDownCommand ?? (rightDownCommand = new RelayCommand(RightDownAction));
        public void RightDownAction()
        {
            var xInterval = CanvasWidth / ColumnCount;

            if (Tank.X < CanvasWidth - xInterval)
                Tank.X += xInterval;
        }

        private ICommand spaceDownCommand;
        public ICommand SpaceDownCommand => spaceDownCommand ?? (spaceDownCommand = new RelayCommand(SpaceDownAction));

        public async void SpaceDownAction()
        {
            var yInterval = CanvasHeight / (RowCount + 1);
            var yList = new List<int>();
            for (int i = 2; i < RowCount; i++)
                yList.Add(i * yInterval);

            Tank.Laser.Width = Tank.Width / 10;

            var random = new Random(DateTime.Now.Millisecond);
            int index = random.Next(yList.Count);

            var rockY = yList[index];
            if (rockY > canvasHeight)
                rockY = 0;

            Tank.Laser.Height = CanvasHeight - rockY;
            Tank.Laser.Y = rockY;

            await Task.Delay(100);

            Tank.Laser.Y = 0;
            Tank.Laser.Height = 0;
        }
    }
}
