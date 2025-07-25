using Microsoft.AspNetCore.Http;
using ObiletJourneySearch.Models.DTOs;
using System.Text.RegularExpressions;

namespace ObiletJourneySearch.Utilities
{
    public static class BrowserInfoExtractor
    {
        private static readonly Regex BrowserRegex = new Regex(@"(chrome|firefox|safari|edge|opera|ie)[\/\s]*([\d\.]+)", RegexOptions.IgnoreCase);
        
        public static Browser GetBrowserInfo(HttpContext context)
        {
            var userAgent = context.Request.Headers.UserAgent.ToString();
            
            var browser = new Browser
            {
                Name = ExtractBrowserName(userAgent),
                Version = ExtractBrowserVersion(userAgent)
            };
            
            return browser;
        }
        
        private static string ExtractBrowserName(string userAgent)
        {
            var match = BrowserRegex.Match(userAgent);
            if (match.Success)
            {
                return match.Groups[1].Value.ToLower() switch
                {
                    "chrome" => "Chrome",
                    "firefox" => "Firefox",
                    "safari" => "Safari",
                    "edge" => "Edge",
                    "opera" => "Opera",
                    "ie" => "Internet Explorer",
                    _ => "Unknown"
                };
            }
            
            return "Unknown";
        }
        
        private static string ExtractBrowserVersion(string userAgent)
        {
            var match = BrowserRegex.Match(userAgent);
            if (match.Success && match.Groups.Count > 2)
            {
                return match.Groups[2].Value;
            }
            
            return "0.0";
        }
    }
}
