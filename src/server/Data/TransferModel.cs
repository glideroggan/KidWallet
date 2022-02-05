using server.Data.User;

namespace server.Data;

public record TransferModel(UserModel SenderUser, int Funds, string Description);