﻿using System;
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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace LocalChat
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			SendButton.Click += Send_Click;
			new Thread(new ThreadStart(Receiver)).Start();
		}
		
		public void ThreadSend(object Message)
		{
			try
			{
				var Msg = (string[])Message;
				var MessageText = Msg[0];
				var ip = Msg[1];

				var EndPoint = new IPEndPoint(IPAddress.Parse(ip), 7000);
				var Connector = new Socket(EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				Connector.Connect(EndPoint);
				var SendBytes = Encoding.Default.GetBytes(MessageText);
				Connector.Send(SendBytes);
				Connector.Close();
				SendMsg("Sand: " + MessageText, ChatBox);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}
		public void SendMsg(string Text, RichTextBox Rtb)
		{
			Action action = () =>
			{
				var tr = new TextRange(Rtb.Document.ContentStart, Rtb.Document.ContentEnd);
				tr.Text += Text;
			};
			Dispatcher.Invoke(action);
		}

		private void Send_Click(object sender, RoutedEventArgs e)
		{
			new Thread(new ParameterizedThreadStart(ThreadSend)).Start(new string[] { Message.Text, IP.Text });
			Message.Text = "";
		}

		protected void Receiver()
		{
			var Listen = new TcpListener(7000);
			Listen.Start();
			Socket ReceiveSocket;
			while (true)
			{
				try
				{
					ReceiveSocket = Listen.AcceptSocket();
					var Receive = new Byte[256];
					using (MemoryStream MessageR = new MemoryStream())
					{
						int ReceivedBytes;
						do
						{
							ReceivedBytes = ReceiveSocket.Receive(Receive, Receive.Length, 0);
							MessageR.Write(Receive, 0, ReceivedBytes);
						} while (ReceiveSocket.Available > 0);
						SendMsg("Received: " + Encoding.Default.GetString(MessageR.ToArray()), ChatBox);
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);
				}
			}
		}
	}
}
