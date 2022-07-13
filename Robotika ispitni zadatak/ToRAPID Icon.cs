using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robotika_ispitni_zadatak
{
    public class ToRAPIDCategoryIcon : Grasshopper.Kernel.GH_AssemblyPriority
    {
        public override Grasshopper.Kernel.GH_LoadingInstruction PriorityLoad()
        {
            Grasshopper.Instances.ComponentServer.AddCategoryIcon("ToRAPID", Properties.Resources.ToRAPID_L);
            Grasshopper.Instances.ComponentServer.AddCategorySymbolName("ToRAPID", 'R');
            return Grasshopper.Kernel.GH_LoadingInstruction.Proceed;
        }
    }
}
