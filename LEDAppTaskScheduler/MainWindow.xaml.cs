using System;
using System.Collections;
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
using System.Windows.Threading;

namespace LEDAppTaskScheduler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    public class DefaultTaskSet : IEnumerable<CustomTask>
    {
        private IEnumerable<CustomTask> taskList;

        public DefaultTaskSet()
        {
            this.taskList = new List<CustomTask>();
        }

        public DefaultTaskSet(IEnumerable<CustomTask> taskList)
        {
            this.taskList = taskList;
        }

        public CustomTask this[int i]
        {
            get
            {
                return taskList.ElementAt(i);
            }
            set
            {
                if (value.GetType() != typeof(CustomTask))
                    throw new ArgumentException($"Argument should be type of {nameof(CustomTask)}");
                taskList.Append(value);
            }
        }

        public IEnumerator<CustomTask> GetEnumerator()
        {
            return taskList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class CustomTask
    {
        private readonly Rectangle rectangle;
        private readonly SolidColorBrush brush;

        public CustomTask(Rectangle rectangle, SolidColorBrush brush)
        {
            this.rectangle = rectangle;
            this.brush = brush;
        }

        public void ColorTheDiod() => rectangle.Dispatcher.BeginInvoke(colorAction, rectangle, brush);

        private readonly Action<Rectangle, SolidColorBrush> colorAction = (Rectangle rect, SolidColorBrush brush) =>
        {
            if (rect.Fill == null) rect.Fill = brush;
            else rect.Fill = null;
        };
    }

    public class CustomOS
    {
        private object taskQueueLocker = new object();

        private IEnumerable<CustomTask> taskQueue;
        private DefaultTaskSet taskPool;
        private DispatcherTimer osTimer;
        private DispatcherTimer tsTimer; // task scheduler timer
        private Random randomizer;
        private CustomTaskScheduler taskScheduler;

        public CustomOS(DefaultTaskSet taskPool)
        {
            this.taskPool = taskPool;
            taskQueue = new List<CustomTask>();
            randomizer = new Random();
            taskScheduler = new CustomTaskScheduler();

            osTimer = new DispatcherTimer();
            osTimer.Interval = TimeSpan.FromSeconds(1d);
            osTimer.Tick += OSTimerEventHandler;
            osTimer.Start();

            tsTimer = new DispatcherTimer();
            tsTimer.Interval = TimeSpan.FromSeconds(1d);
            osTimer.Tick += (s, e) =>
            {
                // get value from taskScheduler considering priority
                // take first element with this value from taskQueue
                // perform it's action (LED)
                var nextTaskNumber = taskScheduler.GetNextTaskNumber();
                
                lock(taskQueueLocker)
                {
                    //override Equals
                    taskQueue.FirstOrDefault(p => p.Equals(taskPool[nextTaskNumber]))
                }
            };
        }

        private class CustomTaskScheduler
        {
            private const int firstQuantity = 65;
            private const int secondQuantity = 35;
            private const int thirdQuantity = (int)11.5;

            private List<int> priorityList;

            private Random randomizer;

            public CustomTaskScheduler()
            {
                this.randomizer = new Random();
                this.priorityList = new List<int>();

                for (int i = 0; i < firstQuantity; i++)
                {
                    this.priorityList.Add(0);
                }
                for (int i = 0; i < secondQuantity; i++)
                {
                    this.priorityList.Add(1);
                }
                for (int i = 0; i < thirdQuantity; i++)
                {
                    this.priorityList.Add(2);
                }
            }

            public int GetNextTaskNumber()
            {
                return this.randomizer.Next(0, this.priorityList.Count);
            }
        }

        private void OSTimerEventHandler(object? sender, EventArgs e)
        {
            int randomTaskNumber = randomizer.Next(0, 3);
            
            lock (taskQueueLocker)
            {
                taskQueue.Append(this.taskPool[randomTaskNumber]);
            }
        }
    }
}
