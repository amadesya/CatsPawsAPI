USE master;
GO

IF DB_ID('ispp3512') IS NOT NULL
    DROP DATABASE ispp3512;
GO

CREATE DATABASE ispp3512;
GO

USE ispp3512;
GO

/* ---------- ���������� ������� ---------- */

CREATE TABLE Roles (
    RoleId INT IDENTITY PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL,
    RoleDescription NVARCHAR(200) NULL
);

CREATE TABLE TaskStatuses (
    StatusId INT IDENTITY PRIMARY KEY,
    StatusName NVARCHAR(50) NOT NULL
);

/* ---------- ������������ ---------- */

CREATE TABLE Users (
    UserId INT IDENTITY PRIMARY KEY,
    Login NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(200) NOT NULL,
    RoleId INT NOT NULL,
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);

/* ---------- ���� ---------- */

CREATE TABLE Topics (
    TopicId INT IDENTITY PRIMARY KEY,
    Topic NVARCHAR(100) NOT NULL,
    Description NVARCHAR(1000) NULL
);

/* ---------- ��������� ---------- */

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

/* ---------- ������� ---------- */

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

/* ---------- ����� ---------- */

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

/* ---------- ������ ---------- */

-- ����
INSERT INTO Roles (RoleName, RoleDescription) VALUES
(N'�������������', N'������ ������'),
(N'�������������', N'�������� ����������, ������� � ������'),
(N'�������', N'������ � �������� � ������');

-- ������� �������
INSERT INTO TaskStatuses (StatusName) VALUES
(N'�� ������'),
(N'� ��������'),
(N'���������'),
(N'�����');

-- ������������ (��� ������� ������ = HASH123 � �.�.)
INSERT INTO Users (Login, PasswordHash, RoleId) VALUES
(N'admin', N'HASH123', 1),
(N'teacher1', N'HASH456', 2),
(N'student1', N'HASH789', 3);

-- ����
INSERT INTO Topics (Topic, Description) VALUES
(N'�����: ����� ��������', N'������� ������������� �����, ������, ����������� ���������'),
(N'���� �� �������', N'���������� �������, �������� � ���������� �������� �����');

-- ���������
INSERT INTO Materials (Title, MaterialType, UrlOrPath, TextContent) VALUES
(N'������� �����', N'text', NULL, N'������� ������� ��������� �������� �����'),
(N'�����: ��� ��������� �� �������', N'video', N'https://example.com/kitten_care.mp4', NULL);

INSERT INTO MaterialTopics (MaterialId, TopicId) VALUES
(1,1),
(2,2);

-- �������
INSERT INTO Tasks (Title, Description, TopicId) VALUES
(N'�������� ���� � ������� �����', N'������� �������� ����� �������� � ���������� ��������������� ������', 1),
(N'��������� ���� ����� �� �������', N'������� ������ � ���������� ��� ��� ������', 2);

INSERT INTO TaskStatusHistory (UserId, TaskId, StatusId) VALUES
(3, 1, 2), 
(3, 2, 1); 

-- �����
INSERT INTO Tests (TopicId, Title, Description) VALUES
(1, N'���� �� ������� �����', N'�������� ����� � ������������� � ��������� �����'),
(2, N'���� �� ����� �� �������', N'������ � ������� � �������� �����');

-- �������
INSERT INTO TestQuestions (TestId, QuestionText) VALUES
(1, N'������ ��������� �������� �����?'),
(2, N'������� ��� � ���� ������������� ������� �������� �����?');

-- �������� �������
INSERT INTO TestOptions (QuestionId, OptionText, IsCorrect) VALUES
-- ������ 1
(1, N'�� ����������� ����� �����', 1),
(1, N'�� ������', 0),
(1, N'�� �����', 0),
(1, N'�� ����', 0),

-- ������ 2
(2, N'1 ��� � ����', 0),
(2, N'2 ���� � ����', 1),
(2, N'5�6 ��� � ����', 0),
(2, N'��� � ������', 0);

-- ���������� ������
INSERT INTO TestResults (UserId, TestId, Score) VALUES
(3, 1, 90.00),
(3, 2, 70.00);
