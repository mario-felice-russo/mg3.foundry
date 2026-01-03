namespace mg3.foundry;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("MainPage", typeof(Features.Home.MainPage));
        Routing.RegisterRoute("ChatPage", typeof(Features.Chat.ChatPage));
        Routing.RegisterRoute("ModelsPage", typeof(Features.ModelManagement.ModelsPage));
    }
}
