namespace Northwind.API.Services
{
    public interface IAddEntity<T>
    {
        void AddEntity(int parentId, T childId);
    }
}