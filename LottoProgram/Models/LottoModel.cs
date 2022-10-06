using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LottoProgram.Models
{
    public class LottoModel
    {
        public int Index { get; set; }
        public string DateTime { get; set; }
        public int Num1 { get; set; }
        public int Num2 { get; set; }
        public int Num3 { get; set; }
        public int Num4 { get; set; }
        public int Num5 { get; set; }
        public int Num6 { get; set; }
        public int BonusNum { get; set; }
        public string Amount { get; set; }
    }
}
