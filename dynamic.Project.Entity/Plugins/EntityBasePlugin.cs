namespace dynamic.Project.Entity.Plugins
{
    /// <summary>
    /// 实体插件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EntityBasePlugin<T> : IEntityBasePlugin<T> where T : EntityBase
    {
        public void OnCreate(T t)
        {
        }

        public void OnDelete(T t)
        {
        }

        public void OnFind(T t)
        {
        }

        public void OnUpdate(T t)
        {
        }
    }
}
