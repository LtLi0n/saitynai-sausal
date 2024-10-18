using Saitynai.Backend.Contracts.Models;

namespace Saitynai.Backend.DataAccess;

public interface IOwnableEntity
{
	Guid? OwnerId { get; set; }
	User Owner { get; set; }
}
