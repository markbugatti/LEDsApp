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

            var initialTaskSet = new TaskPool(new List<CustomTask>()
                {
                    new CustomTask(this.RedLed, Brushes.DarkRed),
                    new CustomTask(this.YellowLed, Brushes.Orange),
                    new CustomTask(this.GreenLed, Brushes.DarkGreen)
                }
            );
            new CustomOS(initialTaskSet);
        }
    }

    public class TaskPool : IEnumerable<CustomTask>
    {
        private IEnumerable<CustomTask> taskList;

        public TaskPool(IEnumerable<CustomTask> taskList)
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

        public void ColorTheDiod() => Task.Run(() => rectangle.Dispatcher.BeginInvoke(colorAction, rectangle, brush, uncolorAction));

        private readonly Action<Rectangle, SolidColorBrush, Action<Rectangle>> colorAction = (Rectangle rect, SolidColorBrush brush, Action<Rectangle> uncolorAction) =>
        {
            rect.Fill = brush;
            Task.Delay(400).ContinueWith(_ => rect.Dispatcher.BeginInvoke(uncolorAction, rect));
        };

        private readonly Action<Rectangle> uncolorAction = (Rectangle rect) =>
        {
            rect.Fill = null;
        };
    }

    public class CustomOS
    {
        private DispatcherTimer tsTimer;
        private CustomTaskScheduler taskScheduler;

        public CustomOS(TaskPool taskPool)
        {
            taskScheduler = new CustomTaskScheduler();

            tsTimer = new DispatcherTimer();
            tsTimer.Interval = TimeSpan.FromSeconds(1d);
            tsTimer.Tick += (s, e) =>
            {
                var nextTaskNumber = taskScheduler.GetNextTaskNumber();
                var taskFromPool = taskPool[nextTaskNumber];

                taskFromPool.ColorTheDiod();
            };
            tsTimer.Start();
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
                var taskIndex = this.randomizer.Next(0, this.priorityList.Count);
                return priorityList[taskIndex];
            }
        }
    }
}
