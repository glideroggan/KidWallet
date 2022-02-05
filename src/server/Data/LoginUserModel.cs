using server.Data.User;

namespace server.Data
{
	public record LoginUserModel(string Username, string ImageUrl, RoleEnum Role);
}
