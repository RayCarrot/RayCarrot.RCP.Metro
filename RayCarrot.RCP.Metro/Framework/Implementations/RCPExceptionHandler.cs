using RayCarrot.CarrotFramework;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The exception handler for the Rayman Control Panel
    /// </summary>
    public class RCPExceptionHandler : DefaultExceptionHandler
    {
        /// <summary>Handles an exception</summary>
        /// <param name="exception">The exception to handle</param>
        /// <param name="debugMessage">An optional debug message</param>
        /// <param name="exceptionLevel">The level of the exception</param>
        /// <param name="debugObject">An optional debug object</param>
        public override void HandleException(Exception exception, string debugMessage, ExceptionLevel exceptionLevel, object debugObject = null, [CallerMemberName] string origin = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            // Call base implementation
            base.HandleException(exception, debugMessage, exceptionLevel, debugObject, origin, filePath, lineNumber);

            try
            {
                if (RCFRCP.Data?.DisplayExceptionLevel <= exceptionLevel)
                    _ = RCF.MessageUI.DisplayMessageAsync(GetMessage(), "Exception", MessageType.Error);
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }

            string GetMessage()
            {
                var sb = new StringBuilder();
                sb.AppendLine("An exception has occurred in the application. Below is the available debug information for the current user level in the Carrot Framework.");
                sb.AppendLine();
                if (RCF.Data.CurrentUserLevel >= UserLevel.Debug)
                {
                    sb.AppendLine($"Exception: {exception}");
                    sb.AppendLine();
                    sb.AppendLine($"Origin: {origin}");
                    sb.AppendLine($"File: {filePath}");
                    sb.AppendLine($"Line: {lineNumber}");
                    sb.AppendLine();
                    sb.AppendLine($"Debug message: {debugMessage}");
                    if (debugObject != null)
                        sb.AppendLine($"Debug object: {debugObject}");
                }
                else
                {
                    sb.AppendLine($"Exception message: {exception.Message}");
                }

                if (RCF.Data.CurrentUserLevel < UserLevel.Advanced)
                    return sb.ToString();

                sb.AppendLine();
                sb.AppendLine($"Exception level: {exceptionLevel}");
                return sb.ToString();
            }
        }
    }
}