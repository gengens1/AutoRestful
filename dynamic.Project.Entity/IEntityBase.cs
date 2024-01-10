namespace dynamic.Project.Entity
{
    public interface IEntityBase
    {
        void OnInsert();
        void OnUpdate();
        void OnFind();
        void OnDelete();
    }
}
