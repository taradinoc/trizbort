using System.Collections.Generic;

namespace Trizbort
{
  public static class ApplicationState
  {
    static ApplicationState()
    {
      Projects = new List<Project>();
    }

    public static IList<Project> Projects { get; private set; }
    public static Project CurrentProject { get; set; }
  }
}