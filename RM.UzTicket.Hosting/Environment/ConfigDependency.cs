
namespace RM.UzTicket.Hosting.Environment
{
	public sealed class ConfigDependency
	{
		public string To { get; set; }
		
		public string From { get; set; }
		
		public Lifetime Lifetime { get; set; }
		
		public Construction Construction { get; set; }
		
		public string ConstructionFactory { get; set; }
		
		public string ConstructionMethod { get; set; }
		
		public string Name { get; set; }
	}
}