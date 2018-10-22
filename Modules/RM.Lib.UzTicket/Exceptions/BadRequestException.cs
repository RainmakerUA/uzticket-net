namespace RM.UzTicket.Lib.Exceptions
{
	public class BadRequestException : HttpException
	{
		public BadRequestException(int statusCode, string responseBody, string requestData = null, string json = null)
			: base(statusCode, responseBody, requestData, json)
		{
		}
	}
}