using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using NLog;

namespace RayCarrot.RCP.Metro;

public class LaunchArguments
{
    #region Constructor

    public LaunchArguments(string[]? args)
    {
        args ??= Array.Empty<string>();
        Args = args;

        if (args.Length > 0 && File.Exists(args[0]))
            FilePathArg = args[0];
    }

    #endregion

    #region Logger

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Private Constant Fields

    private const string PipeName = "RCP_Metro.Args";

    #endregion

    #region Private Fields

    private CancellationTokenSource? _cancellationTokenSource; // Dispose?

    #endregion

    #region Public Properties

    /// <summary>
    /// The launch arguments
    /// </summary>
    public string[] Args { get; }

    /// <summary>
    /// The file path launch argument if one is available. This only specifies the
    /// path if it's the first argument which it will be by default if the program
    /// is opened from a file.
    /// </summary>
    public FileSystemPath? FilePathArg { get; }

    /// <summary>
    /// Indicates if there are any launch arguments available
    /// </summary>
    public bool HasArgs => Args.Length > 0;

    #endregion

    #region Public Methods

    public bool HasArg(string arg) => Array.IndexOf(Args, arg) != -1;
    
    public bool HasArg(string arg, [NotNullWhen(true)]out string? value)
    {
        int index = Array.IndexOf(Args, arg);

        if (index != -1)
        {
            value = Args[index + 1];
            return true;
        }
        else
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Send the launch arguments to another instance
    /// </summary>
    public void SendArguments()
    {
        const string serverName = "."; // This specifies the local computer
        using NamedPipeClientStream pipeClient = new(serverName, PipeName, PipeDirection.Out);
        using StreamWriter writer = new(pipeClient);

        pipeClient.Connect(5000); // 5 second timeout. Probably not needed since it's local...

        // Send the arguments as string lines
        foreach (string arg in Args)
            writer.WriteLine(arg);
    }

    /// <summary>
    /// Starts watching for launch arguments sent from another instance
    /// </summary>
    public async void StartReceiveArguments()
    {
        try
        {
            StopReceiveArguments();
            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = _cancellationTokenSource.Token;

            while (true)
            {
                // Create the server with the matching pipe name
                using NamedPipeServerStream pipeServer = new(PipeName, PipeDirection.In);

                Logger.Info("Start watching for recieved launch arguments from another instance");

                // Wait for a connection
                await pipeServer.WaitForConnectionAsync(token);

                // Throw if cancelled
                token.ThrowIfCancellationRequested();

                Logger.Info("A connection has been established");

                // Create a reader to read the arguments
                using StreamReader reader = new(pipeServer);

                // Read the arguments (one per line)
                List<string> lines = new();
                while (true)
                {
                    string? line = reader.ReadLine();

                    if (line == null)
                        break;

                    lines.Add(line);
                }

                Logger.Info("Read {0} arguments", lines.Count);
                Logger.Trace("Launch arguments: {0}", String.Join(", ", lines));

                // Create a new launch arguments instance
                LaunchArguments newArgs = new(lines.ToArray());

                // NOTE: Right now we're just looking at the file path that gets passed in,
                // but we could expand this to check other arguments as well
                if (newArgs.FilePathArg != null)
                {
                    FileLaunchHandler? fileLaunchHandler =
                        FileLaunchHandler.GetHandler(newArgs.FilePathArg.Value);
                    fileLaunchHandler?.Invoke(newArgs.FilePathArg.Value,
                        FileLaunchHandler.State.Running);
                }

                Logger.Info("Handled launch arguments");
            }
        }
        catch (OperationCanceledException ex)
        {
            Logger.Info(ex, "Receiving launch arguments");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Receiving launch arguments");
        }
        finally
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    /// <summary>
    /// Stops watching for launch arguments started with <see cref="StartReceiveArguments"/>
    /// </summary>
    public void StopReceiveArguments()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }

    #endregion
}