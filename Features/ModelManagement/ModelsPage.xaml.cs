

namespace mg3.foundry.Features.ModelManagement;

public partial class ModelsPage : ContentPage
{
    private readonly ModelsViewModel _viewModel;

    public ModelsPage(ModelsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel.InitializeCommand.CanExecute(null))
        {
            await _viewModel.InitializeCommand.ExecuteAsync(null);
        }
    }

    private void OnAvailableTabClicked(object sender, EventArgs e)
    {
        AvailableModelsView.IsVisible = true;
        CachedModelsView.IsVisible = false;
    }

    private void OnCachedTabClicked(object sender, EventArgs e)
    {
        AvailableModelsView.IsVisible = false;
        CachedModelsView.IsVisible = true;
    }
}
