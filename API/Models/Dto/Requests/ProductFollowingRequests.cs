namespace API.Models.Dto.Requests;

public abstract class BaseProductFollowingRequest
{
    public string ProductId { get; init; } = string.Empty;
}

public class CreateProductFollowingRequest : BaseProductFollowingRequest
{
}

public class DeleteProductFollowingRequest : BaseProductFollowingRequest
{
}