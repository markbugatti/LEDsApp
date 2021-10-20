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

namespace LEDsApp
{
    using System.DirectoryServices.ActiveDirectory;
    using System.Security.RightsManagement;
    using System.Threading;
    using System.Windows.Threading;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource tokenSource;

        private CancellationToken token;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            this.tokenSource.Cancel();
        }

        private Action<DispatcherTimer, Rectangle, SolidColorBrush> timerAction =>
            (timer, rect, brush) =>
                {
                    timer.Tick += (sender, args) => { rect.Dispatcher.BeginInvoke(this.ledAction, timer, rect, brush); };
                    timer.Interval = new TimeSpan(0, 0, 0, 1);
                    timer.Start();
                };

        private Action<DispatcherTimer, Rectangle, SolidColorBrush> ledAction =>
            (timer, rect, brush) =>
                {
                    if (this.token.IsCancellationRequested) timer.Stop();
                    
                    if (rect.Fill == null)
                        rect.Fill = brush;
                    else
                        rect.Fill = null;
                };

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            tokenSource = new CancellationTokenSource();
            this.token = this.tokenSource.Token;

            var redTimer = new DispatcherTimer();
            var yellowTimer = new DispatcherTimer();
            var greenTimer = new DispatcherTimer();

            Task.Run(
                () => redTimer.Dispatcher.BeginInvoke(timerAction, redTimer, this.RedLed, Brushes.DarkRed),
                this.token);

            Task.Delay(770, this.token).ContinueWith(
                _ => yellowTimer.Dispatcher.BeginInvoke(timerAction, yellowTimer, this.YellowLed, Brushes.Orange),
                this.token);

            Task.Delay(1300, this.token).ContinueWith(
                _ => greenTimer.Dispatcher.BeginInvoke(timerAction, greenTimer, this.GreenLed, Brushes.DarkGreen),
                this.token);
        }
    }
}
