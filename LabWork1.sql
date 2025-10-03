USE master;
GO

IF DB_ID('ispp3512') IS NOT NULL
    DROP DATABASE ispp3512;
GO

CREATE DATABASE ispp3512;
GO

USE ispp3512;
GO

/* ---------- Справочные таблицы ---------- */

CREATE TABLE Roles (
    RoleId INT IDENTITY PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL,
    RoleDescription NVARCHAR(200) NULL
);

CREATE TABLE TaskStatuses (
    StatusId INT IDENTITY PRIMARY KEY,
    StatusName NVARCHAR(50) NOT NULL
);

/* ---------- Пользователи ---------- */

CREATE TABLE Users (
    UserId INT IDENTITY PRIMARY KEY,
    Login NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(200) NOT NULL,
    RoleId INT NOT NULL,
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);

/* ---------- Темы ---------- */

CREATE TABLE Topics (
    TopicId INT IDENTITY PRIMARY KEY,
    Topic NVARCHAR(100) NOT NULL,
    Description NVARCHAR(1000) NULL
);

/* ---------- Материалы ---------- */

CREATE TABLE Materials (
    MaterialId INT IDENTITY PRIMARY KEY,
    Title NVARCHAR(250) NOT NULL,
    MaterialType NVARCHAR(50) NOT NULL, -- text, video, audio, pdf ...
    UrlOrPath NVARCHAR(500) NULL,
    TextContent NVARCHAR(MAX) NULL
);

CREATE TABLE MaterialTopics (
    MaterialId INT NOT NULL,
    TopicId INT NOT NULL,
    PRIMARY KEY(MaterialId, TopicId),
    FOREIGN KEY (MaterialId) REFERENCES Materials(MaterialId) ON DELETE CASCADE,
    FOREIGN KEY (TopicId) REFERENCES Topics(TopicId) ON DELETE CASCADE
);

/* ---------- Задания ---------- */

CREATE TABLE Tasks (
    TaskId INT IDENTITY PRIMARY KEY,
    Title NVARCHAR(100) NOT NULL,
    Description NVARCHAR(1000) NULL,
    TopicId INT NOT NULL,
    FOREIGN KEY (TopicId) REFERENCES Topics(TopicId)
);

CREATE TABLE TaskStatusHistory (
    TaskStatusId INT IDENTITY PRIMARY KEY,
    UserId INT NOT NULL,
    TaskId INT NOT NULL,
    StatusId INT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (TaskId) REFERENCES Tasks(TaskId),
    FOREIGN KEY (StatusId) REFERENCES TaskStatuses(StatusId)
);

/* ---------- Тесты ---------- */

CREATE TABLE Tests (
    TestId INT IDENTITY PRIMARY KEY,
    TopicId INT NOT NULL,
    Title NVARCHAR(250) NOT NULL,
    Description NVARCHAR(1000) NULL,
    FOREIGN KEY (TopicId) REFERENCES Topics(TopicId)
);

CREATE TABLE TestQuestions (
    QuestionId INT IDENTITY PRIMARY KEY,
    TestId INT NOT NULL,
    QuestionText NVARCHAR(500) NOT NULL,
    FOREIGN KEY (TestId) REFERENCES Tests(TestId) ON DELETE CASCADE
);

CREATE TABLE TestOptions (
    OptionId INT IDENTITY PRIMARY KEY,
    QuestionId INT NOT NULL,
    OptionText NVARCHAR(250) NOT NULL,
    IsCorrect BIT NOT NULL,
    FOREIGN KEY (QuestionId) REFERENCES TestQuestions(QuestionId) ON DELETE CASCADE
);

CREATE TABLE TestResults (
    UserTestId INT IDENTITY PRIMARY KEY,
    UserId INT NOT NULL,
    TestId INT NOT NULL,
    Score DECIMAL(5,2) NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (TestId) REFERENCES Tests(TestId)
);

/* ---------- Данные ---------- */

-- Роли
INSERT INTO Roles (RoleName, RoleDescription) VALUES
(N'Администратор', N'Полный доступ'),
(N'Преподаватель', N'Создание материалов, заданий и тестов'),
(N'Студент', N'Доступ к обучению и тестам');

-- Статусы заданий
INSERT INTO TaskStatuses (StatusName) VALUES
(N'Не начато'),
(N'В процессе'),
(N'Завершено'),
(N'Сдано');

-- Пользователи (для примера пароли = HASH123 и т.п.)
INSERT INTO Users (Login, PasswordHash, RoleId) VALUES
(N'admin', N'HASH123', 1),
(N'teacher1', N'HASH456', 2),
(N'student1', N'HASH789', 3);

-- Темы
INSERT INTO Topics (Topic, Description) VALUES
(N'Кошки: общие сведения', N'История одомашнивания кошек, породы, особенности поведения'),
(N'Уход за кошками', N'Правильное питание, здоровье и содержание домашних кошек');

-- Материалы
INSERT INTO Materials (Title, MaterialType, UrlOrPath, TextContent) VALUES
(N'История кошек', N'text', NULL, N'Краткая история появления домашних кошек'),
(N'Видео: как ухаживать за котёнком', N'video', N'https://example.com/kitten_care.mp4', NULL);

INSERT INTO MaterialTopics (MaterialId, TopicId) VALUES
(1,1),
(2,2);

-- Задания
INSERT INTO Tasks (Title, Description, TopicId) VALUES
(N'Написать эссе о породах кошек', N'Опишите различия между сиамской и британской короткошёрстной кошкой', 1),
(N'Составить план ухода за котёнком', N'Опишите рацион и распорядок дня для котёнка', 2);

INSERT INTO TaskStatusHistory (UserId, TaskId, StatusId) VALUES
(3, 1, 2), 
(3, 2, 1); 

-- Тесты
INSERT INTO Tests (TopicId, Title, Description) VALUES
(1, N'Тест по истории кошек', N'Основные факты о происхождении и поведении кошек'),
(2, N'Тест по уходу за кошками', N'Знания о питании и здоровье кошек');

-- Вопросы
INSERT INTO TestQuestions (TestId, QuestionText) VALUES
(1, N'Откуда произошли домашние кошки?'),
(2, N'Сколько раз в день рекомендуется кормить взрослую кошку?');

-- Варианты ответов
INSERT INTO TestOptions (QuestionId, OptionText, IsCorrect) VALUES
-- Вопрос 1
(1, N'От африканской дикой кошки', 1),
(1, N'От собаки', 0),
(1, N'От тигра', 0),
(1, N'От лисы', 0),

-- Вопрос 2
(2, N'1 раз в день', 0),
(2, N'2 раза в день', 1),
(2, N'5–6 раз в день', 0),
(2, N'Раз в неделю', 0);

-- Результаты тестов
INSERT INTO TestResults (UserId, TestId, Score) VALUES
(3, 1, 90.00),
(3, 2, 70.00);
