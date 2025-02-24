namespace MusicInteraction.Domain;

public interface IGradable
{
    public float? getGrade();
    public float getMax();
    public void updateGrade(float grade);
}