using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RM.UzTicket.Lib.Exceptions;
using RM.UzTicket.Lib.Model;
using RM.UzTicket.Lib.Utils;

namespace RM.Lib.UzTicket
{
	internal sealed class UzService //: Test.IUzService
	{
		private const string _baseUrl = "https://booking.uz.gov.ua"; // strict without trailing '/'!
		private const string _sessionIdKey = "_gv_sessid";
		private const int _requestTimeout = 10;
		//private const int _tokenMaxAge = 600;
		private const string _dataKey = "data";

		private readonly AutoResetEvent _tokenLock;
		private HttpClientHandler _httpHandler;
		private HttpClient _httpClient;

		//private string _token;
		//private int _tokenTime;
		private string _userAgent;
		private bool _isDisposed;

		public UzService()
		{
			_tokenLock = new AutoResetEvent(true);
			InitializeHttpClient();
		}

		#region Disposable

		~UzService()
		{
			Dispose(false);
		}

		private void Dispose(bool isDisposing)
		{
			if (!_isDisposed)
			{
				if (isDisposing)
				{
					// Free managed resources
					_httpClient.Dispose();
					_httpClient = null;

					_httpHandler.Dispose();
					_httpHandler = null;

					_tokenLock.Dispose();
				}

				// Free unmanaged resources
				// ...

				_isDisposed = true;
			}
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}

		#endregion

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
				return trains.FirstOrDefault(tr => tr.Number == trainNumber);
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

		private async Task<IDictionary<string, string>> GetDefaultHeadersAsync()
		{
			return new Dictionary<string, string>
						{
							["User-Agent"] = await Task.FromResult(_userAgent),
							/*["GV-Ajax"] = "1",
							["GV-Referer"] = referer,
							["GV-Token"] = await GetTokenAsync()*/
						};
		}

		private async Task<string> GetString(string path, HttpMethod method, IDictionary<string, string> headers,
											IDictionary<string, string> data)
		{
			var url = GetUrl(path);
			var req = new HttpRequestMessage(method ?? HttpMethod.Post, url);

			if (data != null && data.Count > 0)
			{
				req.Content = new FormUrlEncodedContent(data);
			}

			if (headers == null)
			{
				headers = await GetDefaultHeadersAsync();
			}

			SetHeaders(req.Headers, headers);

			Logger.Debug($"Fetching: {url}");
			Logger.Debug($"Headers: {String.Join("\n", headers.Select(kv => $"{kv.Key}: {kv.Value}"))}");
			Logger.Debug($"Cookies: {String.Join("\n", _httpHandler.CookieContainer.GetCookies(new Uri(_baseUrl)).Cast<Cookie>().Select(c => c.ToString()))}");

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

		private void InitializeHttpClient()
		{
			_userAgent = UserAgentSelector.GetRandomAgent();
			_httpHandler = new HttpClientHandler();
			_httpClient = new HttpClient(_httpHandler)
								{
									BaseAddress = new Uri(_baseUrl),
									Timeout = TimeSpan.FromSeconds(_requestTimeout)
								};
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

		private static string GetUrl(string relPath)
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
