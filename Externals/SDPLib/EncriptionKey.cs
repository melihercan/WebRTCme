namespace SDPLib
{
    public class EncriptionKey
    {
        public const string ClearMethod = "clear";
        public const string Base64Method = "base64";
        public const string UriMethod = "uri";
        public const string PromptMethod = "prompt";

        public string Method { get; set; }
        public string Value { get; set; }
    }
}
