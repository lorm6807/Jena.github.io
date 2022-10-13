using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SimpleGallag.Models
{
    public delegate void AttackedDelegate(Rock rock);

    public class Rock : ViewModelBase
    {
        private double height;
        public double Height { get => height; set => Set(ref height, value); }

        private double width;
        public double Width { get => width; set => Set(ref width, value); }

        private double x;
        public double X { get => x; set => Set(ref x, value); }

        private double y;
        public double Y { get => y; set => Set(ref y, value); }

        private Brush brush;
        public Brush Brush { get => brush; set => Set(ref brush, value); }

        private int score = 10;
        public int Score { get => score; set => Set(ref score, value); }

        //나중에 색깔같은거 추가해서.. 스피드조정 다르게 하도록..
    }
}
