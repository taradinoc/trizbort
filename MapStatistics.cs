using System.Linq;

namespace Trizbort
{
  public static class MapStatistics
  {
    public static int NumberOfRooms => ApplicationState.CurrentProject.Map.Elements.OfType<Room>().Count();
    public static int NumberOfFloatingRooms => ApplicationState.CurrentProject.Map.Elements.OfType<Room>().Count(p => p.GetConnections().Count == 0);
    public static int NumberOfRoomsWithObjects => ApplicationState.CurrentProject.Map.Elements.OfType<Room>().Count(p => p.ListOfObjects().Count > 0);
    public static int NumberOfTotalObjectsInRooms => ApplicationState.CurrentProject.Map.Elements.OfType<Room>().ToList().Sum(p => p.ListOfObjects().Count);
    public static int NumberOfConnections => ApplicationState.CurrentProject.Map.Elements.OfType<Connection>().Count();
    public static int NumberOfDanglingConnections => ApplicationState.CurrentProject.Map.Elements.OfType<Connection>().Count(p => p.GetSourceRoom() == null || p.GetTargetRoom() == null);

    public static int NumberOfLoopingConnections => ApplicationState.CurrentProject.Map.Elements.OfType<Connection>().Count(p =>
    {
      var sourceRoom = p.GetSourceRoom();
      var targetRoom = p.GetTargetRoom();
      return ((sourceRoom != null && targetRoom != null) && (sourceRoom == targetRoom));
    });

    public static int NumberOfRegions => Settings.Regions.Count(p=>p.RegionName != Region.DefaultRegion);

    public static int NumberOfRoomsInRegion(string pRegion) => ApplicationState.CurrentProject.Map.Elements.OfType<Room>().Count(p => p.Region == pRegion);
  }
}