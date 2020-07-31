using System.Windows;
using SorterIotInterfaceSimulator.ViewModel;

namespace SorterIotInterfaceSimulator.View
{
    /// <summary>
    /// メインウィンドウビュー
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// ビューモデル
        /// </summary>
        MainWindowViewModel viewModel;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // ビューモデル
            this.viewModel = new MainWindowViewModel(this.MessageLogTextBox1);

            // データバインディング
            var bindingData = this.viewModel;
            this.DataContext = bindingData;
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~MainWindow()
        {
            if (this.viewModel != null)
            {
                this.viewModel.Dispose();
                this.viewModel = null;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.viewModel != null)
            {
                this.viewModel.Dispose();
                this.viewModel = null;
            }
        }

        // MQTT Broker 接続・切断
        public void SendConnect(object sender, RoutedEventArgs e)
        {
            // @@@ chg USF-200_機能改善 2018.08.03 by Hemmi ↓
            if (this.viewModel.PortNo == "8883")
            {
                this.viewModel.SendConnectTlsFunc();
            }
            else
            {
                this.viewModel.SendConnectFunc();
            }
            // @@@ chg USF-200_機能改善 2018.08.03 by Hemmi ↑
        }
        // @@@ del USF-200_機能改善 2018.08.03 by Hemmi ↓
        //public void SendConnectTls(object sender, RoutedEventArgs e)
        //{
        //    this.viewModel.SendConnectTlsFunc();
        //}
        // @@@ del USF-200_機能改善 2018.08.03 by Hemmi ↑
        public void SendDisconnect(object sender, RoutedEventArgs e)
        {
            this.viewModel.SendDisconnectFunc();
        }

        // MQTT 送信
        public void SendPublishJson(object sender, RoutedEventArgs e)
        {
            this.viewModel.SendPublishJsonFunc();
        }
        public void SendSubscribe(object sender, RoutedEventArgs e)
        {
            this.viewModel.SendSubscribeFunc();
        }
        public void SendUnSubscribe(object sender, RoutedEventArgs e)
        {
            this.viewModel.SendUnSubscribeFunc();
        }

        // メッセージ読込・選択
        public void LeadMessage(object sender, RoutedEventArgs e)
        {
            this.viewModel.LeadMessageFunc();
        }
        public void SelectMessage(object sender, RoutedEventArgs e)
        {
            this.viewModel.SelectMessageFunc();
        }

        // FTP
        public void CreateDirFTP(object sender, RoutedEventArgs e)
        {
            this.viewModel.CreateDirFTPFunc();
        }
        public void RemoveDirFTP(object sender, RoutedEventArgs e)
        {
            this.viewModel.RemoveDirFTPFunc();
        }
        public void UploadFilesFTP(object sender, RoutedEventArgs e)
        {
            this.viewModel.UploadFilesFTPFunc();
        }
        public void DownloadFilesFTP(object sender, RoutedEventArgs e)
        {
            this.viewModel.DownloadFilesFTPFunc();
        }

        // @@@ add USF-200_機能改善 2017.12.27 by Hemmi ↓
        // ログクリア
        public void ClearLog(object sender, RoutedEventArgs e)
        {
            this.MessageLogTextBox1.Clear();
        }
        // @@@ add USF-200_機能改善 2017.12.27 by Hemmi ↑

        // @@@ add USF-200_機能改善 2018.05.22 by Hemmi ↓
        private void MessageLogTextBox1_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // 自動スクロール
            this.MessageLogTextBox1.ScrollToEnd();
        }

        private void FontSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double fontSize = (int)(e.NewValue * 10) / 10;  // 小数点以下切り捨て

            // フォントサイズ変更
            this.SendMessageTextBox1.FontSize = fontSize;
            this.MessageLogTextBox1.FontSize = fontSize;

            this.FontSizeSlider.ToolTip = string.Format("FontSize: {0}", fontSize);
        }
        // @@@ add USF-200_機能改善 2018.05.22 by Hemmi ↑

    }
}
