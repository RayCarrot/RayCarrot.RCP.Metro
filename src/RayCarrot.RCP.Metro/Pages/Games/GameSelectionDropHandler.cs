using GongSolutions.Wpf.DragDrop;

namespace RayCarrot.RCP.Metro.Pages.Games;

public class GameSelectionDropHandler : DefaultDropHandler
{
    public GameSelectionDropHandler(GamesPageViewModel viewModel)
    {
        ViewModel = viewModel;
    }

    public GamesPageViewModel ViewModel { get; }

    public override void DragOver(IDropInfo dropInfo)
    {
        if (dropInfo.Data is not InstalledGameViewModel game)
            return;

        if (ViewModel.GroupGames && dropInfo.TargetGroup?.Name != game.GameGroup)
            return;

        base.DragOver(dropInfo);
    }

    public override void Drop(IDropInfo? dropInfo)
    {
        if (dropInfo?.DragInfo == null || 
            dropInfo.Data is not InstalledGameViewModel game)
            return;

        int insertIndex = GetInsertIndex(dropInfo);
        int objIndex = ViewModel.Games.IndexOf(game);

        if (insertIndex > objIndex)
            insertIndex--;

        Services.Games.MoveGame(objIndex, insertIndex);

        // Don't call base since the view model will update the
        // collection when we perform the move
        //base.Drop(dropInfo);
    }
}