using SuperMemoAssistant.Plugins.SessionInformation.Interop.Models;
using System.Collections.Generic;

namespace SuperMemoAssistant.Plugins.SessionInformation.Interop
{
  public interface ISessionInformationService
  {
    List<SummarySnapshot> SummarySnapshots { get; }
  }
}
