using MediatR;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;
using System.Text.Json.Serialization;

namespace MusicInteraction.Application;

// Command for getting all ratings
public class GetRatingsCommand: IRequest<GetRatingsResult> { }

// Command for getting a rating by ID
public class GetRatingByIdCommand : IRequest<GetRatingDetailResult>
{
    public Guid RatingId { get; set; }
}

public class GetRatingsResult
{
    public bool RatingsEmpty { get; set; }
    public List<RatingOverviewDTO> Ratings { get; set; }
}

public class GetRatingDetailResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public RatingDetailDTO Rating { get; set; }
}

// Brief overview DTO
public class RatingOverviewDTO
{
    public Guid RatingId { get; set; }
    public float? NormalizedGrade { get; set; }
    public float? Grade { get; set; }
    public float? MinGrade { get; set; }
    public float? MaxGrade { get; set; }
}

// Base component DTO
[JsonPolymorphic(TypeDiscriminatorPropertyName = "componentType")]
[JsonDerivedType(typeof(GradeDetailDTO), typeDiscriminator: "grade")]
[JsonDerivedType(typeof(BlockDetailDTO), typeDiscriminator: "block")]
public abstract class GradableComponentDTO
{
    public string Name { get; set; }
    public float? CurrentGrade { get; set; }
    public float MinPossibleGrade { get; set; }
    public float MaxPossibleGrade { get; set; }
}

// Grade component details
public class GradeDetailDTO : GradableComponentDTO
{
    public float StepAmount { get; set; }
}

// Block component details with nested components
public class BlockDetailDTO : GradableComponentDTO
{
    public List<GradableComponentDTO> Components { get; set; }
    public List<string> Actions { get; set; }
}

// Detailed DTO for showing the full rating structure
public class RatingDetailDTO
{
    public Guid RatingId { get; set; }
    public string ItemId { get; set; }
    public string ItemType { get; set; }
    public string UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public float? NormalizedGrade { get; set; }
    public float? OverallGrade { get; set; }
    public float MinPossibleGrade { get; set; }
    public float MaxPossibleGrade { get; set; }
    public GradableComponentDTO GradingComponent { get; set; }
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

public class GetRatingByIdUseCase : IRequestHandler<GetRatingByIdCommand, GetRatingDetailResult>
{
    private readonly IInteractionStorage interactionStorage;

    public GetRatingByIdUseCase(IInteractionStorage interactionStorage)
    {
        this.interactionStorage = interactionStorage;
    }

    public async Task<GetRatingDetailResult> Handle(GetRatingByIdCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (await interactionStorage.IsEmpty())
            {
                return new GetRatingDetailResult
                {
                    Success = false,
                    ErrorMessage = "No ratings found"
                };
            }

            List<Rating> ratings = await interactionStorage.GetRatings();

            // Find the requested rating
            Rating rating = ratings.FirstOrDefault(r => r.RatingId == request.RatingId);

            if (rating == null)
            {
                return new GetRatingDetailResult
                {
                    Success = false,
                    ErrorMessage = $"Rating with ID {request.RatingId} not found"
                };
            }

            // Create detailed DTO
            var ratingDetail = new RatingDetailDTO
            {
                RatingId = rating.RatingId,
                ItemId = rating.ItemId,
                ItemType = rating.ItemType,
                UserId = rating.UserId,
                CreatedAt = rating.CreatedAt,
                NormalizedGrade = rating.Grade.getNormalizedGrade(),
                OverallGrade = rating.GetGrade(),
                MinPossibleGrade = rating.Grade.getMin(),
                MaxPossibleGrade = rating.Grade.getMax(),
                GradingComponent = ConvertComponentToDto(rating.Grade)
            };

            return new GetRatingDetailResult
            {
                Success = true,
                Rating = ratingDetail
            };
        }
        catch (Exception ex)
        {
            return new GetRatingDetailResult
            {
                Success = false,
                ErrorMessage = $"Error retrieving rating: {ex.Message}"
            };
        }
    }

    // Recursive method to convert components to DTOs
    private GradableComponentDTO ConvertComponentToDto(IGradable component)
    {
        if (component is Grade grade)
        {
            return new GradeDetailDTO
            {
                Name = grade.parametrName,
                CurrentGrade = grade.getGrade(),
                MinPossibleGrade = grade.getMin(),
                MaxPossibleGrade = grade.getMax(),
                StepAmount = grade.stepAmount
            };
        }
        else if (component is GradingBlock block)
        {
            var blockDto = new BlockDetailDTO
            {
                Name = block.BlockName,
                CurrentGrade = block.getGrade(),
                MinPossibleGrade = block.getMin(),
                MaxPossibleGrade = block.getMax(),
                Components = new List<GradableComponentDTO>(),
                Actions = ConvertActionsToStrings(block.Actions)
            };

            // Convert all sub-components
            foreach (var subComponent in block.Grades)
            {
                blockDto.Components.Add(ConvertComponentToDto(subComponent));
            }

            return blockDto;
        }

        throw new InvalidOperationException($"Unknown component type: {component.GetType().Name}");
    }

    private List<string> ConvertActionsToStrings(List<Domain.Action> actions)
    {
        var result = new List<string>();
        foreach (var action in actions)
        {
            switch (action)
            {
                case Domain.Action.Add:
                    result.Add("Add");
                    break;
                case Domain.Action.Subtract:
                    result.Add("Subtract");
                    break;
                case Domain.Action.Multiply:
                    result.Add("Multiply");
                    break;
                case Domain.Action.Divide:
                    result.Add("Divide");
                    break;
                default:
                    result.Add("Unknown");
                    break;
            }
        }
        return result;
    }
}