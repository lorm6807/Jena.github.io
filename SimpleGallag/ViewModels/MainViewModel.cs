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

            BindingOperations.EnableCollectionSynchronization(RockItems, RockItems);
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

        //private int tankPosition = 2;
        //public int TankPosition
        //{
        //    get => tankPosition;
        //    set => Set(ref tankPosition, value);
        //}

        private Tank tank;
        public Tank Tank
        {
            get => tank;
            set => Set(ref tank, value);
        }

        //일단 바인딩 암케나 걸어보자..
        public ObservableCollection<Rock> RockItems { get; } = new ObservableCollection<Rock>();

        private ICommand startCommand;
        public ICommand StartCommand => startCommand ?? (startCommand = new RelayCommand(StartAction));

        private void StartAction()
        {
            IsGaming = true;
            RockItems.Clear();
            // 쓰레드를 만들어
            // 몇개를 만드냐면..
            // 캔버스를 기준으로 컬럼 개수로 나눈거..
            // 캔버스의 영역을 어떻게 가져오지?
            var xInterval = CanvasWidth / ColumnCount;
            var yInterval = CanvasHeight / (RowCount + 1);

            Task.Factory.StartNew(() =>
            {
                while (IsGaming)
                {
                    lock (RockItems)
                    {
                        var rock = new Rock();
                        rock.Y = 0;
                        RockItems.Add(rock);

                        foreach (var item in RockItems)
                        {
                            item.X = 0;
                            if (rock != item)
                                item.Y += yInterval;
                            item.Width = xInterval;
                            item.Height = yInterval;
                            item.Brush = Brushes.Black;
                        }
                    }

                    Thread.Sleep(1000);
                }

            }, TaskCreationOptions.LongRunning);

            Task.Factory.StartNew(() =>
            {
                while (IsGaming)
                {
                    var removeList = new List<Rock>();
                    lock (RockItems)
                    {
                        foreach (var item in RockItems)
                        {
                            if (Tank.Laser.Y != 0 && item.Y >= Tank.Laser.Y)
                                removeList.Add(item);
                        }

                        foreach (var item in removeList)
                        {
                            RockItems.Remove(item);
                        }
                    }

                    Thread.Sleep(100);
                }
            }, TaskCreationOptions.LongRunning);
        }

        private ICommand pauseCommand;
        public ICommand PauseCommand => pauseCommand ?? (pauseCommand = new RelayCommand(PauseAction));

        private void PauseAction()
        {

        }

        private ICommand stopCommand;
        public ICommand StopCommand => stopCommand ?? (stopCommand = new RelayCommand(StopAction));

        private void StopAction()
        {
            IsGaming = false;
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
            // TODO : [Jena] 해당 포지션에 있는 Thread의 트리거를 셋한다..?
            //if (!isGaming)
            //    return;

            if (RockItems == null || RockItems.Count == 0)
                return;

            //var laser = new Laser();
            //Tank.IsAttack = true;
            Tank.Laser.Width = Tank.Width / 10;

            //해당 쓰레드에 있는 Y 찾아가지고 갱신해주기!
            var rockY = RockItems.FirstOrDefault().Y;
            if (rockY > canvasHeight)
                rockY = 0;

            Tank.Laser.Height = CanvasHeight - rockY;
            //Tank.Laser.X = Tank.X + Tank.Width / 2;
            Tank.Laser.Y = rockY;

            await Task.Delay(100);

            Tank.Laser.Y = 0;
            Tank.Laser.Height = 0;

            //Tank.IsAttack = false;

            //OnPropertyChanged("Tank");
        }
    }
}
