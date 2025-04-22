namespace MusicInteraction.Infrastructure.Services;

public class ReviewHotScoreCalculator
{
    // Default weight constants
    private readonly float _likeWeight = 1.0f;
    private readonly float _commentWeight = 2.0f;
    private readonly float _timeConstant = 2.0f;
    private readonly float _gravity = 1.5f;
    private readonly int _maxAgeDays = 30;

    // Constructor with default values
    public ReviewHotScoreCalculator()
    { }

    // Constructor with custom weight values
    public ReviewHotScoreCalculator(float likeWeight, float commentWeight, float timeConstant, float gravity, int maxAgeDays)
    {
        _likeWeight = likeWeight;
        _commentWeight = commentWeight;
        _timeConstant = timeConstant;
        _gravity = gravity;
        _maxAgeDays = maxAgeDays;
    }

    public float CalculateHotScore(int likes, int comments, DateTime createdAt)
    {
        // Calculate age in days (capped at maxAgeDays)
        double ageDays = Math.Min((DateTime.UtcNow - createdAt).TotalDays, _maxAgeDays);

        // Calculate raw score: wL * L + wC * C
        float rawScore = (_likeWeight * likes) + (_commentWeight * comments);

        // Calculate denominator: (t + T0)^G
        double denominator = Math.Pow(ageDays + _timeConstant, _gravity);

        // Final score: RawScore / (t + T0)^G
        float hotScore = (float)(rawScore / denominator);

        return hotScore;
    }
}