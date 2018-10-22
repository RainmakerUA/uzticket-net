
namespace RM.UzTicket.Lib.Exceptions
{
	public class HttpException : UzException
	{
		public HttpException(int statusCode, string responseBody, string requestData = null, string json = null)
			: base($"Status code: {statusCode}; request data: {requestData}; response body: {responseBody}")
		{
			StatusCode = statusCode;
			ResponseBody = responseBody;
			RequestData = requestData;
			Json = json;
		}

		public int StatusCode { get; }

		public string ResponseBody { get; }

		public string RequestData { get; }

		public string Json { get; }
	}
}
