namespace Ashy.Modules;

/// <summary>
/// 模块化插件接口，各功能包实现此接口以自动注册
/// </summary>
public interface IModule
{
    /// <summary>
    /// 模块名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 加载顺序，越小越先加载
    /// </summary>
    int Order => 0;
}