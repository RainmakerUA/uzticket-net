using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RM.Lib.Common.Contracts.Log;
using RM.Lib.Proxy.Contracts;
using RM.Lib.Utility;
using RM.Lib.UzTicket.Model;
using RM.Lib.UzTicket.Utils;
using RM.UzTicket.Lib.Exceptions;

namespace RM.Lib.UzTicket
{
	internal sealed class UzService : IDisposable
	{
		private const string _baseUrlDefault = "https://booking.uz.gov.ua/en"; // strict without trailing '/'!
		private const string _sessionIdKeyDefault = "_gv_sessid";
		private const string _dataKey = "data";
		private const int _requestTimeout = 10;

		private readonly string _baseUrl;
		private readonly string _sessionIdKey;
		private readonly IProxyProvider _proxyProvider;
		private readonly ILog _logger;

		private HttpClientHandler _httpHandler;
		private HttpClient _httpClient;

		private string _userAgent;
		private bool _isDisposed;

		public UzService(string baseUrl = null, string sessionIdKey = null, IProxyProvider proxyProvider = null, ILog logger = null)
		{
			_baseUrl = baseUrl.OrDefault(_baseUrlDefault);
			_sessionIdKey = sessionIdKey.OrDefault(_sessionIdKeyDefault);
			//_proxyProvider = proxyProvider;
			_logger = logger;
		}

		public void Dispose()
		{
			if (!_isDisposed)
			{
				_httpClient.Dispose();
				_httpClient = null;

				_httpHandler.Dispose();
				_httpHandler = null;

				_isDisposed = true;
			}
		}

		public string GetSessionId()
		{
			var value = _httpHandler.CookieContainer.GetCookies(new Uri(_baseUrl))[_sessionIdKey]?.Value;
			return value != null ? _sessionIdKey + "=" + value : null;
		}

		public async Task<Station[]> SearchStationAsync(string name)
		{
			var path = $"train_search/station/?term={name}";
			var json = await GetJson(path, HttpMethod.Get);
			return json.Deserialize<Station[]>();
		}

		public async Task<Station> FetchFirstStationAsync(string name)
		{
			var stations = await SearchStationAsync(name);
			return stations.FirstOrDefault();
		}

		public async Task<Train[]> ListTrainsAsync(DateTime date, Station source, Station destination)
		{
			var data = new Dictionary<string, string>
							{
								["from"] = source.ID.ToString(),
								["to"] = destination.ID.ToString(),
								["date"] = date.ToSortableDateString(),
								["time"] = "00:00",
								["another_ec"] = "0",
								["get_tpl"] = "0"
							};

			var result = await GetJson("train_search/", data: data);

			return result["list"].Deserialize<Train[]>();
		}

		public async Task<Train> FetchTrainAsync(DateTime date, Station source, Station destination, string trainNumber)
		{
			try
			{
				var trains = await ListTrainsAsync(date, source, destination);
				return trains.GetByNumber(trainNumber);
			}
			catch (ResponseException)
			{
				return null;
			}
		}

		public async Task<Coach[]> ListCoachesAsync(Train train, CoachType coachType)
		{
			var data = new IPersistable[] { train, coachType }.ToRequestDictionary(
																	new Dictionary<string, string>
																		{
																			["another_ec"] = "0"
																		});
			var result = await GetJson("train_wagons/", data: data);

			return result["wagons"].Deserialize<Coach[]>();
		}

		public async Task<IReadOnlyDictionary<string, int[]>> ListSeatsAsync(Train train, Coach coach)
		{
			JToken result;

			try
			{
				result = await GetJson("train_wagon/", data: new IPersistable[] { train, coach }.ToRequestDictionary());
			}
			catch (ResponseException)
			{
				return null;
			}
			
			return ParseSeats(result["places"]);
		}

		public async Task<dynamic> BookSeatAsync(Train train, Coach coach, Seat seat, Passenger passenger)
		{
			var data = new Dictionary<string, string>
							{
								["roundtrip"] = "0"
							};
			var place = new IPersistable[] { train, coach, seat, passenger }
							.ToRequestDictionary(new Dictionary<string, string> { ["ord"] = "0", ["reserve"] = "0" });
					
			foreach (var key in place.Keys)
			{
				data[$"places[0][{key}]"] = place[key];
			}

			var result = await GetJson("cart/add/", data: data);

			return result as JObject;
		}

		/* Alternative booking request w/o places
		places[0][ord]: 0
		places[0][from]: 2000002
		places[0][to]: 2044001
		places[0][train]: 002Щ
		places[0][date]: 2018-09-09
		places[0][wagon_num]: 6
		places[0][wagon_class]: 
		places[0][wagon_type]: Л
		places[0][wagon_railway]: 17
		places[0][charline]: 
		places[0][firstname]: Джон
		places[0][lastname]: Боббик
		places[0][bedding]: 0
		places[0][child]: 
		places[0][student]: 
		places[0][reserve]: 0
		wishes[0][count_lower]: 
		wishes[0][places_from]: 
		wishes[0][places_to]: 
		wishes[0][one_coupe]: 0
		wishes[0][no_side]: 1
		wishes[0][train]: 002Щ
		wishes[0][wagon]: 6
		*/

		public async Task<Route> GetSelectedTrainRoute(Train train)
		{
			var routes = await GetTrainRoutes(new RouteData { Train = train });
			return routes.FirstOrDefault();
		}

		public async Task<Route[]> GetTrainRoutes(params RouteData[] routes)
		{
			var data = new Dictionary<string, string>();

			for (int i = 0; i < routes.Length; i++)
			{
				var routeData = routes[i].ToDictionary();

				foreach (var key in routeData.Keys)
				{
					data.Add($"routes[{i}][{key}]", routeData[key]);
				}
			}

			var result = await GetJson("route/", data: data);

			return result["routes"].Deserialize<Route[]>();
		}

		private IDictionary<string, string> GetDefaultHeaders()
		{
			return new Dictionary<string, string>
						{
							["User-Agent"] = _userAgent
						};
		}

		private async Task<string> GetString(string path, HttpMethod method, IDictionary<string, string> headers,
												IDictionary<string, string> data)
		{
			var url = GetUrl(path);
			var req = new HttpRequestMessage(method ?? HttpMethod.Post, url);

			if (_userAgent == null)
			{
				//using (var locker = AsyncLock.Lock(_userAgent, 1000))
				//{
					//if (locker.IsCaptured && _userAgent == null)
					//{
						await InitializeHttpClient();
					//}
				//}
			}

			if (data != null && data.Count > 0)
			{
				req.Content = new FormUrlEncodedContent(data);
			}

			if (headers == null)
			{
				headers = GetDefaultHeaders();
			}

			SetHeaders(req.Headers, headers);

			_logger?.Debug($"Fetching: {url}");
			_logger?.Debug($"Headers: {String.Join("\n", headers.Select(kv => $"{kv.Key}: {kv.Value}"))}");
			_logger?.Debug($"Cookies: {String.Join("\n", _httpHandler.CookieContainer.GetCookies(new Uri(_baseUrl)).Cast<Cookie>().Select(c => c.ToString()))}");

			var resp = await _httpClient.SendAsync(req);

			if (!resp.IsSuccessStatusCode)
			{
				if (resp.StatusCode == HttpStatusCode.BadRequest)
				{
					throw new BadRequestException((int)resp.StatusCode, null);
				}

				throw new HttpException((int)resp.StatusCode, null);
			}

			return await resp.Content.ReadAsStringAsync();
		}

		private async Task<JToken> GetJson(string path, HttpMethod method = null, IDictionary<string, string> headers = null,
												IDictionary<string, string> data = null)
		{
			var str = await GetString(path, method, headers, data);

			JToken json;

			try
			{
				json = JToken.Parse(str);
			}
			catch (FormatException)
			{
				throw new ResponseException("Invalid JSON response!", str);
			}

			var jObj = json as JObject;

			if (jObj != null && jObj.Value<bool?>("error").GetValueOrDefault())
			{
				string message;
				var value = jObj[_dataKey];


				if (value is JObject valueObj && valueObj.TryGetValue("errors", out var errorsArray))
				{
					message = errorsArray is IEnumerable<JToken> errors ? String.Join(" | ", errors.Select(jv => (string)jv)) : null;
				}
				else
				{
					message = (string)value;
				}

				throw new ResponseException(message, str);
			}

			return jObj != null && jObj.ContainsKey(_dataKey) ? json[_dataKey] : json;
		}

		private async Task InitializeHttpClient()
		{
			_userAgent = UserAgentSelector.GetRandomAgent();
			_httpHandler = new HttpClientHandler();
			_httpClient = new HttpClient(_httpHandler)
								{
									BaseAddress = new Uri(_baseUrl),
									Timeout = TimeSpan.FromSeconds(_requestTimeout)
								};

			if (_proxyProvider != null)
			{
				var proxyUrl = await _proxyProvider.GetProxyAsync();
				_httpHandler.Proxy = new WebProxy(new Uri(proxyUrl));
			}
		}

		/*

		private async Task<string> GetTokenAsync()
		{
			if (IsTokenOutdated())
			{
				using (new AsyncLock(_tokenLock))
				{
					if (IsTokenOutdated())
					{
						InitializeHttpClient();

						var resp = await GetString("", null, new Dictionary<string, string> { ["User-Agent"] = _userAgent }, null);
						_token = TokenParser.ParseGvToken(resp);

						if (String.IsNullOrEmpty(_token))
						{
							throw new TokenException(resp);
						}

						_tokenTime = DateTimeExtensions.GetUnixTime();
					}
				}
			}

			return _token;
		}

		private bool IsTokenOutdated()
		{
			var unixNow = DateTimeExtensions.GetUnixTime();
			return unixNow - _tokenTime > _tokenMaxAge;
		}

		*/

		private string GetUrl(string relPath)
		{
			return $"{_baseUrl}/{relPath}";
		}

		private static void SetHeaders(HttpRequestHeaders headers, IDictionary<string, string> dict)
		{
			foreach (var key in dict.Keys)
			{
				headers.Add(key, dict[key]);
			}
		}

		private static IReadOnlyDictionary<string, int[]> ParseSeats(JToken jToken)
		{
			//return values?.ToDictionary(kv => kv.Key, kv => (kv.Value as IEnumerable<JsonValue>)?.Select(jv => jv.ReadAs<int>()).ToArray());
			return new ReadOnlyDictionary<string, int[]>(jToken.Deserialize<IDictionary<string, int[]>>());
		}
	}
}
