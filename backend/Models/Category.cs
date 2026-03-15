namespace TriviaApp.API.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int RoundId { get; set; }
    public int Order { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public Round Round { get; set; } = null!;
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
