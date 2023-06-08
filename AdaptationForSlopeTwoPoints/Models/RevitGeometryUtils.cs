using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace AdaptationForSlopeTwoPoints.Models
{
    internal class RevitGeometryUtils
    {
        // Метод получения экземпляров семейств
        public static List<FamilyInstance> GetFamilyInstances(UIApplication uiapp, out string elementIds)
        {
            Selection sel = uiapp.ActiveUIDocument.Selection;
            var selectedElements = sel.PickElementsByRectangle(new GenericModelCategoryFilter(), "Select Family Instances");
            elementIds = ElementIdToString(selectedElements);
            var familyInstances = selectedElements.OfType<FamilyInstance>().ToList();

            return familyInstances;
        }

        // Метод получения списка линий на поверхности дороги
        public static List<Line> GetRoadLines(UIApplication uiapp, out string elementIds)
        {
            Selection sel = uiapp.ActiveUIDocument.Selection;
            var selectedOnRoadSurface = sel.PickObjects(ObjectType.Element, "Select Road Lines");
            var directShapesRoadSurface = selectedOnRoadSurface.Select(r => uiapp.ActiveUIDocument.Document.GetElement(r))
                                                               .OfType<DirectShape>();
            elementIds = ElementIdToString(directShapesRoadSurface);
            var curvesRoadSurface = GetCurvesByDirectShapes(directShapesRoadSurface);
            var linesRoadSurface = curvesRoadSurface.OfType<Line>().ToList();

            return linesRoadSurface;
        }

        // Получение точки пересечения профиля с линиями на поверхности дороги
        public static XYZ GetIntersectPoint(Document doc ,FamilyInstance profile, IEnumerable<Line> roadLines)
        {
            Plane plane = GetPlanesByAdaptiveProfile(doc, profile);
            if (plane.XVec.Z == -1 || plane.XVec.Z == 1)
            {
                plane = Plane.CreateByOriginAndBasis(plane.Origin, plane.YVec, plane.XVec);
            }
            Line intersectLine = GetIntersectCurve(roadLines, plane);
            XYZ intersectPoint = LinePlaneIntersection(intersectLine, plane, out _);

            return intersectPoint;
        }

        public static List<ReferencePoint> GetShapeHandlePoints(Document doc ,FamilyInstance profile)
        {
            var pointIds = AdaptiveComponentInstanceUtils.GetInstanceShapeHandlePointElementRefIds(profile);
            var points = pointIds.Select(id => doc.GetElement(id)).OfType<ReferencePoint>().ToList();

            return points;
        }

        // Получение линии из списка, которая пересекается с плоскостью
        private static Line GetIntersectCurve(IEnumerable<Line> lines, Plane plane)
        {
            XYZ originPlane = plane.Origin;
            XYZ originPlaneBase = new XYZ(originPlane.X, originPlane.Y, 0);
            XYZ directionLine = plane.XVec;
            XYZ directionLineBase = new XYZ(directionLine.X, directionLine.Y, 0);

            var lineByPlane = Line.CreateUnbound(originPlaneBase, directionLineBase);

            foreach (var line in lines)
            {
                XYZ startPoint = line.GetEndPoint(0);
                XYZ finishPoint = line.GetEndPoint(1);

                XYZ startPointOnBase = new XYZ(startPoint.X, startPoint.Y, 0);
                XYZ finishPointOnBase = new XYZ(finishPoint.X, finishPoint.Y, 0);

                var baseLine = Line.CreateBound(startPointOnBase, finishPointOnBase);

                var result = new IntersectionResultArray();
                var compResult = lineByPlane.Intersect(baseLine, out result);
                if (compResult == SetComparisonResult.Overlap)
                {
                    return line;
                }
            }

            return null;
        }

        /* Пересечение линии и плоскости
        * (преобразует линию в вектор, поэтому пересекает любую линию не параллельную плоскости)
        */
        private static XYZ LinePlaneIntersection(Line line, Plane plane, out double lineParameter)
        {
            XYZ planePoint = plane.Origin;
            XYZ planeNormal = plane.Normal;
            XYZ linePoint = line.GetEndPoint(0);

            XYZ lineDirection = (line.GetEndPoint(1) - linePoint).Normalize();

            // Проверка на параллельность линии и плоскости
            if ((planeNormal.DotProduct(lineDirection)) == 0)
            {
                lineParameter = double.NaN;
                return null;
            }

            lineParameter = (planeNormal.DotProduct(planePoint)
              - planeNormal.DotProduct(linePoint))
                / planeNormal.DotProduct(lineDirection);

            return linePoint + lineParameter * lineDirection;
        }

        // Метод получения плоскости в которой размещен адаптивный профиль.
        private static Plane GetPlanesByAdaptiveProfile(Document doc, FamilyInstance profile)
        {
            var points = AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(profile)
                                                       .Select(id => doc.GetElement(id))
                                                       .OfType<ReferencePoint>()
                                                       .Select(p => p.Position);

            var plane = Plane.CreateByThreePoints(points.ElementAt(0),
                                                  points.ElementAt(1),
                                                  points.ElementAt(2));
            return plane;
        }

        // Получение линий на основе элементов DirectShape
        private static List<Curve> GetCurvesByDirectShapes(IEnumerable<DirectShape> directShapes)
        {
            var curves = new List<Curve>();

            Options options = new Options();
            var geometries = directShapes.Select(d => d.get_Geometry(options)).SelectMany(g => g);

            foreach (var geom in geometries)
            {
                if (geom is PolyLine polyLine)
                {
                    var polyCurve = GetCurvesByPolyline(polyLine);
                    curves.AddRange(polyCurve);
                }
                else
                {
                    curves.Add(geom as Curve);
                }
            }

            return curves;
        }

        // Метод получения списка линий на основе полилинии
        private static IEnumerable<Curve> GetCurvesByPolyline(PolyLine polyLine)
        {
            var curves = new List<Curve>();

            for (int i = 0; i < polyLine.NumberOfCoordinates - 1; i++)
            {
                var line = Line.CreateBound(polyLine.GetCoordinate(i), polyLine.GetCoordinate(i + 1));
                curves.Add(line);
            }

            return curves;
        }

        // Метод получения строки с ElementId
        private static string ElementIdToString(IEnumerable<Element> elements)
        {
            var stringArr = elements.Select(e => "Id" + e.Id.IntegerValue.ToString()).ToArray();
            string resultString = string.Join(", ", stringArr);

            return resultString;
        }
    }
}
