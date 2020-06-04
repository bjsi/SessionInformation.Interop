using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperMemoAssistant.Plugins.SessionInformation.Interop.Models
{
  [Serializable]
  public class ElementReferences
  {
    /// <summary>
    /// The original author for the given content
    /// </summary>
    public string Author { get; set; }

    /// <summary>
    /// The title for the given content
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// The dates relevant to the original content (e.g. creation date, updated date)
    /// </summary>
    public string Date { get; set; }

    /// <summary>
    /// The original source for the given content
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// The original uri for the given content
    /// </summary>
    public string Link { get; set; }

    /// <summary>
    /// The email from which the given content was extracted
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Notes about the given content
    /// </summary>
    public string Comment { get; set; }

    public ElementReferences(Dictionary<string, string> results)
    {

      this.Author = results["Author"];
      this.Title = results["Title"];
      this.Date = results["Date"];
      this.Source = results["Source"];
      this.Link = results["Link"];
      this.Email = results["Email"];
      this.Comment = results["Comment"];
    }
  }

  public static class ReferenceHelpers
  {
    public static string referenceTitle = "#SuperMemo Reference:";
    public static Dictionary<string, string> refs = new Dictionary<string, string>()
    {
      { "Author", "#Author: "},
      { "Email", "#Email: "},
      { "Comment", "#Comment: "},
      { "Link", "#Link: "},
      { "Source", "#Source: "},
      { "Title", "#Title: "},
      { "Date", "#Date: "},
      { "Article", "#Article: "},
      { "Parent", "#Parent: "},
      { "Concept Group", "#Concept group: " }
    };

    public static string ParseReferences(string content)
    {
      if (string.IsNullOrEmpty(content))
        return null;

      var doc = new HtmlDocument();
      doc.LoadHtml(content);
      string text = doc.DocumentNode.InnerText;

      if (string.IsNullOrEmpty(text))
        return null;

      int refIdx = text.IndexOf(referenceTitle);
      if (refIdx < 0)
        return null;

      return text.Substring(refIdx + referenceTitle.Length).Trim();
    }

    public static ElementReferences CreateReferences(string references)
    {
      var results = new Dictionary<string, string>();
      foreach (var key in refs.Keys)
      {
        string result = ParseReferenceElement(references, refs[key]);
        results.Add(key, result);
      }
      return new ElementReferences(results);
    }

    private static string ParseReferenceElement(string references, string refKey)
    {
      if (string.IsNullOrEmpty(references))
        return string.Empty;

      int matchIdx = references.IndexOf(refKey);
      if (matchIdx < 0)
        return string.Empty;

      string search = references.Substring(matchIdx);
      var indices = refs.Values.Select(x => search.IndexOf(x)).Where(x => x > 0);
      int endIdx = -1;
      if (indices.Count() == 0)
        endIdx = search.Length;
      else
        endIdx = indices.Min();
      return search.Substring(refKey.Length, endIdx - refKey.Length).Trim();
    }
  }
}
