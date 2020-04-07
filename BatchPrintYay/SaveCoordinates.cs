using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace BatchPrintYay
{
    public class SaveCoordinates : ISaveSharedCoordinatesCallback
    {
        public SaveModifiedLinksOptions GetSaveModifiedLinksOption(RevitLinkType link)
        {
            return SaveModifiedLinksOptions.DoNotSaveLinks;
        }
    }
}
