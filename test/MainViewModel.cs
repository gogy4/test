using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace test
{
    public class MainViewModel : ObservableObject
    {
        private readonly Document doc;

        public ObservableCollection<RoomItem> Rooms { get; set; } = new ObservableCollection<RoomItem>();

        public IRelayCommand LoadRoomsCommand { get; }
        public IRelayCommand BuildWallsCommand { get; }

        public MainViewModel(Document doc)
        {
            this.doc = doc;
            LoadRoomsCommand = new RelayCommand(LoadRooms);
            BuildWallsCommand = new RelayCommand(BuildWalls);
        }

        private void LoadRooms()
        {
            Rooms.Clear();

            var collector = new FilteredElementCollector(doc)
                .OfClass(typeof(SpatialElement))
                .OfType<Room>();

            foreach (var room in collector)
            {
                Rooms.Add(new RoomItem { Room = room, IsSelected = false });
            }
        }

        private void BuildWalls()
        {
            var selectedRooms = Rooms.Where(r => r.IsSelected).Select(r => r.Room).ToList();
            if (!selectedRooms.Any())
            {
                TaskDialog.Show("Info", "нет выбранных комнат!");
                return;
            }

            using (var t = new Transaction(doc, "build walls from rooms"))
            {
                try
                {
                    t.Start();

                    var level = new FilteredElementCollector(doc)
                        .OfClass(typeof(Level))
                        .Cast<Level>()
                        .FirstOrDefault();

                    if (level is null)
                    {
                        TaskDialog.Show("Error", "нет уровня");
                        return;
                    }

                    var wallType = new FilteredElementCollector(doc)
                        .OfClass(typeof(WallType))
                        .Cast<WallType>()
                        .FirstOrDefault();

                    if (wallType == null)
                    {
                        TaskDialog.Show("Error", "нет типа стены!");
                        return;
                    }

                    var wallHeight = 3.0d;

                    foreach (var room in selectedRooms)
                    {
                        var segments = room.GetBoundarySegments(new SpatialElementBoundaryOptions());
                        if (segments == null) continue;

                        foreach (var segmentList in segments)
                        {
                            foreach (var seg in segmentList)
                            {
                                var curve = seg.GetCurve();
                                if (curve == null) continue;

                                var direction = (curve.GetEndPoint(1) - curve.GetEndPoint(0)).CrossProduct(XYZ.BasisZ).Normalize();

                                var midpoint = (curve.GetEndPoint(0) + curve.GetEndPoint(1)) / 2;
                                var testPoint = midpoint + direction * 0.1;
                                if (!room.IsPointInRoom(testPoint))
                                {
                                    direction = -direction;
                                }

                                var halfWidth = wallType.Width / 2;
                                var offsetCurve = curve.CreateOffset(halfWidth, direction);
                                if (offsetCurve == null) continue;

                                Wall.Create(doc, offsetCurve, wallType.Id, level.Id, wallHeight, 0.0, false, false);
                            }
                        }
                    }

                    t.Commit();
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", ex.Message);
                    t.RollBack();
                }
            }
        }


    }
}
