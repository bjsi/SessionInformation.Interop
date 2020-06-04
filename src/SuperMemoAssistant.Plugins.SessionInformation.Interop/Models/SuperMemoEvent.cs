using System;

namespace SuperMemoAssistant.Plugins.SessionInformation.Interop.Models
{

  public enum EventOrigin
  {
    DisplayedElementChanged,
    Keyboard,
    Mouse,
    EditedElement
  }

  public class SuperMemoEvent
  {
    // TODO: Should this be UTC Now
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public int ElementId { get; set; }
    public string Content { get; set; }
    public EventOrigin Origin { get; set; }
    public int ChildrenCount { get; set; }
    public SuperMemoEvent(int elementId, EventOrigin Origin, string Content)
    {
      this.ElementId = elementId;
      this.Origin = Origin;
      this.Content = Content;
    }

    public override string ToString()
    {
      return $"[timestamp={Timestamp} id={ElementId} origin={Origin}]";
    }
  }
}
