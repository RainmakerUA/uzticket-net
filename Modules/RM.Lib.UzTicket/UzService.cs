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
using RM.Lib.UzTicket.Contracts.DataContracts;
using RM.Lib.UzTicket.Model;
using RM.Lib.UzTicket.Utils;
using RM.UzTicket.Lib.Exceptions;

namespace RM.Lib.UzTicket
{
	internal sealed class UzService : IDisposable
	{
		private const string _sessionIdKeyDefault = "_gv_sessid";
		private const string _dataKey = "data";
		private const int _requestTimeout = 10;

		private readonly string _baseUrl;
		private readonly string _sessionIdKey;
		private readonly IProxyProvider _proxyProvider;
		private readonly ILog _logger;
		private readonly object _httpInitLock;

		private HttpClientHandler _httpHandler;
		private HttpClient _httpClient;
		private string _userAgent;
		private bool _isDisposed;

		public UzService(string baseUrl = null, string sessionIdKey = null, IProxyProvider proxyProvider = null, ILog logger = null)
		{
			_baseUrl = baseUrl.OrDefault("https://booking.uz.gov.ua/ru");
			_sessionIdKey = sessionIdKey.OrDefault(_sessionIdKeyDefault);
			_proxyProvider = proxyProvider;
			_logger = logger;

			_httpInitLock = new object();
		}

		public void Dispose()
		{
			if (!_isDisposed)
			{
				_httpClient?.Dispose();
				_httpClient = null;

				_httpHandler?.Dispose();
				_httpHandler = null;

				_userAgent = null;
				_isDisposed = true;
			}
		}

		public string GetSessionId()
		{
			var value = _httpHandler.CookieContainer.GetCookies(new Uri(_baseUrl))[_sessionIdKey]?.Value;
			return value != null ? _sessionIdKey + "=" + value : null;
		}

		public Task<Station[]> SearchStationAsync(string name)
		{
			var path = $"train_search/station/?term={name}";
			return GetJsonAsync<Station[]>(path, HttpMethod.Get);
		}

		public Task<Station> FetchFirstStationAsync(string name)
		{
			return SearchStationAsync(name).Then(t => t.Result.FirstOrDefault());
		}

		public Task<Train[]> ListTrainsAsync(DateTime date, Station source, Station destination)
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

			return GetJsonAsync<Train[]>("train_search/", data: data, jsonKey: "list");
		}

		public Task<Train> FetchTrainAsync(DateTime date, Station source, Station destination, string trainNumber)
		{
			return ListTrainsAsync(date, source, destination).Then(
																t => t.Result.GetByNumber(trainNumber),
																ex => ex is ResponseException ? (Train)null : throw ex
															);
		}

		public Task<Coach[]> ListCoachesAsync(Train train, CoachType coachType)
		{
			var data = new IPersistable[] { train, coachType }.ToRequestDictionary(
																	new Dictionary<string, string>
																	{
																		["another_ec"] = "0"
																	});
			return GetJsonAsync<Coach[]>("train_wagons/", data: data, jsonKey: "wagons");
		}

		public Task<IReadOnlyDictionary<string, int[]>> ListSeatsAsync(Train train, Coach coach)
		{
			return GetJsonAsync("train_wagon/", data: new IPersistable[] { train, coach }.ToRequestDictionary(), jsonKey: "places")
					.Then(t => ParseSeats(t.Result), ex => ex is ResponseException ? (IReadOnlyDictionary<string, int[]>)null : throw ex);
		}

		public Task<dynamic> BookSeatAsync(Train train, Coach coach, Seat seat, Passenger passenger)
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

			return GetJsonAsync("cart/add/", data: data).Then(t => (dynamic)(t.Result as JObject));
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

		public Task<Route> GetSelectedTrainRouteAsync(Train train)
		{
			return GetTrainRoutesAsync(new RouteData { Train = train }).Then(t => t.Result.FirstOrDefault());
		}

		public Task<Route[]> GetTrainRoutesAsync(params RouteData[] routes)
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

			return GetJsonAsync<Route[]>("route/", data: data, jsonKey: "routes");
		}

		private IDictionary<string, string> GetDefaultHeaders()
		{
			return new Dictionary<string, string>
			{
				["User-Agent"] = _userAgent
			};
		}

		private async Task<string> GetStringAsync(string path, HttpMethod method,
													IDictionary<string, string> headers,
													IDictionary<string, string> data)
		{
			var url = path;
			var req = new HttpRequestMessage(method ?? HttpMethod.Post, url);

			if (_userAgent == null)
			{
				using (var httpLock = AsyncLock.Lock(_httpInitLock))
				{
					if (httpLock.IsCaptured && _userAgent == null)
					{
						await InitializeHttpClient();
					}
				}
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

		private async Task<JToken> GetJsonAsync(string path, HttpMethod method = null,
													IDictionary<string, string> headers = null,
													IDictionary<string, string> data = null,
													string jsonKey = null)
		{
			var str = await GetStringAsync(path, method, headers, data);

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


				if (value is JObject valueObj)
				{
					if (valueObj.TryGetValue("errors", out var errorsArray))
					{
						message = errorsArray is IEnumerable<JToken> errors ? String.Join(" | ", errors.Select(jv => (string)jv)) : null;
					}
					else if (valueObj.TryGetValue("error", out var errorValue))
					{
						message = errorValue.ToString();
					}
					else
					{
						message = valueObj.ToString();
					}
				}
				else
				{
					message = (string)value;
				}

				throw new ResponseException(message, str);
			}

			var token = jObj != null && jObj.ContainsKey(_dataKey) ? json[_dataKey] : json;
			return String.IsNullOrEmpty(jsonKey) ? token : token[jsonKey];
		}

		private Task<T> GetJsonAsync<T>(string path, HttpMethod method = null,
											IDictionary<string, string> headers = null,
											IDictionary<string, string> data = null,
											string jsonKey = null)
		{
			return GetJsonAsync(path, method, headers, data, jsonKey).Then(t => t.Result.Deserialize<T>());
		}

		private async Task InitializeHttpClient()
		{
			var userAgent = UserAgentSelector.GetRandomAgent();
			var httpHandler = new HttpClientHandler();
			var httpClient = new HttpClient(httpHandler)
			{
				BaseAddress = new Uri(_baseUrl),
				Timeout = TimeSpan.FromSeconds(_requestTimeout)
			};

			if (_proxyProvider != null)
			{
				var proxyUrl = await _proxyProvider.GetProxyAsync(CheckProxy);
				httpHandler.Proxy = new WebProxy(new Uri(proxyUrl));
				_logger.Info("UzService uses proxy " + proxyUrl);
			}

			_httpClient = httpClient;
			_httpHandler = httpHandler;
			_userAgent = userAgent;
		}

		private async Task<bool> CheckProxy(string proxyUrl)
		{
			var uri = new Uri(_baseUrl);
			var builder = new UriBuilder(uri) { Path = String.Empty };

			uri = builder.Uri;

			var handler = new HttpClientHandler { Proxy = new WebProxy(new Uri(proxyUrl)), AllowAutoRedirect = true };
			var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(_requestTimeout) };

			try
			{
				var req = new HttpRequestMessage(HttpMethod.Head, uri);
				req.Headers.UserAgent.ParseAdd(UserAgentSelector.GetRandomAgent());

				var resp = await client.SendAsync(req);
				return resp.IsSuccessStatusCode;
			}
			catch (Exception e)
			{
				return false;
			}
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
			return new ReadOnlyDictionary<string, int[]>(jToken.Deserialize<IDictionary<string, int[]>>());
		}
	}
}
