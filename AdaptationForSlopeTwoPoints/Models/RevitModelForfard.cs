using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;
using System.Collections.ObjectModel;
using AdaptationForSlopeTwoPoints.Models;
using System.IO;

namespace AdaptationForSlopeTwoPoints
{
    public class RevitModelForfard
    {
        private UIApplication Uiapp { get; set; } = null;
        private Application App { get; set; } = null;
        private UIDocument Uidoc { get; set; } = null;
        private Document Doc { get; set; } = null;

        public RevitModelForfard(UIApplication uiapp)
        {
            Uiapp = uiapp;
            App = uiapp.Application;
            Uidoc = uiapp.ActiveUIDocument;
            Doc = uiapp.ActiveUIDocument.Document;
        }

        #region Семейства адаптивных профилей
        public List<FamilyInstance> AdaptiveProfiles { get; set; }

        private string _adaptiveProfileElemIds;
        public string AdaptiveProfileElemIds
        {
            get => _adaptiveProfileElemIds;
            set => _adaptiveProfileElemIds = value;
        }

        public void GetAdaptiveProfiles()
        {
            AdaptiveProfiles = RevitGeometryUtils.GetFamilyInstances(Uiapp, out _adaptiveProfileElemIds);
        }

        #endregion

        #region Линия на поверхности 1
        public List<Line> RoadLines1 { get; set; }

        private string _roadLineElemIds1;
        public string RoadLineElemIds1
        {
            get => _roadLineElemIds1;
            set => _roadLineElemIds1 = value;
        }

        public void GetRoadLine1()
        {
            RoadLines1 = RevitGeometryUtils.GetRoadLines(Uiapp, out _roadLineElemIds1);
        }
        #endregion

        #region Линия на поверхности 2
        public List<Line> RoadLines2 { get; set; }

        private string _roadLineElemIds2;
        public string RoadLineElemIds2
        {
            get => _roadLineElemIds2;
            set => _roadLineElemIds2 = value;
        }

        public void GetRoadLine2()
        {
            RoadLines2 = RevitGeometryUtils.GetRoadLines(Uiapp, out _roadLineElemIds2);
        }
        #endregion

        #region Перенос точки ручки формы на линию
        public void MoveShapeHandlePoint()
        {
            using (Transaction trans = new Transaction(Doc, "Адаптация Профиля Под Уклон"))
            {
                trans.Start();
                foreach (var profile in AdaptiveProfiles)
                {
                    XYZ intersectionPoint1 = RevitGeometryUtils.GetIntersectPoint(Doc, profile, RoadLines1);
                    XYZ intersectionPoint2 = RevitGeometryUtils.GetIntersectPoint(Doc, profile, RoadLines2);

                    ReferencePoint shapeHandlePoint1 = RevitGeometryUtils.GetShapeHandlePoints(Doc, profile).First();
                    ReferencePoint shapeHandlePoint2 = RevitGeometryUtils.GetShapeHandlePoints(Doc, profile).ElementAt(1);

                    shapeHandlePoint1.Position = intersectionPoint1;
                    shapeHandlePoint2.Position = intersectionPoint2;
                }
                trans.Commit();
            }
        }
        #endregion

    }
}
