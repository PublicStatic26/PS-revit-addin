using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using PSRevitAddin.Forms;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PSRevitAddin
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        private static MainForm? _form = null;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication uiApp = commandData.Application;

                // 이미 폼이 열려 있으면 앞으로 가져온다
                if (_form != null && !_form.IsDisposed)
                {
                    _form.Focus();
                    _form.BringToFront();
                    return Result.Succeeded;
                }

                // 모드리스 폼 생성
                _form = new MainForm(uiApp);
                _form.Show();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return Result.Failed;
            }
        }
    }
}
