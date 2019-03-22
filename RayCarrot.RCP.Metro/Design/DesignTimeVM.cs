using System;
using System.Collections.ObjectModel;
using ByteSizeLib;
using MahApps.Metro.IconPacks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Design time view models to bind to
    /// </summary>
    public static class DesignTimeVM
    {
        public static ObservableCollection<GameBackupItemViewModel> GameBackupItems => new ObservableCollection<GameBackupItemViewModel>()
        {
            new GameBackupItemViewModel(Games.Rayman1)
            {
                BackupSize = new ByteSize(1000000),
                LastBackup = DateTime.Now
            },
            new GameBackupItemViewModel(Games.Rayman2),
            new GameBackupItemViewModel(Games.Rayman3),
            new GameBackupItemViewModel(Games.RaymanRavingRabbids),
            new GameBackupItemViewModel(Games.RaymanOrigins),
            new GameBackupItemViewModel(Games.RaymanFiestaRun),
        };

        public static AboutPageViewModel AboutPageViewModel => new AboutPageViewModel();

        public static GameDisplayViewModel GameDisplayViewModel => new GameDisplayViewModel("Rayman 2", Games.Rayman2.GetIconSource(), new ActionItemViewModel("Launch", PackIconMaterialKind.Play, null), new OverflowButtonItemViewModel[]{});
    }
}