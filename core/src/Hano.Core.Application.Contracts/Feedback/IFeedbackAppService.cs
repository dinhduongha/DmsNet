using System;
using System.Threading.Tasks;
using Hano.Core.Application.Contracts.Feedback.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Hano.Core.Application.Contracts.Feedback;

/// <summary>
/// Inherits: GetAsync, GetListAsync, CreateAsync, UpdateAsync, DeleteAsync
/// from ICrudAppService. Custom create logic overrides base.
/// </summary>
public interface IFeedbackAppService
    : ICrudAppService<FeedbackDto, Guid, FeedbackFilterDto, CreateFeedbackDto, UpdateFeedbackDto>
{
}
