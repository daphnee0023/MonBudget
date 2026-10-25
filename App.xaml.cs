namespace MonBudget;

public partial class App : Application
{
    public App()
{
    InitializeComponent();
    UserAppTheme = AppTheme.Light;
}

    protected override Window CreateWindow(IActivationState? activationState)
        => new Window(new AppShell());
}
