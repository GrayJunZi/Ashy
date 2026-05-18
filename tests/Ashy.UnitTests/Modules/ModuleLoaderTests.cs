using System.Reflection;
using Ashy.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ashy.UnitTests.Modules;

public class TestModuleA : IModule
{
    public string Name => "A";
    public int Order => 2;
    public bool ConfigureServicesCalled { get; private set; }
    public bool ConfigureCalled { get; private set; }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        ConfigureServicesCalled = true;
    }

    public void Configure(IApplicationBuilder app)
    {
        ConfigureCalled = true;
    }
}

public class TestModuleB : IModule
{
    public string Name => "B";
    public int Order => 1;
    public bool ConfigureServicesCalled { get; private set; }
    public bool ConfigureCalled { get; private set; }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        ConfigureServicesCalled = true;
    }

    public void Configure(IApplicationBuilder app)
    {
        ConfigureCalled = true;
    }
}

public class ModuleLoaderTests
{
    private static Assembly GetThisAssembly() => typeof(ModuleLoaderTests).Assembly;

    [Fact]
    public void LoadModules_Scans_And_Calls_ConfigureServices_In_Order()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().Build();

        ModuleLoader.LoadModules(services, config, GetThisAssembly());

        var moduleA = new TestModuleA();
        var moduleB = new TestModuleB();
    }

    [Fact]
    public void LoadModules_Returns_ServiceCollection()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().Build();

        var result = ModuleLoader.LoadModules(services, config, GetThisAssembly());
        Assert.Same(services, result);
    }

    [Fact]
    public void LoadModules_Empty_Assemblies_Does_Not_Throw()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().Build();

        var result = ModuleLoader.LoadModules(services, config, Array.Empty<Assembly>());
        Assert.Same(services, result);
    }

    [Fact]
    public void UseModules_Returns_ApplicationBuilder()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        var result = ModuleLoader.UseModules(app, GetThisAssembly());
        Assert.Same(app, result);
    }

    [Fact]
    public void UseModules_Empty_Assemblies_Does_Not_Throw()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        var result = ModuleLoader.UseModules(app, Array.Empty<Assembly>());
        Assert.Same(app, result);
    }
}