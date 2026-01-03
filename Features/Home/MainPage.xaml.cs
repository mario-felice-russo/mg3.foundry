

namespace mg3.foundry.Features.Home;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;

    public MainPage(MainViewModel viewModel)
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
}
