﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using RM.Lib.Proxy.Contracts;
using RM.Lib.Proxy.Utils;
using RM.Lib.Utility;
using RM.UzTicket.Settings.Contracts;

namespace RM.Lib.Proxy
{
	internal class ProxyProvider : IProxyProvider
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
																					["xpp"] = "1",
																					["xf1"] = "1",
																					["xf2"] = "1",
																					["xf4"] = "0",
																					["xf5"] = "1"
																				};

		private readonly ISettingsProvider _settingsProvider;
		private readonly List<Proxy> _proxies = new List<Proxy>(_maxProxies);
		private ISettings _settings;
		private int _currentIndex;

		public ProxyProvider(ISettingsProvider provider)
		{
			_settingsProvider = provider;
		}

		public async Task<string> GetProxyAsync()
		{
			while (_currentIndex >= _proxies.Count || !await IsProxyValidAsync(_proxies[_currentIndex]))
			{
				if (_proxies.Count == 0)
				{
					await LoadProxiesAsync();
					_currentIndex = 0;
				}
				else

				{
					_currentIndex++;
				}
			}

			return _proxies[_currentIndex].ToString();
		}

		private async Task LoadProxiesAsync()
		{
			using (AsyncLock.Lock(_proxies))
			{
				_proxies.Clear();

				_settings = _settingsProvider.GetSettings();

				var client = new HttpClient();
				var req = new HttpRequestMessage(HttpMethod.Post, _settings.ProxySource) { Content = new FormUrlEncodedContent(_proxyRequestData) };

				var resp = await client.SendAsync(req);

				if (resp.IsSuccessStatusCode)
				{
					var htmlDoc = new HtmlDocument();
					htmlDoc.Load(await resp.Content.ReadAsStreamAsync());

					var scriptNode = htmlDoc.DocumentNode.SelectSingleNode(_settings.ProxyScriptPath);
					var unpacked = ScriptUnpacker.Unpack(scriptNode.InnerText);
					var calc = new Calculator(unpacked);

					var nodes = htmlDoc.DocumentNode.SelectNodes(_settings.ProxyPath);
					_proxies.AddRange(nodes.Select(n => ParseProxy(n.InnerText, calc, _settings)).Take(_maxProxies));
				}
			}
		}

		private static Proxy ParseProxy(string nodeText, Calculator calc, ISettings settings)
		{
			var re = new Regex(settings.ProxyRegex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
			var match = re.Match(nodeText);

			if (match.Success)
			{
				var portParts = match.Groups["port"].Value.Split("+".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
				portParts = Array.ConvertAll(portParts, expr => calc.CalcValue(expr).ToString());
				return new Proxy(match.Groups["ip"].Value, Int32.Parse(String.Join(String.Empty, portParts)));
			}

			throw new ArgumentException($"Cannot parse node text '{nodeText}'!");
		}

		private static async Task<bool> IsProxyValidAsync(Proxy proxy)
		{
			var isSuccess = false;
			try
			{
				var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 200);
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);

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