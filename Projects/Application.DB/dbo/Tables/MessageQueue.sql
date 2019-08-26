CREATE TABLE [dbo].[MessageQueue] (
    [IdQueue]         INT            IDENTITY (1, 1) NOT NULL,
    [IdMessageType]   INT            CONSTRAINT [DF_MessagesQueue_IdMessageType] DEFAULT ((0)) NOT NULL,
    [Direction]       BIT            CONSTRAINT [DF_MessageQueue_Direction] DEFAULT ((0)) NOT NULL,
    [DateCreate]      DATETIME       CONSTRAINT [DF_MessageQueue_DateCreate] DEFAULT (getdate()) NOT NULL,
    [StateType]       TINYINT        CONSTRAINT [DF_MessageQueue_StateType] DEFAULT ((0)) NOT NULL,
    [State]           NVARCHAR (200) NULL,
    [IdTypeComponent] INT            NULL,
    [DateChange]      DATETIME       NULL,
    [MessageInfo]     NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_MessageQueue] PRIMARY KEY CLUSTERED ([IdQueue] ASC)
);



