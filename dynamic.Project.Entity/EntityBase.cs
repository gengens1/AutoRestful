namespace dynamic.Project.Entity
{
    public class EntityBase : IEntityBase
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public DateTime UpdateAt { get; set; } = DateTime.Now;

        public void OnDelete()
        {
        }

        public void OnFind()
        {
        }

        public void OnInsert()
        {
        }

        public void OnUpdate()
        {
            UpdateAt = DateTime.Now;
        }
    }
}
