using Ganss.Xss;
using System;

namespace BlogApp.Helpers
{
    public static class HtmlSanitizerHelper
    {
        private static readonly HtmlSanitizer _sanitizer;

        static HtmlSanitizerHelper()
        {
            _sanitizer = new HtmlSanitizer();
            
            // Allow common HTML tags for content
            _sanitizer.AllowedTags.Add("div");
            _sanitizer.AllowedTags.Add("span");
            _sanitizer.AllowedTags.Add("p");
            _sanitizer.AllowedTags.Add("h1");
            _sanitizer.AllowedTags.Add("h2");
            _sanitizer.AllowedTags.Add("h3");
            _sanitizer.AllowedTags.Add("h4");
            _sanitizer.AllowedTags.Add("h5");
            _sanitizer.AllowedTags.Add("h6");
            _sanitizer.AllowedTags.Add("strong");
            _sanitizer.AllowedTags.Add("em");
            _sanitizer.AllowedTags.Add("u");
            _sanitizer.AllowedTags.Add("s");
            _sanitizer.AllowedTags.Add("ul");
            _sanitizer.AllowedTags.Add("ol");
            _sanitizer.AllowedTags.Add("li");
            _sanitizer.AllowedTags.Add("blockquote");
            _sanitizer.AllowedTags.Add("code");
            _sanitizer.AllowedTags.Add("pre");
            _sanitizer.AllowedTags.Add("hr");
            _sanitizer.AllowedTags.Add("br");
            _sanitizer.AllowedTags.Add("img");
            _sanitizer.AllowedTags.Add("a");
            _sanitizer.AllowedTags.Add("table");
            _sanitizer.AllowedTags.Add("thead");
            _sanitizer.AllowedTags.Add("tbody");
            _sanitizer.AllowedTags.Add("tr");
            _sanitizer.AllowedTags.Add("th");
            _sanitizer.AllowedTags.Add("td");
            
            // Allow necessary attributes
            _sanitizer.AllowedAttributes.Add("class");
            _sanitizer.AllowedAttributes.Add("id");
            _sanitizer.AllowedAttributes.Add("href");
            _sanitizer.AllowedAttributes.Add("target");
            _sanitizer.AllowedAttributes.Add("src");
            _sanitizer.AllowedAttributes.Add("alt");
            _sanitizer.AllowedAttributes.Add("title");
            _sanitizer.AllowedAttributes.Add("style");
            _sanitizer.AllowedAttributes.Add("width");
            _sanitizer.AllowedAttributes.Add("height");
            
            // Allow some safe CSS properties
            _sanitizer.AllowedCssProperties.Add("color");
            _sanitizer.AllowedCssProperties.Add("background-color");
            _sanitizer.AllowedCssProperties.Add("font-size");
            _sanitizer.AllowedCssProperties.Add("font-weight");
            _sanitizer.AllowedCssProperties.Add("text-align");
            _sanitizer.AllowedCssProperties.Add("margin");
            _sanitizer.AllowedCssProperties.Add("padding");
            _sanitizer.AllowedCssProperties.Add("border");
            _sanitizer.AllowedCssProperties.Add("display");
            
            // Restrict URL schemes
            _sanitizer.AllowedSchemes.Add("http");
            _sanitizer.AllowedSchemes.Add("https");
            _sanitizer.AllowedSchemes.Add("mailto");
        }

        /// <summary>
        /// Sanitizes HTML content to prevent XSS attacks
        /// </summary>
        /// <param name="html">The raw HTML content</param>
        /// <returns>Sanitized HTML content</returns>
        public static string Sanitize(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return string.Empty;
            }
            
            try
            {
                return _sanitizer.Sanitize(html);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"HTML sanitization error: {ex.Message}");
                
                // Return safe alternative
                return HtmlEncode(html);
            }
        }
        
        private static string HtmlEncode(string text)
        {
            return System.Net.WebUtility.HtmlEncode(text);
        }
    }
} 