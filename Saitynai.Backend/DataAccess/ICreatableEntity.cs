namespace Saitynai.Backend.DataAccess;

public interface ICreatableEntity : IEntity
{
    DateTime CreatedAt { get; set; }
}
