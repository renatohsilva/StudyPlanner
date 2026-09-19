using MediatR;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Api.Common;
using StudyPlanner.Application.Reviews.Queries.GetPendingReviews;

namespace StudyPlanner.Api.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController(ISender sender) : ControllerBase
{
    [HttpGet("pending")]
    public async Task<IActionResult> Pending([FromQuery] Guid? examId, CancellationToken cancellationToken)
    {
        var reviews = await sender.Send(new GetPendingReviewsQuery(User.GetUserId(), examId), cancellationToken);
        return Ok(reviews);
    }
}
