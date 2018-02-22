using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Hiale.Podbook.httpServer
{
	public class CustomHttpServer
	{
		public class PortEventArgs : EventArgs
		{
			public int Port { get; set; }
		}

		public event EventHandler<PortEventArgs> PortObtained;

		public int Port { get; set; }

		private TcpListener _listener;
		private readonly CustomHttpProcessor _processor;
		private bool _isActive;

		public CustomHttpServer(int port, IEnumerable<CustomRoute> routes)
		{
			Port = port;
			_processor = new CustomHttpProcessor();
			foreach (var route in routes)
				_processor.AddRoute(route);
		}

		public void Listen()
		{
			_isActive = true;
			_listener = new TcpListener(IPAddress.Any, Port);
			_listener.Start();
			if (PortObtained != null)
			{
			    if (_listener?.LocalEndpoint is IPEndPoint endpoint)
					PortObtained(this, new PortEventArgs {Port = endpoint.Port});
			}
			while (_isActive)
			{
				TcpClient client = null;
				try
				{
					client = _listener.AcceptTcpClient();
				}
				catch (SocketException se)
				{
					if (se.SocketErrorCode == SocketError.Interrupted)
						return;
				}
				var thread = new Thread(() =>
				{
					try
					{
						_processor.HandleClient(client);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex);
					}
				}) {IsBackground = true};
				thread.Start();
				Thread.Sleep(1);
			}
		}

		public void Stop()
		{
			_isActive = false;
			_listener.Stop();
		}

	    public void StartListening()
	    {
	        Task.Factory.StartNew(() =>
	        {
	            var thread = new Thread(delegate ()
	            {
	                while (true)
	                {
	                    try
	                    {
	                        Listen();
	                        break;
	                    }
	                    catch (SocketException)
	                    {
	                        if (Port == 0)
	                            throw;
	                        Port = 0; //try again with a random port
	                    }
	                }
	            });
	            thread.Start();
	        });
	    }
    }
}
