﻿#nullable disable
using System.Windows.Controls;

namespace RayCarrot.RCP.Metro;

/// <summary>
/// Interaction logic for GameOptions_DOSBox_Control.xaml
/// </summary>
public partial class GameOptions_DOSBox_Control : UserControl
{
    public GameOptions_DOSBox_Control(GameInstallation gameInstallation)
    {
        InitializeComponent();

        ViewModel = new GameOptions_DOSBox_ViewModel(gameInstallation);
        DataContext = ViewModel;
    }

    public GameOptions_DOSBox_ViewModel ViewModel { get; }
}