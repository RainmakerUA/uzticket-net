using System.Data.Common;
using System.Text.RegularExpressions;

namespace System.Data.SqlClient
{
	public sealed class SqlConnectionStringBuilder : DbConnectionStringBuilder
	{
		private static readonly Regex _dsRegex = new Regex(@"(?:^|;)data source=(?'dsquote'\""*)(?'dsname'.+?)\k'dsquote'(?:$|;)", RegexOptions.Compiled);
		private static readonly Regex _catRegex = new Regex(@"(?:^|;)initial catalog=(?'catquote'\""*)(?'catname'.+?)\k'catquote'(?:$|;)", RegexOptions.Compiled);

		public SqlConnectionStringBuilder(string connectionString)
		{
			(DataSource, InitialCatalog) = ParseConnectionString(connectionString);
		}

		public string DataSource { get; }

		public string InitialCatalog { get; }

		private static (string, string) ParseConnectionString(string str)
		{
			return (
						_dsRegex.Match(str) is Match m1 && m1.Success ? m1.Groups["dsname"].Value : null,
						_catRegex.Match(str) is Match m2 && m2.Success ? m2.Groups["catname"].Value : null
				);
		}
	}
}
