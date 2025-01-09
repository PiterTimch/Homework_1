using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Socket _server;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void connectBT_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateIPAddress(this.serverIPTB.Text) || !ValidatePort(this.serverPortTB.Text, out int port))
            {
                MessageBox.Show("Invalid IP address or port", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var ip = IPAddress.Parse(this.serverIPTB.Text);
                var serverEndPoint = new IPEndPoint(ip, port);

                _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                _server.Connect(serverEndPoint);
                this.sendBT.IsEnabled = true;
                this.connectBT.IsEnabled = false;

                MessageBox.Show("Successful connection", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void sendBT_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.messageToServerTB.Text))
            {
                MessageBox.Show("Message cannot be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var data = Encoding.Unicode.GetBytes(this.messageToServerTB.Text);
                await _server.SendAsync(data);

                data = new byte[1024];
                int bytes = 0;

                StringBuilder responseBuilder = new StringBuilder();
                do
                {
                    bytes = _server.Receive(data);
                    responseBuilder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                } while (_server.Available > 0);

                this.serverMessageTB.Text += responseBuilder.ToString() + '\n';
                this.sendBT.IsEnabled = false;
                this.connectBT.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateIPAddress(string ipAddress)
        {
            return IPAddress.TryParse(ipAddress, out _);
        }

        private bool ValidatePort(string portText, out int port)
        {
            return int.TryParse(portText, out port) && port > 0 && port <= 65535; //чи є якась константа для цього?
        }
    }
}
