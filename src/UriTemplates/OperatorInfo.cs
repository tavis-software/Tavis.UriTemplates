namespace Tavis.UriTemplates
{
    public class OperatorInfo
    {
        public bool Default { get; set; }
        public string First { get; set; }
        public char Separator { get; set; }
        public bool Named { get; set; }
        public string IfEmpty { get; set; }
        public bool AllowReserved { get; set; }

    }
}