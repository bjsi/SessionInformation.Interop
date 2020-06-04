using Anotar.Serilog;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
using SuperMemoAssistant.Interop.SuperMemo.Learning;
using SuperMemoAssistant.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperMemoAssistant.Plugins.SessionInformation.Interop.Models
{

  /// <summary>
  /// An element that exists somewhere along the path between the event element and the collection root.
  /// </summary>
  public class PathNode
  {
    public string Title { get; set; }
    public int ElementId { get; set; }
    public ElementType ElementType { get; set; }
    public PathNode(IElement element)
    {
      Title = element.Title;
      ElementId = element.Id;
      ElementType = element.Type;
    }
  }

  /// <summary>
  /// Aggregates the lower level SuperMemo events into a snapshot that summarises
  /// the user's interactions with an element.
  /// </summary>
  public class SummarySnapshot
  {
    // Time Information
    public DateTime StartTimestamp { get; set; }
    public DateTime EndTimestamp { get; set; }
    public double Duration { get; set; }

    // Element Information
    public int ElementId { get; set; }
    public string Title { get; set; }
    public ElementType ElementType { get; set; }
    public int ConceptId { get; set; }
    public string ConceptName { get; set; }
    public List<PathNode> FullPath { get; set; }
    public bool Deleted { get; set; } // Was the element deleted during the Snapshot?
    public ElementReferences References { get; set; }
    public int ChildrenDelta { get; set; } // Change in the number of children of the element

    // Element Content
    public string FirstContent { get; set; }
    public string LastContent { get; set; }
    public string DiffedContent { get; set; }

    // Other Information
    public string CollectionName { get; set; }
    public LearningMode LearningMode { get; set; }

    // TODO: add template and multiple html content components
    // TODO: add element_status
    // TODO: add element_priority
    // TODO: add whether extracts were created / how many
    // --> Currently only indirectly tracked via children
    // TODO: add whether a grade was given to an item

    public SummarySnapshot(List<SuperMemoEvent> events)
    {
      if (events == null || events.Count < 2)
      {
        LogTo.Error("Failed to CreateSnapshot because events is null or contains too few events");
        return;
      }

      SuperMemoEvent FirstEvent = events.First();
      SuperMemoEvent LastEvent = events.Last();

      // Every event should come from the same element id
      int id = FirstEvent.ElementId;
      if (events.Any(e => e.ElementId != id))
      {
        LogTo.Error("Failed to CreateSnapshot because the events list contained events with different element ids");
        return;
      }

      StartTimestamp = FirstEvent.Timestamp;
      EndTimestamp = LastEvent.Timestamp;
      Duration = (LastEvent.Timestamp - FirstEvent.Timestamp).TotalSeconds;

      if (StartTimestamp > EndTimestamp)
      {
        LogTo.Error("Failed to CreateSnapshot because StartTimestamp was greater than EndTimestamp");
        return;
      }

      var element = Svc.SM.Registry.Element[id];
      if (element == null)
      {
        LogTo.Error("Failed to Create SummarySnapshot because element was null");
        return;
      }

      // Other
      LearningMode = Svc.SM.UI.ElementWdw.CurrentLearningMode;
      CollectionName = Svc.SM.Collection.Name;

      // Element Information
      ConceptId = element.Concept?.Id ?? -1;
      ConceptName = element.Concept?.Name ?? string.Empty;
      Deleted = element.Deleted;
      ElementId = id;
      ElementType = element.Type;
      FullPath = GetFullPath(element);
      string refs = ReferenceHelpers.ParseReferences(LastContent);
      References = ReferenceHelpers.CreateReferences(refs);
      Title = element.Title;

      // Element Content

      // TODO What about initially null or empty elements?
      FirstContent = events
        .Where(x => !string.IsNullOrEmpty(x.Content))
        .FirstOrDefault()?.Content ?? string.Empty;

      LastContent = events
        .Where(x => !string.IsNullOrEmpty(x.Content))
        .LastOrDefault()?.Content ?? string.Empty;

      ChildrenDelta = LastEvent.ChildrenCount - FirstEvent.ChildrenCount;

      FirstContent = FirstContent.GetHtmlInnerText();
      LastContent = LastContent.GetHtmlInnerText();
      DiffedContent = DiffEx.CreateDiffList(FirstContent, LastContent).Jsonify();
    }

    private List<PathNode> GetFullPath(IElement element)
    {
      var fullPath = new List<PathNode>();
      if (element == null)
      {
        LogTo.Error("Failed to GetFullPath because the element was null");
        return fullPath;
      }

      // Get parents until the root
      var cur = element.Parent;
      while (cur != null)
      {
        fullPath.Add(new PathNode(cur));
        cur = cur.Parent;
      }
      return fullPath;
    }

    public override string ToString()
    {
      return $"[\nstart: {StartTimestamp},\nend: {EndTimestamp},\nelid: {ElementId},\nduration: {Duration},\ncontent_diff: {DiffedContent},\n]";
    }
  }
}
