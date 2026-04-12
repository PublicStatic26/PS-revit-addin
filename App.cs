using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace PSRevitAddin
{
    public class App : IExternalApplication
    {
        private const string TabName = "PS 창호설계";
        private const string PanelName = "창호 자동화";
        private const string ButtonName = "PSWindowDesign";
        private const string ButtonText = "창호 설계";

        public static UIControlledApplication? UIControlApp = null;
        static readonly string AddInPath = typeof(App).Assembly.Location;

        public Result OnStartup(UIControlledApplication app)
        {
            UIControlApp = app;

            try
            {
                try { app.CreateRibbonTab(TabName); } catch { }

                RibbonPanel? ribbonPanel = null;
                var panels = app.GetRibbonPanels(TabName);

                foreach (RibbonPanel rp in panels)
                {
                    if (rp.Name == PanelName)
                    {
                        ribbonPanel = rp;
                        break;
                    }
                }

                ribbonPanel ??= app.CreateRibbonPanel(TabName, PanelName);

                bool bFound = false;
                foreach (RibbonItem item in ribbonPanel.GetItems())
                {
                    if (item.Name == ButtonName)
                    {
                        bFound = true;
                        break;
                    }
                }

                if (!bFound)
                {
                    PushButton? pushButton = ribbonPanel.AddItem(
                        new PushButtonData(ButtonName, ButtonText, AddInPath, "PSRevitAddin.Command")
                    ) as PushButton;

                    if (pushButton != null)
                        pushButton.ToolTip = "창호 설계 자동화를 시작합니다.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication _)
        {
            return Result.Succeeded;
        }
    }
}
