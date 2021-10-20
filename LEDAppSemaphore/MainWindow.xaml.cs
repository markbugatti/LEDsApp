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

namespace LEDAppSemaphore
{
    using System.Net;
    using System.Threading;
    using System.Windows.Threading;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Semaphore semaphore = new Semaphore(1, 1);

        private CancellationTokenSource tokenSource;

        private CancellationToken token;

        public MainWindow()
        {
            InitializeComponent();
        }

        private Action<DispatcherTimer, Rectangle, SolidColorBrush, int> timerAction =>
            (timer, rect, brush, ms) =>
                {
                    timer.Tick += (sender, args) => { rect.Dispatcher.BeginInvoke(this.ledAction, timer, rect, brush); };
                    timer.Interval = new TimeSpan(0, 0, 0, 0, ms);
                    timer.Start();
                };

        private Action<DispatcherTimer, Rectangle, SolidColorBrush> ledAction =>
            (timer, rect, brush) =>
                {
                    if (this.token.IsCancellationRequested) timer.Stop();

                    semaphore.WaitOne();
                    rect.Fill = brush;

                    Task.Delay(1000).ContinueWith(
                        _ =>
                            {
                                semaphore.Release();
                            });
                };

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            tokenSource = new CancellationTokenSource();
            this.token = this.tokenSource.Token;

            var redTimer = new DispatcherTimer();
            var yellowTimer = new DispatcherTimer();
            var greenTimer = new DispatcherTimer();

            Task.Run(
                () => redTimer.Dispatcher.BeginInvoke(timerAction, redTimer, this.SharedLed, Brushes.DarkRed, 1000),
                this.token);

            Task.Run(
                () => yellowTimer.Dispatcher.BeginInvoke(timerAction, yellowTimer, this.SharedLed, Brushes.Orange, 1500),
                this.token);

            Task.Run(
                () => greenTimer.Dispatcher.BeginInvoke(timerAction, greenTimer, this.SharedLed, Brushes.DarkGreen, 2000),
                this.token);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            this.tokenSource.Cancel();
        }
    }
}
