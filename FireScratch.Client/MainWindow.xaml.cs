using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
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

namespace FireScratch.Client
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void clickCreateUser(object sender, RoutedEventArgs e)
        {
            await SignUpAsync();

        }

        private async void clickLogin(object sender, RoutedEventArgs e)
        {
            await SignInAsync();

        }

        FirebaseAuthLink _authLink = null;
        string apikey = "<api key>";
        string databaseURL = "<database URL>";
        string DatabasePath = "scratch/firecat";

        /// <summary>
        /// サインインを行う
        /// </summary>
        public async Task SignInAsync()
        {
            try
            {
                // 認証するためのオブジェクトを作成
                var auth = new FirebaseAuthProvider(new FirebaseConfig(apikey));

                // 認証を行い、リンクを取得する
                this._authLink = await auth.SignInWithEmailAndPasswordAsync(this.mail.Text, this.passwd.Password);

                this.msg.Text = "サインインに成功しました";

                // データベースの監視を開始
                this.StartWatchingRealtimeValue();
            }
            catch (FirebaseAuthException ex)
            {
                // エラー発生！
                this.msg.Text = "エラー発生しました！エラーコード：" + ex.Reason;
            }
        }

        /// <summary>
        /// ユーザ作成を行う
        /// </summary>
        public async Task SignUpAsync()
        {
            try
            {
                // 認証するためのオブジェクトを作成
                var auth = new FirebaseAuthProvider(new FirebaseConfig(apikey));

                // サインアップを行い、リンクを取得する
                this._authLink = await auth.CreateUserWithEmailAndPasswordAsync(this.mail.Text, this.passwd.Password);

                this.msg.Text = "ユーザ作成に成功しました";
            }
            catch (FirebaseAuthException ex)
            {
                // エラー発生！
                this.msg.Text = "エラー発生しました！エラーコード：" + ex.Reason;
            }
        }


        /// <summary>
        /// データベースのクエリを取得
        /// </summary>
        /// <returns>設定値に応じたデータベースへの参照</returns>
        private ChildQuery GetDatabaseQuery(string path = null)
        {
            if (this._authLink == null)
            {
                throw new NullReferenceException();
            }
            return new FirebaseClient(
                // FirebaseコンソールのWeb APIのとこに書いてあった、firebaseio.comでおわるやつ
                databaseURL,
                new FirebaseOptions
                {
            // 認証の時に手に入れたリンクからトークンを抜く
            AuthTokenAsyncFactory = () => Task.FromResult(this._authLink.FirebaseToken),
                })
                .Child(path ?? this.DatabasePath);      // 参照先パス
        }
        /// <summary>
        /// データベースにテキストを保存
        /// </summary>
        public async Task UploadTextToDatabaseAsync()
        {
            try
            {
                // クエリを取得
                var query = this.GetDatabaseQuery();

                // データを格納する
                await query.PostAsync(new Data { Text = this.data1.Text, });

                this.msg.Text = "データ保存に成功しました";
            }
            catch (FirebaseException ex)
            {
                this.msg.Text = "エラー発生しました！：" + ex.ResponseData;
            }
            catch
            {
                this.msg.Text = "エラー発生しました！";
            }
        }

        /// <summary>
        /// データベースからテキストを取得
        /// /// </summary>
        public async Task DownloadTextFromDatabaseAsync()
        {
            try
            {
                // クエリを取得
                var query = this.GetDatabaseQuery();

                // データを取得する
                var results = await query.OnceAsync<Data>();
                var resultObjects = results.Select(obj => obj.Object);

                this.msg.Text = "取得完了：" + resultObjects.First().Text;
            }
            catch (FirebaseException ex)
            {
                this.msg.Text = "エラー発生しました！：" + ex.ResponseData;
            }
            catch
            {
                this.msg.Text = "エラー発生しました！";
            }
        }

        private async void clickSave(object sender, RoutedEventArgs e)
        {
            await UploadTextToDatabaseAsync();

        }

        private async void clickLoad(object sender, RoutedEventArgs e)
        {
            await DownloadTextFromDatabaseAsync();
        }

        /// <summary>
        /// リアルタイムでのデータベース監視をおこなうインスタンス（を破棄する権限を持つインスタンス）
        /// </summary>
        private IDisposable _realtimeDatabaseWatcher;

        /// <summary>
        /// リアルタイムのデータ監視を開始
        /// </summary>
        public void StartWatchingRealtimeValue()
        {
            // データベースの監視を開始
            this._realtimeDatabaseWatcher =
                this.GetDatabaseQuery("scratch/firecat")
                .AsObservable<Data>()
                .Subscribe(ev =>
                {
                    if (ev?.Object != null)
                    {
                        switch (ev.EventType)
                        {
                            case Firebase.Database.Streaming.FirebaseEventType.InsertOrUpdate:
                                this.Dispatcher.Invoke(() => {
                                    this.reailtime.Text = "監視: 値 " + ev.Object.Text + " が追加されました";
                                });
                                break;
                            case Firebase.Database.Streaming.FirebaseEventType.Delete:
                                this.Dispatcher.Invoke(() => {
                                    this.reailtime.Text = "監視: 値 " + ev.Object.Text + " が削除されました";
                                });
                                break;
                        }
                    }
                });

            // 監視を中止（なぜDisposeなのか詳しいことはRxの勉強へGo!）
            // this._realtimeDatabaseWatcher.Dispose();
        }
    }
    /// <summary>
    /// Firebase で扱う構造体
    /// </summary>
    public class Data
    {
        public string Text { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

}
