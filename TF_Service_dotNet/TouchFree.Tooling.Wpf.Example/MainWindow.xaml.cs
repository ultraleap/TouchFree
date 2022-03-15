using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Tooling;

namespace TouchFree.Tooling.Wpf.Example
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var connectionManager = new ConnectionManager();

            MessageReceiver.TransmitInputAction += TransmitInputAction;

            StartCheckingQueues();
        }

        private async Task StartCheckingQueues()
        {
            while (true)
            {
                await Task.Delay(1);
                ConnectionManager.messageReceiver.CheckQueues();
            }
        }

        private void TopLeftButton_Clicked(object sender, RoutedEventArgs e)
        {
            this.ATextBox.Text = "Top Left Button Clicked";
        }

        private void BottomRightButton_Clicked(object sender, RoutedEventArgs e)
        {
            this.ATextBox.Text = "Bottom Right Button Clicked";
        }

        private MethodInfo? ButtonOnClick = typeof(Button).GetMethod("OnClick", BindingFlags.NonPublic | BindingFlags.Instance);

        private IInputElement? LastElementDownActionOn;

        private void TransmitInputAction(ClientInputAction _inputData)
        {
            double xPosition = _inputData.CursorPosition.x;
            double yPosition = SystemParameters.PrimaryScreenHeight - _inputData.CursorPosition.y;

            if (this.WindowState != WindowState.Maximized)
            {
                xPosition -= this.Left;
                yPosition -= this.Top;
            }

            switch (_inputData.InputType)
            {
                case InputType.CANCEL:
                case InputType.MOVE:
                    break;

                case InputType.DOWN:
                    Point downPoint2Window = this.PointFromScreen(new Point(xPosition, yPosition));

                    LastElementDownActionOn = this.InputHitTest(downPoint2Window);
                    break;

                case InputType.UP:
                    Point upPoint2Window = this.PointFromScreen(new Point(xPosition, yPosition));

                    IInputElement upElement = this.InputHitTest(upPoint2Window);
                    if (LastElementDownActionOn != null && LastElementDownActionOn == upElement)
                    {
                        if (upElement is Button)
                        {
                            ButtonOnClick?.Invoke(((Button)upElement), null);
                        }
                        else if (upElement is Border)
                        {
                            var parentButton = FindParent<Button>((Border)upElement);
                            if (parentButton != null)
                            {
                                ButtonOnClick?.Invoke(parentButton, null);
                            }
                        }
                    }
                    break;
            }

            var cursorSize = (1 - _inputData.ProgressToClick) * 20;

            this.PositionRectangle.Width = cursorSize;
            this.PositionRectangle.Height = cursorSize;

            this.PositionRectangle.Margin = new Thickness(xPosition - (cursorSize/2), yPosition - (cursorSize/2), 0, 0);
        }

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
            {
                return null;
            }
            else if (parentObject is T parent)
            {
                return parent;
            }
            else
            {
                return FindParent<T>(parentObject);
            }
        }

    }
}
