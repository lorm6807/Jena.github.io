using LottoProgram.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LottoProgram.Handlers
{
    public class DataExportHandler : TaskHandler<LottoModel>
    {
        public DataExportHandler()
        {
            CreateTask();
        }

        private static DataExportHandler instance;
        public static DataExportHandler Instance => instance ?? (instance = new DataExportHandler());

        //요걸로 하면 굳이 multi로 안해도 되긴한데..
        public override void MultiObjectProc(List<LottoModel> data)
        {

        }

        public override void SingleObjectProc(LottoModel data)
        {

        }

        public List<LottoModel> DataTableExcelImport()
        {
            string path = @"c:\debug\Lotto\LottoNum.csv";
            var texts = File.ReadAllText(path);
            string[] splitTextArray = texts.Split('\n');
            List<LottoModel> lottoModels = new List<LottoModel>();

            foreach (var splitText in splitTextArray)
            {
                if (splitText == "" || splitText.Length < 0)
                    continue;

                var lottoText = splitText.Trim().Split(',');

                var newModel = new LottoModel();
                newModel.Index = Convert.ToInt32(lottoText[0]);
                newModel.DateTime = lottoText[1];
                newModel.Num1 = Convert.ToInt32(lottoText[2]);
                newModel.Num2 = Convert.ToInt32(lottoText[3]);
                newModel.Num3 = Convert.ToInt32(lottoText[4]);
                newModel.Num4 = Convert.ToInt32(lottoText[5]);
                newModel.Num5 = Convert.ToInt32(lottoText[6]);
                newModel.Num6 = Convert.ToInt32(lottoText[7]);
                newModel.BonusNum = Convert.ToInt32(lottoText[8]);

                lottoModels.Add(newModel);
            }

            return lottoModels;
        }

        public void DataTableExcelExport(List<LottoModel> lottoModels)
        {
            string path = @"c:\debug\Lotto";
            using (DataTable dt = new DataTable())
            {
                dt.Columns.Add("회차", typeof(string));
                dt.Columns.Add("날짜", typeof(string));
                dt.Columns.Add("당첨번호1", typeof(string));
                dt.Columns.Add("당첨번호2", typeof(string));
                dt.Columns.Add("당첨번호3", typeof(string));
                dt.Columns.Add("당첨번호4", typeof(string));
                dt.Columns.Add("당첨번호5", typeof(string));
                dt.Columns.Add("당첨번호6", typeof(string));
                dt.Columns.Add("보너스 번호", typeof(string));

                foreach (var lottoModel in lottoModels)
                {
                    dt.Rows.Add($"{lottoModel.Index}");
                    dt.Rows.Add($"{lottoModel.DateTime}");
                    dt.Rows.Add($"{lottoModel.Num1}");
                    dt.Rows.Add($"{lottoModel.Num2}");
                    dt.Rows.Add($"{lottoModel.Num3}");
                    dt.Rows.Add($"{lottoModel.Num4}");
                    dt.Rows.Add($"{lottoModel.Num5}");
                    dt.Rows.Add($"{lottoModel.Num6}");
                    dt.Rows.Add($"{lottoModel.BonusNum}");
                }

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                WriteCsv($@"{path}\LottoNum.csv", dt);
            }
        }

        private void WriteCsv(string path, DataTable dataTable)
        {
            StringBuilder sb = new StringBuilder();
            foreach (DataRow row in dataTable.Rows)
            {
                foreach (var item in row.ItemArray)
                    sb.Append($@"{item},");

                sb.AppendLine();
            }

            File.WriteAllText(path, sb.ToString());
        }
    }
}
