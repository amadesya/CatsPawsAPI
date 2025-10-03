using System;
using System.Collections.Generic;

#nullable disable

// ---------- Роли ----------
public class Role
{
    public int RoleId { get; set; }
    public string RoleName { get; set; }
    public string RoleDescription { get; set; }

    public ICollection<User> Users { get; set; }
}

// ---------- Пользователи ----------
public class User
{
    public int UserId { get; set; }
    public string Login { get; set; }
    public string PasswordHash { get; set; }

    public int RoleId { get; set; }
    public Role Role { get; set; }

    public ICollection<TaskStatusHistory> TaskStatusHistory { get; set; }
    public ICollection<TestResult> TestResults { get; set; }
}

// ---------- Темы ----------
public class Topic
{
    public int TopicId { get; set; }
    public string TopicName { get; set; }
    public string Description { get; set; }

    public ICollection<MaterialTopic> MaterialTopics { get; set; }
    public ICollection<Task> Tasks { get; set; }
    public ICollection<Test> Tests { get; set; }
}

// ---------- Материалы ----------
public class Material
{
    public int MaterialId { get; set; }
    public string Title { get; set; }
    public string MaterialType { get; set; } // text, video, pdf ...
    public string UrlOrPath { get; set; }
    public string TextContent { get; set; }

    public ICollection<MaterialTopic> MaterialTopics { get; set; }
}

public class MaterialTopic
{
    public int MaterialId { get; set; }
    public Material Material { get; set; }

    public int TopicId { get; set; }
    public Topic Topic { get; set; }
}

// ---------- Задания ----------
public class Task
{
    public int TaskId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }

    public int TopicId { get; set; }
    public Topic Topic { get; set; }

    public ICollection<TaskStatusHistory> TaskStatusHistory { get; set; }
}

public class TaskStatus
{
    public int StatusId { get; set; }  
    public string StatusName { get; set; }

    public ICollection<TaskStatusHistory> TaskStatusHistory { get; set; }
}

public class TaskStatusHistory
{
    public int TaskStatusId { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public int TaskId { get; set; }
    public Task Task { get; set; }

    public int StatusId { get; set; }
    public TaskStatus Status { get; set; }
}

// ---------- Тесты ----------
public class Test
{
    public int TestId { get; set; }
    public int TopicId { get; set; }
    public Topic Topic { get; set; }

    public string Title { get; set; }
    public string Description { get; set; }

    public ICollection<TestQuestion> TestQuestions { get; set; }
    public ICollection<TestResult> TestResults { get; set; }
}

public class TestQuestion
{
    public int QuestionId { get; set; }
    public int TestId { get; set; }
    public Test Test { get; set; }

    public string QuestionText { get; set; }
    public ICollection<TestOption> TestOptions { get; set; }
}

public class TestOption
{
    public int OptionId { get; set; }
    public int QuestionId { get; set; }
    public TestQuestion Question { get; set; }

    public string OptionText { get; set; }
    public bool IsCorrect { get; set; }
}

public class TestResult
{
    public int UserTestId { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public int TestId { get; set; }
    public Test Test { get; set; }

    public decimal Score { get; set; }
}
