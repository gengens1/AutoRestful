namespace dynamic.Project.Entity.Plugins
{
    /// <summary>
    /// 插件
    /// </summary>
    public interface IEntityBasePlugin<T>
    {
        void OnFind(T t);
        void OnCreate(T t);
        void OnUpdate(T t);
        void OnDelete(T t);
    }
}
