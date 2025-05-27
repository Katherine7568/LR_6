using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

public class RegexSearcher
{
    public class MatchInfo
    {
        public string Value { get; set; }
        public int Position { get; set; }  // Позиция в тексте (начиная с 0)
        public int Length { get; set; }
    }

    public static List<MatchInfo> FindMatches(string text, string pattern)
    {
        var matches = new List<MatchInfo>();
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(pattern))
            return matches;

        try
        {
            foreach (Match match in Regex.Matches(text, pattern))
            {
                matches.Add(new MatchInfo
                {
                    Value = match.Value,
                    Position = match.Index,
                    Length = match.Length
                });
            }
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"Ошибка в регулярном выражении: {ex.Message}");
        }

        return matches;
    }
}