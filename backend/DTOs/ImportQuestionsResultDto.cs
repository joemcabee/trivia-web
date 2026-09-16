namespace TriviaApp.API.DTOs;

public class ImportQuestionsResultDto
{
    public int TotalRows { get; set; }
    public int ImportedQuestions { get; set; }
    public int CreatedRounds { get; set; }
    public int CreatedCategories { get; set; }
    public int SkippedRows { get; set; }
    public List<ImportRowErrorDto> Errors { get; set; } = new();
}

public class ImportRowErrorDto
{
    public int RowNumber { get; set; }
    public string Message { get; set; } = string.Empty;
}

