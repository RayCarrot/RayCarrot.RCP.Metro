﻿using RayCarrot.WPF;
using System;
using System.Linq;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using RayCarrot.Common;
using RayCarrot.Logging;

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
        /// <param name="origin">The caller member name (leave at default for compiler-time value)</param>
        /// <param name="filePath">The caller file path (leave at default for compiler-time value)</param>
        /// <param name="lineNumber">The caller line number (leave at default for compiler-time value)</param>
        public override void HandleException(Exception exception, string debugMessage, ExceptionLevel exceptionLevel, object debugObject = null, [CallerMemberName] string origin = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            // Call base implementation
            base.HandleException(exception, debugMessage, exceptionLevel, debugObject, origin, filePath, lineNumber);
            
            try
            {
                if (RCPServices.Data.DisplayExceptionLevel <= exceptionLevel)
                    MessageBox.Show(GetMessage(), Resources.ExceptionMessageHeader, MessageBoxButton.OK, MessageBoxImage.Error);
            }
#pragma warning disable 168
            catch (Exception ex)
#pragma warning restore 168
            {
                Debugger.Break();
            }

            // Helper method for getting the message to display
            string GetMessage()
            {
                var sb = new StringBuilder();
                
                sb.AppendLine(Resources.ExceptionMessageInfo);
                sb.AppendLine();

                if (Services.Data.CurrentUserLevel >= UserLevel.Debug)
                {
                    sb.AppendLine($"Exception: {exception}");

                    if (exception.Data.Any())
                    {
                        sb.AppendLine();
                        sb.AppendLine($"Data: {exception.Data.Values.Cast<object>().JoinItems(", ")}");
                    }

                    sb.AppendLine();
                    sb.AppendLine($"Origin: {origin}  -  File: {filePath}  -  Line: {lineNumber}");
                    sb.AppendLine();
                    sb.AppendLine($"Debug message: {debugMessage}");
                    if (debugObject != null)
                        sb.AppendLine($"Debug object: {debugObject}");
                }
                else
                {
                    sb.AppendLine($"Exception message: {exception.Message}");
                }

                if (Services.Data.CurrentUserLevel < UserLevel.Advanced)
                    return sb.ToString();

                sb.AppendLine();
                sb.AppendLine($"Exception level: {exceptionLevel}");

                return sb.ToString();
            }
        }
    }
}