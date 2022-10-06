using LottoProgram.Handlers;
using LottoProgram.Helper;
using LottoProgram.Interfaces;
using LottoProgram.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace LottoProgram.ViewModels
{
    /// <summary>
    /// 과제1. 로또 프로그램 만들기
    //- 동행복권 DB에서 역대 로또번호 읽어오기
    //- 로또 번호 뽑기 기능
    //- 역대 로또번호 조회 기능
    //- 로또번호 정보를 내가 만든 클래스로 변환 후 JSON 포맷이 아닌 방식으로 설정 저장/불러오기
    /// </summary>
    /// 

    /// 나의 목표
    /// 1. 딥러닝 적용
    /// 2. DB로 따로 관리
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(PredictionModels, PredictionModels);
        }

        private List<LottoModel> lottoModels = new List<LottoModel>();
        public List<LottoModel> LottoModels
        {
            get => lottoModels;
            set => Set(ref lottoModels, value);
        }

        private LottoModel lottoModel = new LottoModel();
        public LottoModel LottoModel
        {
            get => lottoModel;
            set => Set(ref lottoModel, value);
        }

        private ICommand dbInquireComamnd;
        public ICommand DbInquireCommand => dbInquireComamnd ?? (dbInquireComamnd = new RelayCommand(DbInquireAction));

        public string GetResposeData(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "GET";
            request.Timeout = 20 * 1000;

            try
            {
                using (HttpWebResponse hwr = (HttpWebResponse)request.GetResponse())
                {
                    if (hwr.StatusCode == HttpStatusCode.OK)
                    {
                        Stream respStream = hwr.GetResponseStream();
                        using (StreamReader sr = new StreamReader(respStream))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return default;
        }

        private void DbInquireAction()
        {
            string siteUri = "https://dhlottery.co.kr/gameResult.do?method=byWin";
            var response = GetResposeData(siteUri);
            int curRound = GetCurrentRound(response);
            var modelList = new List<LottoModel>();

            Stopwatch watch = new Stopwatch();
            watch.Start();

            //var loopResult = Parallel.For(1, curRound, i =>
            for (int i = 1; i <= curRound; i++)
            {
                var siteURI = "https://www.dhlottery.co.kr/common.do?method=getLottoNumber&drwNo=";
                var uriPath = $"{siteURI}{i}";
                var responseText = GetResposeData(uriPath);

                var model = new LottoModel();

                JObject jObject = JObject.Parse(responseText);
                model.Index = i;
                model.DateTime = jObject["drwNoDate"].ToString();
                model.Num1 = Convert.ToInt32(jObject["drwtNo1"].ToString());
                model.Num2 = Convert.ToInt32(jObject["drwtNo2"].ToString());
                model.Num3 = Convert.ToInt32(jObject["drwtNo3"].ToString());
                model.Num4 = Convert.ToInt32(jObject["drwtNo4"].ToString());
                model.Num5 = Convert.ToInt32(jObject["drwtNo5"].ToString());
                model.Num6 = Convert.ToInt32(jObject["drwtNo6"].ToString());
                model.BonusNum = Convert.ToInt32(jObject["bnusNo"].ToString());
                model.Amount = string.Format("{0:#,0 won}", Convert.ToDouble(jObject["totSellamnt"].ToString()));

                modelList.Add(model);
            };

            watch.Stop();
            var serachTime = watch.Elapsed;
            Console.WriteLine($"{serachTime.TotalSeconds: 0.000} sec");

            modelList.Reverse();
            LottoModels = modelList/*.OrderBy(x => x.Index).ToList()*/;
        }

        private int GetCurrentRound(string crawlText)
        {
            var split = crawlText?.Split('\n');

            if (split == null || split.Count() < 2)
                return -1;

            var cur = split.Where(x => x.Contains("option value")).FirstOrDefault();
            if (cur == null)
                return -1;

            return Convert.ToInt32(cur.Trim().Split('\"')[1]);
        }

        private ICommand exportCommand;
        public ICommand ExportCommand => exportCommand ?? (exportCommand = new RelayCommand(ExportAction));

        private void ExportAction()
        {
            if (LottoModels == null || LottoModels.Count() == 0)
                return;

            DataExportHandler.Instance.DataTableExcelExport(LottoModels);
        }

        private ICommand importCommand;
        public ICommand ImportCommand => importCommand ?? (importCommand = new RelayCommand(ImportAction));

        private void ImportAction()
        {
            LottoModels = DataExportHandler.Instance.DataTableExcelImport();
        }

        private ICommand predictCommand;
        public ICommand PredictCommnad => predictCommand ?? (predictCommand = new RelayCommand(PredictAction));

        Task predictTask;

        enum TimerStop
        {
            None,
            Num1,
            Num2,
            Num3,
            Num4,
            Num5,
            Num6,
            Bonus,
        }

        private async void PredictAction()
        {
            if (predictTask != null && LottoModels == null)
                return;

            var predictModels = PredictAlgorithm.WeightCalculate(LottoModels);

            TimerStop step = TimerStop.None;

            bool isStart = true;

            Stopwatch stopwatch = new Stopwatch();
            Random random = new Random(DateTime.Now.Millisecond);
            stopwatch.Start();

            predictTask = Task.Factory.StartNew(() =>
            {
                while (isStart)
                {
                    var lottoModel = new LottoModel();

                    switch (step)
                    {
                        case TimerStop.None:
                            lottoModel.Num1 = random.Next(1, 46);
                            lottoModel.Num2 = random.Next(1, 46);
                            lottoModel.Num3 = random.Next(1, 46);
                            lottoModel.Num4 = random.Next(1, 46);
                            lottoModel.Num5 = random.Next(1, 46);
                            lottoModel.Num6 = random.Next(1, 46);
                            lottoModel.BonusNum = random.Next(1, 46);

                            if (stopwatch.ElapsedMilliseconds > 2000)
                                step = TimerStop.Num1;
                            break;
                        case TimerStop.Num1:
                            lottoModel.Num1 = predictModels.Num1;
                            lottoModel.Num2 = random.Next(1, 46);
                            lottoModel.Num3 = random.Next(1, 46);
                            lottoModel.Num4 = random.Next(1, 46);
                            lottoModel.Num5 = random.Next(1, 46);
                            lottoModel.Num6 = random.Next(1, 46);
                            lottoModel.BonusNum = random.Next(1, 46);

                            if (stopwatch.ElapsedMilliseconds > 3000)
                                step = TimerStop.Num2;
                            break;
                        case TimerStop.Num2:
                            lottoModel.Num1 = predictModels.Num1;
                            lottoModel.Num2 = predictModels.Num2;
                            lottoModel.Num3 = random.Next(1, 46);
                            lottoModel.Num4 = random.Next(1, 46);
                            lottoModel.Num5 = random.Next(1, 46);
                            lottoModel.Num6 = random.Next(1, 46);
                            lottoModel.BonusNum = random.Next(1, 46);

                            if (stopwatch.ElapsedMilliseconds > 4000)
                                step = TimerStop.Num3;
                            break;
                        case TimerStop.Num3:
                            lottoModel.Num1 = predictModels.Num1;
                            lottoModel.Num2 = predictModels.Num2;
                            lottoModel.Num3 = predictModels.Num3;
                            lottoModel.Num4 = random.Next(1, 46);
                            lottoModel.Num5 = random.Next(1, 46);
                            lottoModel.Num6 = random.Next(1, 46);
                            lottoModel.BonusNum = random.Next(1, 46);

                            if (stopwatch.ElapsedMilliseconds > 5000)
                                step = TimerStop.Num4;
                            break;
                        case TimerStop.Num4:
                            lottoModel.Num1 = predictModels.Num1;
                            lottoModel.Num2 = predictModels.Num2;
                            lottoModel.Num3 = predictModels.Num3;
                            lottoModel.Num4 = predictModels.Num4;
                            lottoModel.Num5 = random.Next(1, 46);
                            lottoModel.Num6 = random.Next(1, 46);
                            lottoModel.BonusNum = random.Next(1, 46);

                            if (stopwatch.ElapsedMilliseconds > 6000)
                                step = TimerStop.Num5;
                            break;
                        case TimerStop.Num5:
                            lottoModel.Num1 = predictModels.Num1;
                            lottoModel.Num2 = predictModels.Num2;
                            lottoModel.Num3 = predictModels.Num3;
                            lottoModel.Num4 = predictModels.Num4;
                            lottoModel.Num5 = predictModels.Num5;
                            lottoModel.Num6 = random.Next(1, 46);
                            lottoModel.BonusNum = random.Next(1, 46);

                            if (stopwatch.ElapsedMilliseconds > 7000)
                                step = TimerStop.Num6;
                            break;
                        case TimerStop.Num6:
                            lottoModel.Num1 = predictModels.Num1;
                            lottoModel.Num2 = predictModels.Num2;
                            lottoModel.Num3 = predictModels.Num3;
                            lottoModel.Num4 = predictModels.Num4;
                            lottoModel.Num5 = predictModels.Num5;
                            lottoModel.Num6 = predictModels.Num6;
                            lottoModel.BonusNum = random.Next(1, 46);

                            if (stopwatch.ElapsedMilliseconds > 8000)
                                step = TimerStop.Bonus;
                            break;
                        case TimerStop.Bonus:
                            lottoModel.Num1 = predictModels.Num1;
                            lottoModel.Num2 = predictModels.Num2;
                            lottoModel.Num3 = predictModels.Num3;
                            lottoModel.Num4 = predictModels.Num4;
                            lottoModel.Num5 = predictModels.Num5;
                            lottoModel.Num6 = predictModels.Num6;
                            lottoModel.BonusNum = predictModels.BonusNum;
                            isStart = false;
                            break;
                    }

                    //Console.WriteLine($"{step}");
                    LottoModel = lottoModel;
                }
            });

            await predictTask;
            PredictionModels.Add(LottoModel);
        }

        private ObservableCollection<LottoModel> predictionModels = new ObservableCollection<LottoModel>();
        public ObservableCollection<LottoModel> PredictionModels
        {
            get => predictionModels;
            set => Set(ref predictionModels, value);
        }
    }
}
