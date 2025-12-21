namespace TriviaApp.API.DTOs;

public class QuestionDto
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public int Order { get; set; }
}

public class CreateQuestionDto
{
    public string QuestionText { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
}

public class UpdateQuestionDto
{
    public string QuestionText { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}

