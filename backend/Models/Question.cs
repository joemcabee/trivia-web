namespace TriviaApp.API.Models;

public class Question
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public int Order { get; set; }
    
    // Navigation properties
    public Category Category { get; set; } = null!;
}

