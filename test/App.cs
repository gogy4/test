using Autodesk.Revit.UI;
using System.Reflection;

namespace test
{
    internal class App : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            var tbName = "CustomTools";
            application.CreateRibbonTab(tbName);
            var panel = application.CreateRibbonPanel(tbName, "Walls");

            var btnData = new PushButtonData("RoomWalls", "Помещения => Стены",
                Assembly.GetExecutingAssembly().Location,
                "test.WallFromRoomCommand");

            panel.AddItem(btnData);
            return Result.Succeeded;
        }
    }
}
