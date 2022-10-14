using Common.Interfaces;
using SimpleGallag.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            StartBasicRockDrop,
            CheckBasicRockCrush,
            CheckGameOver,
        }

        private ICommand loadedCommand;
        public ICommand LoadedCommand => loadedCommand ?? (loadedCommand = new RelayCommand(LoadedAction));

        private void LoadedAction()
        {
            //OnPropertyChanged("CanvasWidth");
            //OnPropertyChanged("CanvasHeight");
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

            BindingOperations.EnableCollectionSynchronization(BasicRockItems, BasicRockItems);
            BindingOperations.EnableCollectionSynchronization(SpeedRockItems, SpeedRockItems);

            TaskPauseMap = new Dictionary<TaskType, SemaphoreSlim>();

            TaskPauseMap[TaskType.StartBasicRockDrop] = new SemaphoreSlim(1);
            TaskPauseMap[TaskType.CheckBasicRockCrush] = new SemaphoreSlim(1);
            TaskPauseMap[TaskType.CheckGameOver] = new SemaphoreSlim(1);
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

        private bool isGaming;
        public bool IsGaming
        {
            get => isGaming;
            set => Set(ref isGaming, value);
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
        //private Dictionary<Task, SemaphoreSlim> TaskCompleteMap;

        public ObservableCollection<Rock> BasicRockItems { get; } = new ObservableCollection<Rock>();
        public ObservableCollection<Rock> SpeedRockItems { get; } = new ObservableCollection<Rock>();

        private ICommand startCommand;
        public ICommand StartCommand => startCommand ?? (startCommand = new RelayCommand(StartAction));

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

        private void StartRockDropTask(TaskType taskType, ObservableCollection<Rock> rocks, int sleepTime)
        {
            var xInterval = CanvasWidth / ColumnCount;
            var yInterval = CanvasHeight / (RowCount + 1);

            var xList = new List<int>();
            for (int i = 0; i < ColumnCount; i++)
                xList.Add(i * xInterval);

            var random = new Random(DateTime.Now.Millisecond);
            int index = 0;

            Task.Factory.StartNew(async () =>
            {
                while (IsGaming)
                {
                    await TaskPauseMap[taskType].WaitAsync();
                    TaskPauseMap[taskType].Release();

                    lock (rocks)
                    {
                        var rock = new Rock();
                        index = random.Next(xList.Count);
                        rock.X = xList[index];
                        rock.Y = 0;

                        rocks.Add(rock);

                        foreach (var item in rocks)
                        {
                            if (rock != item)
                                item.Y += yInterval;

                            item.Width = xInterval;
                            item.Height = yInterval;
                            item.Brush = Brushes.Black;
                        }
                    }

                    Thread.Sleep(sleepTime);
                }

            }, TaskCreationOptions.LongRunning);
        }

        private void StartAction()
        {
            IsGaming = true;
            Score = 0;

            BasicRockItems.Clear();
            SpeedRockItems.Clear();

            StartRockDropTask(TaskType.StartBasicRockDrop, BasicRockItems, 1000);
            CheckCrushTask(TaskType.CheckBasicRockCrush, BasicRockItems);
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

            BasicRockItems.Clear();
            SpeedRockItems.Clear();
        }

        private ICommand leftDownCommand;
        public ICommand LeftDownCommand => leftDownCommand ?? (leftDownCommand = new RelayCommand(LeftDownAction));
        public void LeftDownAction()
        {
            var xInterval = CanvasWidth / ColumnCount;

            if (Tank.X > 0)
                Tank.X -= xInterval;

            //OnPropertyChanged("Tank");
        }

        private ICommand rightDownCommand;
        public ICommand RightDownCommand => rightDownCommand ?? (rightDownCommand = new RelayCommand(RightDownAction));
        public void RightDownAction()
        {
            var xInterval = CanvasWidth / ColumnCount;

            if (Tank.X < CanvasWidth - xInterval)
                Tank.X += xInterval;

            //OnPropertyChanged("Tank");
        }

        private ICommand spaceDownCommand;
        public ICommand SpaceDownCommand => spaceDownCommand ?? (spaceDownCommand = new RelayCommand(SpaceDownAction));
        public async void SpaceDownAction()
        {
            if (BasicRockItems == null || BasicRockItems.Count == 0)
                return;

            Tank.Laser.Width = Tank.Width / 10;

            var yList = BasicRockItems.Select(rock => rock.Y).ToList();
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
