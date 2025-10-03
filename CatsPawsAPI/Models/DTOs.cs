// Для отправки вопросов и вариантов клиенту
public class TestDto
{
    public int TestId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public List<QuestionDto> Questions { get; set; }
}

public class QuestionDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; }
    public List<OptionDto> Options { get; set; }
}

public class OptionDto
{
    public int OptionId { get; set; }
    public string OptionText { get; set; }
}

// Для отправки ответов
public class TestSubmissionDto
{
    public int UserId { get; set; }
    public int TestId { get; set; }
    public List<int> SelectedOptionIds { get; set; } // ID выбранных вариантов
}

// Для ответа после проверки
public class TestResultDto
{
    public int UserId { get; set; }
    public int TestId { get; set; }
    public decimal Score { get; set; }
}
