

namespace mg3.foundry.Features.Chat;

public partial class ChatPage : ContentPage
{
    private readonly ChatViewModel _viewModel;

    public ChatPage(ChatViewModel viewModel)
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

    private async void OnCopyMessageClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string text)
        {
            await Clipboard.SetTextAsync(text);
            
            // Optional: Show a brief confirmation
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Copied", "Message copied to clipboard", "OK");
            }
        }
    }
}
