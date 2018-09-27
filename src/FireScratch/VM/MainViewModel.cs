using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FireScratch.VM
{
    class MainViewModel : ObservableObject
    {
        string _ApiKey = "";
        string _DatabaseURL = "";
        string _DatabasePath = "";
        string _Email = "";
        string _Password = "";
        int _Port = 5411;


        public string ApiKey { get => _ApiKey; set => SetProperty(ref _ApiKey, value, nameof(ApiKey)); }
        public string DatabaseURL { get => _DatabaseURL; set => SetProperty(ref _DatabaseURL, value, nameof(DatabaseURL)); }
        public string DatabasePath { get => _DatabasePath; set => SetProperty(ref _DatabasePath, value, nameof(DatabasePath)); }
        public string Email { get => _Email; set => SetProperty(ref _Email, value, nameof(Email)); }
        public string Password { get => _Password; set => SetProperty(ref _Password, value, nameof(Password)); }
        public int Port { get => _Port; set => SetProperty(ref _Port, value, nameof(Port)); }


        /// <summary>
        /// FireScratch.config の読み込み
        /// </summary>
        public void LoadConfig()
        {
            try
            {
                FireScratch.Core.FireScratch.LoadConfig();
                var conf = new FireScratch.Core.FireScratch.Config();
                this.ApiKey = conf.ApiKey;
                this.DatabaseURL = conf.DatabaseURL;
                this.DatabasePath = conf.DatabasePath;
                this.Email = conf.Email;
                this.Password = conf.Password;
            }
            catch { }
        }

        /// <summary>
        /// サーバーを実行
        /// </summary>
        public void Run()
        {
            var conf = new FireScratch.Core.FireScratch.Config();
            conf.ApiKey = this.ApiKey;
            conf.DatabaseURL = this.DatabaseURL;
            conf.DatabasePath = this.DatabasePath;
            conf.Email = this.Email;
            conf.Password = this.Password;
            FireScratch.Core.FireScratch.SaveConfig();
            System.Threading.Tasks.Task.Factory.StartNew(() => {
                FireScratch.Core.FireScratch.Server(this.Port);
            });
        }
    }

    public class ObservableObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Sets the property.
        /// </summary>
        /// <returns><c>true</c>, if property was set, <c>false</c> otherwise.</returns>
        /// <param name="backingStore">Backing store.</param>
        /// <param name="value">Value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <param name="onChanged">On changed.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        protected bool SetProperty<T>(
            ref T backingStore, T value,
            [CallerMemberName]string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the property changed event.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        protected void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
