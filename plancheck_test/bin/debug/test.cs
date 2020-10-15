using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace VMS.TPS
{
  public class Script
  {
    public Script()
    {
    }

    public void Execute(ScriptContext context /*, System.Windows.Window window*/)
    {
            MessageBox.Show(  context.PlanSetup.Beams.First().ControlPoints.Skip(1).First().JawPositions.Y2.ToString());
            MessageBox.Show(context.PlanSetup.Beams.First().MLC.Model);
    }
  }
}
