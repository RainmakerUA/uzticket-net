using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using RM.Lib.Proxy.Contracts;
using RM.Lib.Proxy.Utils;
using RM.Lib.Utility;
using RM.UzTicket.Data.Contracts;
using RM.UzTicket.Settings.Contracts;

namespace RM.Lib.Proxy
{
	internal class ProxyProvider : IProxyProvider
	{
		[DataContract]
		private struct Proxy
		{
			[DataMember]
			public readonly string IpAddress;

			[DataMember]
			public readonly int Port;

			public Proxy(string ipAddress, int port)
			{
				IpAddress = ipAddress;
				Port = port;
			}

			public override string ToString()
			{
				return ToString("http");
			}

			public string ToString(string schema)
			{
				return $"{schema}://{IpAddress}:{Port}/";
			}
		}

		private const int _maxRetriesDefault = 10;
		private const int _maxProxies = 50;

		private const string _managerKey = "proxy-manager";
		private const string _proxyKey = "proxies";
		private const string _indexKey = "index";

		private static readonly IDictionary<string, string> _proxyRequestData = new Dictionary<string, string>
																					{
																						["xpp"] = "2",
																						["xf1"] = "1",
																						["xf2"] = "1",
																						["xf4"] = "0",
																						["xf5"] = "1"
																					};

		private readonly ISettingsProvider _settingsProvider;
		private readonly IPersistenceProvider _dataProvider;
		private readonly List<Proxy> _proxies = new List<Proxy>(_maxProxies);
		private IProxySettings _settings;
		private int _currentIndex;

		public ProxyProvider(ISettingsProvider provider, IPersistenceProvider dataProvider = null)
		{
			_settingsProvider = provider;
			_dataProvider = dataProvider;
		}

		public async Task<string> GetProxyAsync(Func<string, Task<bool>> proxyChecker = null)
		{
			var numTries = 0;

			while (numTries++ < _maxRetriesDefault
					&& (_currentIndex >= _proxies.Count || !await IsProxyValidAsync(_proxies[_currentIndex], proxyChecker)))
			{
				if (_proxies.Count == 0)
				{
					await LoadProxiesAsync();
				}
				else
				{
					_currentIndex++;
				}
			}

			if (_proxies.Count == 0)
			{
				return null;
			}

			await SetDbProxies(_currentIndex, null);

			return _proxies[_currentIndex].ToString();
		}

		private async Task LoadProxiesAsync()
		{
			using (AsyncLock.Lock(_proxies))
			{
				_proxies.Clear();

				var (proxies, index) = await GetDbProxies();

				if (proxies != null && proxies.Length != 0 && index >= 0 && index < proxies.Length)
				{
					_currentIndex = index;
					_proxies.AddRange(proxies);
				}
				else
				{
					_settings = _settingsProvider.GetSettings().Proxy;

					(proxies, index) = await GetProxies(_settings);

					if (proxies != null)
					{
						_proxies.AddRange(proxies);
						_currentIndex = index;
						await SetDbProxies(_currentIndex, _proxies.ToArray());
					}
				}
			}
		}

		private static async Task<(Proxy[], int)> GetProxies(IProxySettings settings)
		{
			var client = new HttpClient();
			var req = new HttpRequestMessage(HttpMethod.Post, settings.SourceUrl) { Content = new FormUrlEncodedContent(_proxyRequestData) };

			var resp = await client.SendAsync(req);

			if (resp.IsSuccessStatusCode)
			{
				var htmlDoc = new HtmlDocument();
				htmlDoc.Load(await resp.Content.ReadAsStreamAsync());

				var scriptNode = htmlDoc.DocumentNode.SelectSingleNode(settings.ScriptPath);
				var unpacked = ScriptUnpacker.Unpack(scriptNode.InnerText);
				var calc = new Calculator(unpacked);

				var nodes = htmlDoc.DocumentNode.SelectNodes(settings.ProxyPath);
				return (nodes.Select(n => ParseProxy(n.InnerText, calc, settings)).Take(_maxProxies).ToArray(), 0);
			}

			return default;
		}

		private async Task<(Proxy[], int)> GetDbProxies()
		{
			if (_dataProvider == null)
			{
				return (null, 0);
			}

			using (var client = await _dataProvider.GetClientAsync(_managerKey))
			{
				var index = await client.GetValueAsync<int?>(_indexKey);
				var proxies = await client.GetListAsync<Proxy>(_proxyKey);
				return (proxies, index ?? 0);
			}
		}

		private async Task SetDbProxies(int index, Proxy[] proxies)
		{
			if (_dataProvider != null)
			{
				using (var client = await _dataProvider.GetClientAsync(_managerKey))
				{
					var indexTask = client.SetValueAsync(_indexKey, index);

					if (proxies == null)
					{
						await indexTask;
					}
					else
					{
						await Task.WhenAll(indexTask, client.SetListAsync(_proxyKey, proxies));
					}
				}
			}
		}

		private static Proxy ParseProxy(string nodeText, Calculator calc, IProxySettings settings)
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

		private static Task<bool> IsProxyValidAsync(Proxy proxy, Func<string, Task<bool>> validatorAsync)
		{
			return DefaultProxyValidatorAsync(proxy).Then(
											result => result
													? validatorAsync == null ? Task.FromResult(true) : validatorAsync(proxy.ToString())
													: Task.FromResult(false),
											e => Task.FromResult(false)
										);
		}

		private static async Task<bool> DefaultProxyValidatorAsync(Proxy proxy)
		{
			var isSuccess = false;
			var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			try
			{
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 200);
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);

				var ip = IPAddress.Parse(proxy.IpAddress);
				var ipep = new IPEndPoint(ip, proxy.Port);

				await socket.ConnectAsync(ipep);

				if (socket.Connected)
				{
					isSuccess = true;
				}
			}
			catch (Exception)
			{
				isSuccess = false;
			}
			finally
			{
				socket.Close();
			}

			return isSuccess;
		}
	}
}
