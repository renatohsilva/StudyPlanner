using MediatR;
using Microsoft.AspNetCore.Mvc;
using StudyPlanner.Application.Reviews.Queries.GetPendingReviews;

namespace StudyPlanner.Api.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController(ISender sender) : ControllerBase
{
    [HttpGet("pending")]
    public async Task<IActionResult> Pending([FromQuery] Guid userId, [FromQuery] Guid? examId, CancellationToken cancellationToken)
    {
        var reviews = await sender.Send(new GetPendingReviewsQuery(userId, examId), cancellationToken);
        return Ok(reviews);
    }
}
