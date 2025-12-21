namespace TriviaApp.API.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int RoundId { get; set; }
    public int Order { get; set; }
    public List<QuestionDto> Questions { get; set; } = new();
}

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public int RoundId { get; set; }
}

public class UpdateCategoryDto
{
    public string Name { get; set; } = string.Empty;
}

