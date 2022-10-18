using Common.Interfaces;
using SimpleGallag.Handlers;
using SimpleGallag.Models;
using SimpleGallag.Views;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SimpleGallag.ViewModels
{
    public static class UiRefresh
    {
        private static Action EmptyDelegate = delegate () { };

        public static void Refresh(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }
    }

    public enum SpeedType
    {
        Normal,
        Fast1,
        Fast2,
        Fast3,
    }

    public enum LevelType
    {
        Easy,
        Normal,
        Hard,
    }

    public class MainViewModel : ViewModelBase
    {
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

            TaskPause = new ManualResetEventSlim(true);

            RockMap = new Dictionary<SpeedType, ObservableCollection<Rock>>();
            RockMap.Add(SpeedType.Normal, NormalRockItems);
            RockMap.Add(SpeedType.Fast1, HardRockItems);
            RockMap.Add(SpeedType.Fast2, MoreHardRockItems);
            RockMap.Add(SpeedType.Fast3, BonusRockItems);

            var imageBrush = new ImageBrush();
            imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/Tank2.png"));
            TankImageBrush = imageBrush;

            var skyImageBrush = new ImageBrush();
            skyImageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/Sky.jpg"));
            SkyImageBrush = skyImageBrush;
        }

        private ICommand loadedCommand;
        public ICommand LoadedCommand => loadedCommand ?? (loadedCommand = new RelayCommand<UIElement>(LoadedAction));

        UIElement Canvas;
        private void LoadedAction(UIElement obj)
        {
            Canvas = ((MainView)obj).canvas;
        }

        private LevelType levelType;
        public LevelType LevelType
        {
            get => levelType;
            set => Set(ref levelType, value);
        }

        private ImageBrush skyImageBrush;
        public ImageBrush SkyImageBrush
        {
            get => skyImageBrush;
            set => Set(ref skyImageBrush, value);
        }

        private ImageBrush tankImageBrush;
        public ImageBrush TankImageBrush
        {
            get => tankImageBrush;
            set => Set(ref tankImageBrush, value);
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

        private int columnCount = 10;
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

        private bool isEasy = true;
        public bool IsEasy
        {
            get => isEasy;
            set
            {
                if (Set(ref isEasy, value) && value)
                    LevelType = LevelType.Easy;
            }
        }

        private bool isNormal = false;
        public bool IsNormal
        {
            get => isNormal;
            set
            {
                if (Set(ref isNormal, value) && value)
                    LevelType = LevelType.Normal;
            }
        }

        private bool isHard = false;
        public bool IsHard
        {
            get => isHard;
            set
            {
                if (Set(ref isHard, value) && value)
                    LevelType = LevelType.Hard;
            }
        }

        private Tank tank;
        public Tank Tank
        {
            get => tank;
            set => Set(ref tank, value);
        }

        private ManualResetEventSlim TaskPause;
        private Dictionary<SpeedType, ObservableCollection<Rock>> RockMap;

        public ObservableCollection<Rock> NormalRockItems { get; } = new ObservableCollection<Rock>();
        public ObservableCollection<Rock> HardRockItems { get; } = new ObservableCollection<Rock>();
        public ObservableCollection<Rock> MoreHardRockItems { get; } = new ObservableCollection<Rock>();
        public ObservableCollection<Rock> BonusRockItems { get; } = new ObservableCollection<Rock>();

        private ICommand startCommand;
        public ICommand StartCommand => startCommand ?? (startCommand = new RelayCommand(StartAction));

        Task checkTask;
        private void CheckGameOverTask(ObservableCollection<Rock> rocks)
        {
            checkTask = Task.Factory.StartNew(() =>
             {
                 while (IsGaming)
                 {
                     TaskPause.Wait();

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
                     Thread.Sleep(100);
                 }
             }, TaskCreationOptions.LongRunning);

        }

        private void CheckCrushTask(ObservableCollection<Rock> rocks)
        {
            Task.Factory.StartNew(() =>
            {
                while (IsGaming)
                {
                    TaskPause.Wait();

                    var removeList = new List<Rock>();
                    lock (rocks)
                    {
                        foreach (var item in rocks)
                        {
                            if (Tank.Laser.Y != 0 && item.Y + item.Height >= Tank.Laser.Y && Tank.X == item.X)
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

        private void StartRockDropTask(ObservableCollection<Rock> rocks, SpeedType levelType)
        {
            var xInterval = CanvasWidth / ColumnCount;
            var yInterval = CanvasHeight / (RowCount + 1);

            var xList = new List<int>();
            for (int i = 0; i < ColumnCount; i++)
                xList.Add(i * xInterval);

            var random = new Random(DateTime.Now.Millisecond);
            int index = 0;
            double ratio = 1;

            if (IsEasy)
                ratio = 1;
            else if (IsNormal)
                ratio = 0.7;
            else if (isHard)
                ratio = 0.5;

            int sleepTime = 1000;
            int score = 10;
            int percent = 100;
            ImageBrush imageBrush = new ImageBrush();

            switch (levelType)
            {
                case SpeedType.Normal:
                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/Rock1.png"));
                    sleepTime = (int)(2000 * ratio);
                    percent = 80;
                    score = 10;
                    break;
                case SpeedType.Fast1:
                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/Rock2.png"));
                    sleepTime = (int)(1500 * ratio);
                    percent = 40;
                    score = 20;
                    break;
                case SpeedType.Fast2:
                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/Rock3.png"));
                    sleepTime = (int)(1000 * ratio);
                    percent = 20;
                    score = 30;
                    break;
                case SpeedType.Fast3:
                    imageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/Rock5.png"));
                    sleepTime = (int)(500 * ratio);
                    percent = 10;
                    score = 100;
                    break;
            }

            //imageBrush.Opacity = 0.8;
            imageBrush.Freeze();
            Task.Factory.StartNew(() =>
            {
                var randomValue = new Random(DateTime.Now.Millisecond);

                while (IsGaming)
                {
                    TaskPause.Wait();

                    lock (rocks)
                    {
                        Rock rock = null;

                        var randomPercent = randomValue.Next(0, 100);
                        if (randomPercent < percent)
                        {
                            rock = new Rock();
                            index = random.Next(xList.Count);
                            rock.X = xList[index];
                            rock.Score = score;
                            rock.Y = 0;
                            rock.Brush = imageBrush;
                            rocks.Add(rock);
                        }

                        foreach (var item in rocks)
                        {
                            if (rock != item)
                                item.Y += yInterval;

                            item.Width = xInterval;
                            item.Height = yInterval;
                        }
                    }

                    UiRefresh.Refresh(Canvas);
                    Thread.Sleep(sleepTime);
                }

            }, TaskCreationOptions.LongRunning);
        }

        private void StartAction()
        {
            //SemaphoreSlim Test
            //if (reset == null)
            //    reset = new SemaphoreSlim(0);

            //var random = new Random();
            //var releaseCount = random.Next(10) + 1;
            //Console.WriteLine($"Release Count : {releaseCount}");

            //reset.Release(releaseCount);

            //int outputIndex = 0;

            //Task.Run(() =>
            //{
            //    while (true)
            //    {
            //        reset.Wait();
            //        Console.WriteLine($"Thread Repeat Count : {outputIndex++}");
            //    }
            //});

            //return;

            IsGaming = true;
            IsGameOver = false;
            Score = 0;

            foreach (var rock in RockMap.Values)
                rock.Clear();

            Parallel.For(0, RockMap.Count, index =>
            {
                SimpleLogHandler.Instance.Add($"StartRockDropTask[{index}]");
                StartRockDropTask(RockMap[(SpeedType)index], (SpeedType)index);
                CheckCrushTask(RockMap[(SpeedType)index]);
                CheckGameOverTask(RockMap[(SpeedType)index]);
            });
        }

        private ICommand pauseCommand;
        public ICommand PauseCommand => pauseCommand ?? (pauseCommand = new RelayCommand(PauseAction));

        private void PauseAction()
        {
            if (IsPause)
                TaskPause.Reset();
            else
                TaskPause.Set();
        }

        private ICommand stopCommand;
        public ICommand StopCommand => stopCommand ?? (stopCommand = new RelayCommand(StopAction));

        private void StopAction()
        {
            IsGaming = false;
            Score = 0;

            TaskPause.Set();
            checkTask.Wait();

            foreach (var rock in RockMap.Values)
                rock.Clear();
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
            for (int i = RowCount / 2; i < RowCount; i++)
                yList.Add(i * yInterval);

            Tank.Laser.Width = Tank.Width / 20;

            var random = new Random(DateTime.Now.Millisecond);
            int index = random.Next(yList.Count);

            var rockY = yList[index];
            if (rockY > canvasHeight)
                rockY = 0;

            Tank.Laser.Height = /*CanvasHeight - */rockY;
            Tank.Laser.Y = CanvasHeight - rockY;

            await Task.Delay(100);

            Tank.Laser.Y = 0;
            Tank.Laser.Height = 0;

            UiRefresh.Refresh(Canvas);
        }
    }
}
