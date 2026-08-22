using System;
using HandyControlDemo.Service;
using Microsoft.Extensions.DependencyInjection;

namespace HandyControlDemo.ViewModel;

public class ViewModelLocator
{
    private static readonly Lazy<ViewModelLocator> InstanceInternal =
        new(() => new ViewModelLocator(), isThreadSafe: true);

    public static ViewModelLocator Instance => InstanceInternal.Value;

    private readonly IServiceProvider _serviceProvider;

    private ViewModelLocator()
    {
        var services = new ServiceCollection();

        services.AddSingleton<DataService>();
        services.AddSingleton<MainViewModel>();
        services.AddTransient<InputElementDemoViewModel>();
        services.AddTransient<CardDemoViewModel>();
        services.AddTransient<DialogDemoViewModel>();
        services.AddTransient<CoverViewModel>();
        services.AddTransient<CarouselViewModel>();
        services.AddTransient<ColorPickerViewModel>();
        services.AddTransient<SideMenuViewModel>();
        services.AddTransient<TransferViewModel>();
        services.AddTransient<PropertyGridViewModel>();
        services.AddTransient<GrowlDemoViewModel>();

        _serviceProvider = services.BuildServiceProvider();
    }

    public MainViewModel Main => _serviceProvider.GetService<MainViewModel>()!;

    public InputElementDemoViewModel InputElementDemo => _serviceProvider.GetService<InputElementDemoViewModel>()!;

    public CardDemoViewModel CardDemo => _serviceProvider.GetService<CardDemoViewModel>()!;

    public DialogDemoViewModel DialogDemo => _serviceProvider.GetService<DialogDemoViewModel>()!;

    public CoverViewModel CoverView => _serviceProvider.GetService<CoverViewModel>()!;

    public CarouselViewModel Carousel => _serviceProvider.GetService<CarouselViewModel>()!;

    public ColorPickerViewModel ColorPicker => _serviceProvider.GetService<ColorPickerViewModel>()!;

    public SideMenuViewModel SideMenu => _serviceProvider.GetService<SideMenuViewModel>()!;

    public TransferViewModel Transfer => _serviceProvider.GetService<TransferViewModel>()!;

    public PropertyGridViewModel PropertyGrid => _serviceProvider.GetService<PropertyGridViewModel>()!;

    public GrowlDemoViewModel GrowlDemo => _serviceProvider.GetService<GrowlDemoViewModel>()!;
}
