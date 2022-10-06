using LottoProgram.Views;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace LottoProgram
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var currentProcess = Process.GetCurrentProcess();

            bool isNew = false;
            Mutex mutex = new Mutex(true, currentProcess.ProcessName, out isNew);

            if (!isNew)
            {
                MessageBox.Show("프로그램이 이미 실행중입니다");
                currentProcess.Kill();
                return;
            }

            try
            {
                Lazy<MainView> mainView = new Lazy<MainView>(() => new MainView());
                MainWindow = mainView.Value;
                MainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"{ ex.Message}");
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}
