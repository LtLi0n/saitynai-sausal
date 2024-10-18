namespace Saitynai.Backend.Core;

public class ResultError
{
	public int StatusCode { get; set; }
	public string Message { get; set; }

	public ResultError(int statusCode, string message)
	{
		StatusCode = statusCode;
		Message = message;
	}
}