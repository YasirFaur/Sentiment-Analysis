namespace Sentiment
{
    public class TextProcessor
    {
        // Validates if the text length sits strictly between 256 and 1024 characters
        public bool IsValidLength(string text)
        {
            // Return true only if text is not null and its length is within range
            return text != null && text.Length >= 256 && text.Length <= 1024;
        }
    }
}