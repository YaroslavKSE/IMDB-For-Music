using MediatR;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;
namespace MusicInteraction.Application;

public class GetRatingsCommand: IRequest<GetRatingsResult> { }

public class GetRatingsResult
{
    public bool RatingsEmpty { get; set; }
    public List<RatingOverviewDTO> Ratings { get; set; }
}

public class RatingOverviewDTO
{
    public Guid RatingId { get; set; }
    public float? NormalizedGrade { get; set; }
    public float? Grade { get; set; }
    public float? MinGrade { get; set; }
    public float? MaxGrade { get; set; }
}

public class GetRatingsUseCase : IRequestHandler<GetRatingsCommand, GetRatingsResult>
{
    private readonly IInteractionStorage interactionStorage;

    public GetRatingsUseCase(IInteractionStorage interactionStorage)
    {
        this.interactionStorage = interactionStorage;
    }

    public async Task<GetRatingsResult> Handle(GetRatingsCommand request, CancellationToken cancellationToken)
    {
        if (await interactionStorage.IsEmpty())
        {
            return new GetRatingsResult() {RatingsEmpty = true};
        }

        List<Rating> ratings = interactionStorage.GetRatings().Result;
        List<RatingOverviewDTO> ratingsDTOs = new List<RatingOverviewDTO>();
        foreach (var rating in ratings)
        {
            RatingOverviewDTO ratingDTO = new RatingOverviewDTO()
            {
                Grade = rating.GetGrade(), MaxGrade = rating.GetMax(), MinGrade = rating.GetMin(),
                NormalizedGrade = rating.Grade.getNormalizedGrade(), RatingId = rating.RatingId
            };
            ratingsDTOs.Add(ratingDTO);
        }
        return new GetRatingsResult() {RatingsEmpty = false, Ratings = ratingsDTOs};
    }
}