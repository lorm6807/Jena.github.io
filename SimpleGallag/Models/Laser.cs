using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGallag.Models
{
    public class Laser : ViewModelBase
    {
        private double height;
        public double Height { get => height; set => Set(ref height, value); }

        private double width;
        public double Width { get => width; set => Set(ref width, value); }

        private double x;
        public double X { get => x; set => Set(ref x, value); }

        private double y;
        public double Y { get => y; set => Set(ref y, value); }
    }
}
