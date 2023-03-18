using GongSolutions.Wpf.DragDrop;

namespace RayCarrot.RCP.Metro;

public class GameClientsSetupDropHandler : DefaultDropHandler
{
    public GameClientsSetupDropHandler(GameClientsSetupViewModel viewModel)
    {
        ViewModel = viewModel;
    }

    public GameClientsSetupViewModel ViewModel { get; }

    public override void DragOver(IDropInfo dropInfo)
    {
        if (dropInfo.Data is not InstalledGameClientViewModel)
            return;

        base.DragOver(dropInfo);
    }

    public override void Drop(IDropInfo? dropInfo)
    {
        if (dropInfo?.DragInfo == null || 
            dropInfo.Data is not InstalledGameClientViewModel gameClient)
            return;

        int insertIndex = GetInsertIndex(dropInfo);
        int objIndex = ViewModel.InstalledGameClients.IndexOf(gameClient);

        if (insertIndex > objIndex)
            insertIndex--;

        Services.GameClients.MoveGameClient(objIndex, insertIndex);

        // Don't call base since the view model will update the
        // collection when we perform the move
        //base.Drop(dropInfo);
    }
}