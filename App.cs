using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
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

            // Revit이 NuGet DLL을 못 찾을 때 애드인 폴더에서 직접 로드
            string addInDir = Path.GetDirectoryName(typeof(App).Assembly.Location)!;

            // 이 애드인을 로드한 AssemblyLoadContext에 리졸버 등록
            var loadContext = AssemblyLoadContext.GetLoadContext(typeof(App).Assembly);
            if (loadContext != null)
            {
                loadContext.Resolving += (context, assemblyName) =>
                {
                    string path = Path.Combine(addInDir, assemblyName.Name + ".dll");
                    return File.Exists(path) ? context.LoadFromAssemblyPath(path) : null;
                };
            }

            // 혹시 Default 컨텍스트에서도 못 찾을 경우 대비
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string assemblyName = new AssemblyName(args.Name).Name!;
                string path = Path.Combine(addInDir, assemblyName + ".dll");
                return File.Exists(path) ? Assembly.LoadFrom(path) : null;
            };

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

                if (ribbonPanel == null)
                {
                    ribbonPanel = app.CreateRibbonPanel(TabName, PanelName);
                }

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
