using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SimpleGallag.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
        }

        private bool isGaming;
        public bool IsGaming
        {
            get => isGaming;
            set => Set(ref isGaming, value);
        }

        private int columnCount = 5;
        public int ColumnCount
        {
            get => columnCount;
            set => Set(ref columnCount, value);
        }

        private int tankPosition = 2;
        public int TankPosition
        {
            get => tankPosition;
            set => Set(ref tankPosition, value);
        }

        private ICommand leftDownCommand;
        public ICommand LeftDownCommand => leftDownCommand ?? (leftDownCommand = new RelayCommand(LeftDownAction));
        public void LeftDownAction()
        {
            //if (!isGaming)
            //    return;
            if (tankPosition > 0)
                TankPosition = tankPosition - 1;
        }

        private ICommand rightDownCommand;
        public ICommand RightDownCommand => rightDownCommand ?? (rightDownCommand = new RelayCommand(RightDownAction));
        public void RightDownAction()
        {
            //if (!isGaming)
            //    return;
            if (tankPosition < ColumnCount - 1)
                TankPosition = tankPosition + 1;
        }
    }
}
