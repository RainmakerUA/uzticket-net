using System.Xml.Linq;

namespace RM.Lib.StateMachine
{
    internal static class XmlNames
    {
        public const string ScxmlSchemaUri = "http://www.w3.org/2005/07/scxml";
        public const string Id = "id";

        public static readonly XName StateName = XName.Get("state", ScxmlSchemaUri);
			
        public static readonly XName TransitionName = XName.Get("transition", ScxmlSchemaUri);

        public static readonly XName DatamodelName = XName.Get("datamodel", ScxmlSchemaUri);
			
        public static readonly XName DataName = XName.Get("data", ScxmlSchemaUri);
			
        public static readonly XName OnEntryName = XName.Get("onentry", ScxmlSchemaUri);
			
        public static readonly XName OnExitName = XName.Get("onexit", ScxmlSchemaUri);
    }
}