using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGallag.Models
{
    public class Tank : ViewModelBase
    {
        private double height;
        public double Height { get => height; set => Set(ref height, value); }

        private double width;
        public double Width { get => width; set => Set(ref width, value); }

        private double x;
        public double X
        {
            get => x;
            set
            {
                Set(ref x, value);

                Laser.X = x + Width / 2;
            }
        }

        private double y;
        public double Y { get => y; set => Set(ref y, value); }

        private bool isAttack;
        public bool IsAttack { get => isAttack; set => Set(ref isAttack, value); }

        private Laser laser = new Laser();
        public Laser Laser { get => laser; set => Set(ref laser, value); }
    }
}
