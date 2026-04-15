using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace PSRevitAddin
{
    public class GenericExternalEventHandler : IExternalEventHandler
    {
        public Action<UIApplication>? ActionToExecute { get; set; }
        public bool Success { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;

        public void Execute(UIApplication app)
        {
            Success = false;
            ErrorMessage = string.Empty;

            try
            {
                ActionToExecute?.Invoke(app);
                Success = true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                Success = false;
            }
        }

        public string GetName()
        {
            return "PSRevitAddin.GenericExternalEventHandler";
        }
    }
}
