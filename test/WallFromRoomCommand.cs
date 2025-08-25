
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;


namespace test
{
    [Transaction(TransactionMode.Manual)]
    public class WallFromRoomCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;
            var viewModal = new MainViewModel(doc);
            var window = new MainWindow { DataContext = viewModal };
            window.ShowDialog();
            return Result.Succeeded;
        }
    }
}