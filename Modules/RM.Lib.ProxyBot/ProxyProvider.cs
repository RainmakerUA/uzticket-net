using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RM.Lib.Common.Contracts.Log;
using RM.Lib.Proxy.Contracts;
using RM.Lib.Utility;
using RM.UzTicket.Settings.Contracts;

namespace RM.Lib.ProxyBot
{
	public class ProxyProvider : IProxyProvider
	{
		// http://spys.me/proxy.txt Full Regex
		// ^(?<ip>(?:\d{1,3}\.){3}\d{1,3}):(?<port>\d{2,5})\s+(?<country>UA)-(?<anon>\w)(?:-(?<ssl>\w))?\s+(?<google>\+|-)\s+$

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
				return ToString("http");
			}

			public string ToString(string schema)
			{
				return $"{schema}://{IpAddress}:{Port}/";
			}
		}

		private const int _maxRetriesDefault = 10;

		private readonly ILog _log;
		private readonly ISettingsProvider _settingsProvider;
		private readonly List<Proxy> _proxies;
		private IProxySettings _settings;
		private Regex _proxyRE;
		private int _currentIndex;

		public ProxyProvider(ISettingsProvider settingsProvider)
		{
			_log = LogFactory.GetLog(GetType());
			_proxies = new List<Proxy>(32);
			_settingsProvider = settingsProvider;
		}

		public async Task<string> GetProxyAsync(Func<string, Task<bool>> proxyCheckerAsync)
		{
			var numTries = 0;

			while (numTries++ < _maxRetriesDefault
			       && (_currentIndex >= _proxies.Count || !await IsProxyValidAsync(_proxies[_currentIndex], proxyCheckerAsync)))
			{
				if (_proxies == null || _proxies.Count == 0)
				{
					_settings = _settingsProvider.GetSettings().Proxy;
					_proxyRE = new Regex(_settings.ProxyRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
					await LoadProxiesAsync();
				}
				else
				{
					_currentIndex++;
				}
			}

			return _proxies.Count == 0 ? null : _proxies[_currentIndex].ToString();
		}

		private async Task LoadProxiesAsync()
		{
			using (AsyncLock.Lock(_proxies))
			{
				var webClient = new WebClient();
				var proxyUri = new Uri(_settings.SourceUrl);
				_proxies.Clear();

				try
				{
					var proxyList = await webClient.DownloadStringTaskAsync(proxyUri);
					var matches = _proxyRE.Matches(proxyList);
					_proxies.AddRange(matches.Cast<Match>().Where(m => m.Success).Select(
																m => new Proxy(m.Groups["ip"].Value, Int32.Parse(m.Groups["port"].Value))
															));
				}
				catch (Exception e)
				{
					_log.Warning("Error downloading proxy list: {0}", e, proxyUri);
				}
			}
		}

		private static Task<bool> IsProxyValidAsync(Proxy proxy, Func<string, Task<bool>> validatorAsync)
		{
			return DefaultProxyValidatorAsync(proxy).Then(
					t => t.Result
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
