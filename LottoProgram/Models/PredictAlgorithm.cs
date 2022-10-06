using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LottoProgram.Models
{
    public static class PredictAlgorithm
    {
        public static LottoModel WeightCalculate(List<LottoModel> lottoModels)
        {
            var newModel = new LottoModel();

            Random random = new Random(DateTime.Now.Millisecond);

            if (lottoModels.Count() == 0)
                return newModel;

            double count = lottoModels.Count();

            var num1 = lottoModels.Select(x => x.Num1).GroupBy(x => x).ToDictionary(k => k.Key, v => v.Count() / count * 100).GetWeightedRandom(random);
            var num2 = lottoModels.Select(x => x.Num2).GroupBy(x => x).ToDictionary(k => k.Key, v => v.Count() / count * 100).GetWeightedRandom(random, new List<int> { num1 });
            var num3 = lottoModels.Select(x => x.Num3).GroupBy(x => x).ToDictionary(k => k.Key, v => v.Count() / count * 100).GetWeightedRandom(random, new List<int> { num1, num2 });
            var num4 = lottoModels.Select(x => x.Num4).GroupBy(x => x).ToDictionary(k => k.Key, v => v.Count() / count * 100).GetWeightedRandom(random, new List<int> { num1, num2, num3 });
            var num5 = lottoModels.Select(x => x.Num5).GroupBy(x => x).ToDictionary(k => k.Key, v => v.Count() / count * 100).GetWeightedRandom(random, new List<int> { num1, num2, num3, num4 });
            var num6 = lottoModels.Select(x => x.Num6).GroupBy(x => x).ToDictionary(k => k.Key, v => v.Count() / count * 100).GetWeightedRandom(random, new List<int> { num1, num2, num3, num4, num5 });
            var bonus = lottoModels.Select(x => x.BonusNum).GroupBy(x => x).ToDictionary(k => k.Key, v => v.Count() / count * 100).GetWeightedRandom(random, new List<int> { num1, num2, num3, num4, num5, num6 });

            newModel.Num1 = num1;
            newModel.Num2 = num2;
            newModel.Num3 = num3;
            newModel.Num4 = num4;
            newModel.Num5 = num5;
            newModel.Num6 = num6;
            newModel.BonusNum = bonus;

            return newModel;
        }

        public static int GetWeightedRandom(this Dictionary<int, double> itemWeightDict, Random random, List<int> exceptNum = null)
        {
            if (itemWeightDict == null || itemWeightDict.Count() <= 0)
                return -1;

            double bestValue = double.MaxValue;
            int resultValue = 0;

            int randomValue = random.Next(1, 46);

            while (exceptNum != null && exceptNum.Contains(randomValue))
            {
                randomValue = random.Next(1, 46);
            }

            foreach (var keyValue in itemWeightDict)
            {
                double value = -Math.Log(randomValue) / keyValue.Value;

                if (value < bestValue)
                {
                    bestValue = value;
                    resultValue = randomValue;
                }
            }

            return resultValue;
        }

        public static double WeightedAverage(this Dictionary<int, int> keyValuePairs)
        {
            double sum = 0;
            double count = 0;

            foreach (var keyValue in keyValuePairs)
            {
                sum += keyValue.Key * keyValue.Value;
                count += keyValue.Value;
            }

            return sum / count;
        }

        public static LottoModel DeepLeaningPredict(List<LottoModel> lottoModels)
        {
            var newModel = new LottoModel();

            return newModel;
        }
    }
}
