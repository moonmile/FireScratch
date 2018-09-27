using FireScratch.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FireScratch
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }
        MainViewModel _vm; 
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _vm = new MainViewModel();
            _vm.LoadConfig();
            this.DataContext = _vm;
            // パスワードだけ別口で設定する
            this.pw.Password = _vm.Password;

        }

        /// <summary>
        /// 実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cllicRun(object sender, RoutedEventArgs e)
        {
            this._vm.Password = this.pw.Password;
            _vm.Run();
            this.btnRun.IsEnabled = false;
            this.btnRun.Content = "Running...";
        }
    }
}
