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
using AdaptationForSlopeTwoPoints.ViewModels;

namespace AdaptationForSlopeTwoPoints.Models
{
    internal class GenericModelCategoryFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if(elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel)
            {
                return true;
            }

            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }
}
