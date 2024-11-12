using API.Models.Database;

namespace API.Services.Interfaces;

public interface ICachedCategorizationIssueService
    : ICachedRepository<CategorizationIssue, string>,
        ICategorizationIssueService { }