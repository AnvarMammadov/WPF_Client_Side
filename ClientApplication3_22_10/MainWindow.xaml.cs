using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClientApplication3_22_10
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Socket ClientSocket = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);

        private const int PORT = 27001;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            ConnectToServer();
            RequestLoop();
        }

        private void RequestLoop()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    ReceiveResponse();
                }
            });
        }

        private void ReceiveResponse()
        {
            var buffer = new byte[100000];
            int received = ClientSocket.Receive(buffer, SocketFlags.None);
            if (received == 0) return;
            var data = new byte[received];
            Array.Copy(buffer, data, received);
            string text = Encoding.ASCII.GetString(data);

            App.Current.Dispatcher.Invoke(() =>
            {
                responseTxt.Text = text;
            });
        }

        private void ConnectToServer()
        {
            while (!ClientSocket.Connected)
            {
                try
                {
                    ClientSocket.Connect(IPAddress.Parse("192.168.56.1"), PORT);
                }
                catch (Exception)
                {
                }
            }

            MessageBox.Show("Connected");

            var buffer = new byte[5000];
            int received = ClientSocket.Receive(buffer, SocketFlags.None);
            if (received == 0) return;

            var data = new byte[received];
            Array.Copy(buffer, data, received);

            string text = Encoding.ASCII.GetString(data);

            App.Current.Dispatcher.Invoke(() =>
            {
                ParseForView(text);
            });
        }

        private void ParseForView(string text)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var commands = text.Split('\n');
                var result = commands.ToList();
                result.Remove("");

                foreach (var item in result)
                {
                    Button button = new Button();
                    button.FontSize = 22;
                    button.Margin = new Thickness(0, 10, 0, 0);
                    button.Content = item;
                    button.Click += Button_Click;
                    commandsStackPanel.Children.Add(button);
                }
            });
        }
        TextBox textbox = new TextBox();
        public string SelectedCommand { get; set; }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button bt)
            {
                var content = bt.Content.ToString();
                var result = content.Remove(content.Length - 1, 1);
                SelectedCommand = result;

                var splitResult = result.Split('\\');
                if (splitResult.Length > 2 || splitResult.Length == 2 && result.Contains("json"))
                {
                    textbox.Width = 800;
                    textbox.Height = 60;
                    textbox.Text = "*" + splitResult.Last();
                    textbox.FontSize = 22;

                    if (paramsStackPanel.Children.Count > 3)
                    {
                        paramsStackPanel.Children.RemoveAt(3);
                        paramsStackPanel.Children.RemoveAt(3);
                    }

                    paramsStackPanel.Children.Add(textbox);

                    Button button = new Button();
                    button.FontSize = 22;
                    button.Margin = new Thickness(0, 10, 0, 0);
                    button.Content = "Execute";
                    button.Click += ExecuteClick;
                    paramsStackPanel.Children.Add(button);
                }
                else
                {
                    if (paramsStackPanel.Children.Count > 3)
                    {
                        paramsStackPanel.Children.RemoveAt(3);
                        paramsStackPanel.Children.RemoveAt(3);
                    }
                    SendString(result);
                }


            }
        }

        private void ExecuteClick(object sender, RoutedEventArgs e)
        {
            var result = SelectedCommand.Split('\\');
            var resultText = result[0] + "\\" + result[1] + '\\' + textbox.Text;

            if (SelectedCommand.Contains("json"))
            {
                var method = result[1].Split(' ')[0];
                resultText = result[0] +'\\'+method+ ' ' + textbox.Text;
            }
            SendString(resultText);
        }

        private void SendString(string result)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(result);
            ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        private void sendRequest_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
