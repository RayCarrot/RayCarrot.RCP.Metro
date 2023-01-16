using RayCarrot.RCP.Metro.Games.Data;

namespace RayCarrot.RCP.Metro.Games.Components;

public class Ray1MsDosAdditionalLaunchActionsComponent : AdditionalLaunchActionsComponent
{
    public Ray1MsDosAdditionalLaunchActionsComponent() : base(GetAdditionalLaunchActions) { }

    private static IEnumerable<ActionItemViewModel> GetAdditionalLaunchActions(GameInstallation gameInstallation)
    {
        // Add a lunch action for each version
        Ray1MsDosData? data = gameInstallation.GetObject<Ray1MsDosData>(GameDataKey.Ray1_MsDosData);

        if (data == null)
            return Enumerable.Empty<ActionItemViewModel>();

        Ray1MsDosData.Version[] versions = data.AvailableVersions;

        // Only show additional launch actions for the game if we have more than one version
        if (versions.Length <= 1)
            return Enumerable.Empty<ActionItemViewModel>();

        return versions.Select(x =>
            new IconCommandItemViewModel(
                header: x.DisplayName,
                description: x.Id,
                iconKind: GenericIconKind.GameAction_Play,
                command: new AsyncRelayCommand(async () =>
                {
                    if (gameInstallation.GetComponent<LaunchGameComponent>() is not DosBoxLaunchGameComponent c)
                        return;

                    string args = Ray1LaunchArgumentsComponent.GetLaunchArgs(x.Id);
                    bool success = await c.LaunchGameAsync(args);

                    if (success)
                        await gameInstallation.GetComponents<OnGameLaunchedComponent>().InvokeAllAsync();
                })));
    }
}