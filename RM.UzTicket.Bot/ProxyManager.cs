using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace RM.UzTicket.Bot
{
	internal class ProxyManager
	{
		private struct Proxy
		{
			public readonly string IpAddress;

			public readonly int Port;

			public Proxy(string ip, int port)
			{
				IpAddress = ip;
				Port = port;
			}

			public override string ToString()
			{
				return ToString("https");
			}

			public string ToString(string schema)
			{
				return $"{schema}://{IpAddress}:{Port}/";
			}
		}

		private const int _maxProxies = 50;

		private static readonly IDictionary<string, string> _proxyRequestData = new Dictionary<string, string>
																				{
																					["xpp"] = "2",
																					["xf1"] = "0",
																					["xf2"] = "1",
																					["xf4"] = "0",
																					["xf5"] = "0"
																				};

		private readonly Settings _settings;
		private readonly List<Proxy> _proxies = new List<Proxy>(_maxProxies);
		private int _currentIndex;

		public ProxyManager(Settings settings)
		{
			_settings = settings;
		}

		public async Task<string> GetProxy()
		{
			while (_currentIndex >= _proxies.Count || !await IsProxyValid(_proxies[_currentIndex]))
			{
				if (_proxies.Count == 0)
				{
					await LoadProxies();
					_currentIndex = 0;
				}
				else

				{
					_currentIndex++;
				}
			}

			return _proxies[_currentIndex].ToString();
		}

		private async Task LoadProxies()
		{
			var listLocked = false;

			try
			{
				if (Monitor.TryEnter(_proxies))
				{
					listLocked = true;

					_proxies.Clear();

					var client = new HttpClient();
					var req = new HttpRequestMessage(HttpMethod.Post, _settings.ProxySource) { Content = new FormUrlEncodedContent(_proxyRequestData) };

					var resp = await client.SendAsync(req);

					if (resp.IsSuccessStatusCode)
					{
						var htmlDoc = new HtmlDocument();
						htmlDoc.Load(await resp.Content.ReadAsStreamAsync());

						var nodes = htmlDoc.DocumentNode.SelectNodes(_settings.ProxyPath);
						_proxies.AddRange(nodes.Select(n => new Proxy(n.InnerText, 8080)).Take(_maxProxies));
					}
				}
			}
			finally
			{
				if (listLocked)
				{
					Monitor.Exit(_proxies);
				}
			}
		}

		private static async Task<bool> IsProxyValid(Proxy proxy)
		{
			var isSuccess = false;
			try
			{
				var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 200);

				var ip = IPAddress.Parse(proxy.IpAddress);
				var ipep = new IPEndPoint(ip, proxy.Port);

				await socket.ConnectAsync(ipep);

				if (socket.Connected)
				{
					isSuccess = true;
				}

				socket.Close();
			}
			catch (Exception)
			{
				isSuccess = false;
			}

			return isSuccess;
		}
	}
}
