USE [fgSourceDb]
GO
/****** Object:  Table [dbo].[COMPUTER]    Script Date: 7/20/2013 5:15:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[COMPUTER](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[NAME] [nvarchar](25) NOT NULL,
 CONSTRAINT [PK_COMPUTER] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_COMPUTER] UNIQUE NONCLUSTERED 
(
	[NAME] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DOMAIN]    Script Date: 7/20/2013 5:15:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DOMAIN](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[NAME] [varchar](256) NOT NULL,
 CONSTRAINT [PK_DOMAIN] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_DOMAIN] UNIQUE NONCLUSTERED 
(
	[NAME] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[EXTENSION]    Script Date: 7/20/2013 5:15:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EXTENSION](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[NAME] [nvarchar](256) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_EXTENSION] UNIQUE NONCLUSTERED 
(
	[NAME] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[EXTENSION_FILTER]    Script Date: 7/20/2013 5:15:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EXTENSION_FILTER](
	[USER_GROUP_ID] [bigint] NOT NULL,
	[EXTENSION_ID] [bigint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[USER_GROUP_ID] ASC,
	[EXTENSION_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[FILE]    Script Date: 7/20/2013 5:15:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FILE](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[NAME] [nvarchar](256) NOT NULL,
	[COMPUTER_ID] [bigint] NOT NULL,
	[USER_ID] [bigint] NOT NULL,
	[EXTENSION_ID] [bigint] NOT NULL,
 CONSTRAINT [PK_FILE] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_FILE] UNIQUE NONCLUSTERED 
(
	[NAME] ASC,
	[COMPUTER_ID] ASC,
	[USER_ID] ASC,
	[EXTENSION_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[FILE_EVENT]    Script Date: 7/20/2013 5:15:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FILE_EVENT](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[SOURCE_FILE_ID] [bigint] NOT NULL,
	[TARGET_FILE_ID] [bigint] NOT NULL,
	[PROCESS_ID] [bigint] NOT NULL,
	[START_TIME] [datetime] NOT NULL,
	[END_TIME] [datetime] NOT NULL,
	[TYPE] [tinyint] NOT NULL,
	[COUNT] [bigint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[GROUP_SETTING]    Script Date: 7/20/2013 5:15:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[GROUP_SETTING](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[NAME] [varchar](256) NOT NULL,
	[ACCUMULATION_INTERVAL] [time](0) NOT NULL,
	[FILTER_UPDATE_INTERVAL] [time](0) NOT NULL,
	[GRANULARITY] [time](0) NOT NULL,
	[COLLECT_PROCESSES] [bit] NOT NULL,
	[COLLECT_EXTENSIONS] [bit] NOT NULL,
 CONSTRAINT [PK_GROUP_SETTING] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_GROUP_SETTING] UNIQUE NONCLUSTERED 
(
	[NAME] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[PROCESS]    Script Date: 7/20/2013 5:15:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PROCESS](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[NAME] [nvarchar](256) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_PROCESS] UNIQUE NONCLUSTERED 
(
	[NAME] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[PROCESS_FILTER]    Script Date: 7/20/2013 5:15:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PROCESS_FILTER](
	[USER_GROUP_ID] [bigint] NOT NULL,
	[PROCESS_ID] [bigint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[USER_GROUP_ID] ASC,
	[PROCESS_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[USER]    Script Date: 7/20/2013 5:15:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[USER](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[NAME] [nvarchar](256) NOT NULL,
	[DOMAIN_ID] [bigint] NOT NULL,
	[USER_GROUP_ID] [bigint] NOT NULL,
 CONSTRAINT [PK_USER] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_USER] UNIQUE NONCLUSTERED 
(
	[NAME] ASC,
	[DOMAIN_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[USER_GROUP]    Script Date: 7/20/2013 5:15:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[USER_GROUP](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[NAME] [varchar](256) NOT NULL,
	[FILTER] [varchar](256) NOT NULL,
	[GROUP_SETTING_ID] [bigint] NOT NULL,
 CONSTRAINT [PK_USER_GROUP] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[EXTENSION_FILTER]  WITH CHECK ADD  CONSTRAINT [FK_EXTENSION_FILTER_EXTENSION] FOREIGN KEY([EXTENSION_ID])
REFERENCES [dbo].[EXTENSION] ([ID])
GO
ALTER TABLE [dbo].[EXTENSION_FILTER] CHECK CONSTRAINT [FK_EXTENSION_FILTER_EXTENSION]
GO
ALTER TABLE [dbo].[EXTENSION_FILTER]  WITH CHECK ADD  CONSTRAINT [FK_EXTENSION_FILTER_USER_GROUP] FOREIGN KEY([USER_GROUP_ID])
REFERENCES [dbo].[USER_GROUP] ([ID])
GO
ALTER TABLE [dbo].[EXTENSION_FILTER] CHECK CONSTRAINT [FK_EXTENSION_FILTER_USER_GROUP]
GO
ALTER TABLE [dbo].[FILE]  WITH CHECK ADD  CONSTRAINT [FK_FILE_COMPUTER] FOREIGN KEY([COMPUTER_ID])
REFERENCES [dbo].[COMPUTER] ([ID])
GO
ALTER TABLE [dbo].[FILE] CHECK CONSTRAINT [FK_FILE_COMPUTER]
GO
ALTER TABLE [dbo].[FILE]  WITH CHECK ADD  CONSTRAINT [FK_FILE_EXTENSION] FOREIGN KEY([EXTENSION_ID])
REFERENCES [dbo].[EXTENSION] ([ID])
GO
ALTER TABLE [dbo].[FILE] CHECK CONSTRAINT [FK_FILE_EXTENSION]
GO
ALTER TABLE [dbo].[FILE]  WITH CHECK ADD  CONSTRAINT [FK_FILE_USER] FOREIGN KEY([USER_ID])
REFERENCES [dbo].[USER] ([ID])
GO
ALTER TABLE [dbo].[FILE] CHECK CONSTRAINT [FK_FILE_USER]
GO
ALTER TABLE [dbo].[FILE_EVENT]  WITH CHECK ADD  CONSTRAINT [FK_FILE_EVENT_FILE] FOREIGN KEY([TARGET_FILE_ID])
REFERENCES [dbo].[FILE] ([ID])
GO
ALTER TABLE [dbo].[FILE_EVENT] CHECK CONSTRAINT [FK_FILE_EVENT_FILE]
GO
ALTER TABLE [dbo].[FILE_EVENT]  WITH CHECK ADD  CONSTRAINT [FK_FILE_EVENT_FILE1] FOREIGN KEY([SOURCE_FILE_ID])
REFERENCES [dbo].[FILE] ([ID])
GO
ALTER TABLE [dbo].[FILE_EVENT] CHECK CONSTRAINT [FK_FILE_EVENT_FILE1]
GO
ALTER TABLE [dbo].[FILE_EVENT]  WITH CHECK ADD  CONSTRAINT [FK_FILE_EVENT_PROCESS] FOREIGN KEY([PROCESS_ID])
REFERENCES [dbo].[PROCESS] ([ID])
GO
ALTER TABLE [dbo].[FILE_EVENT] CHECK CONSTRAINT [FK_FILE_EVENT_PROCESS]
GO
ALTER TABLE [dbo].[PROCESS_FILTER]  WITH CHECK ADD  CONSTRAINT [FK_PROCESS_FILTER_PROCESS] FOREIGN KEY([PROCESS_ID])
REFERENCES [dbo].[PROCESS] ([ID])
GO
ALTER TABLE [dbo].[PROCESS_FILTER] CHECK CONSTRAINT [FK_PROCESS_FILTER_PROCESS]
GO
ALTER TABLE [dbo].[PROCESS_FILTER]  WITH CHECK ADD  CONSTRAINT [FK_PROCESS_FILTER_USER_GROUP] FOREIGN KEY([USER_GROUP_ID])
REFERENCES [dbo].[USER_GROUP] ([ID])
GO
ALTER TABLE [dbo].[PROCESS_FILTER] CHECK CONSTRAINT [FK_PROCESS_FILTER_USER_GROUP]
GO
ALTER TABLE [dbo].[USER]  WITH CHECK ADD  CONSTRAINT [FK_USER_DOMAIN] FOREIGN KEY([DOMAIN_ID])
REFERENCES [dbo].[DOMAIN] ([ID])
GO
ALTER TABLE [dbo].[USER] CHECK CONSTRAINT [FK_USER_DOMAIN]
GO
ALTER TABLE [dbo].[USER]  WITH CHECK ADD  CONSTRAINT [FK_USER_USER_GROUP] FOREIGN KEY([USER_GROUP_ID])
REFERENCES [dbo].[USER_GROUP] ([ID])
GO
ALTER TABLE [dbo].[USER] CHECK CONSTRAINT [FK_USER_USER_GROUP]
GO
ALTER TABLE [dbo].[USER_GROUP]  WITH CHECK ADD  CONSTRAINT [FK_USER_GROUP_GROUP_SETTING] FOREIGN KEY([GROUP_SETTING_ID])
REFERENCES [dbo].[GROUP_SETTING] ([ID])
GO
ALTER TABLE [dbo].[USER_GROUP] CHECK CONSTRAINT [FK_USER_GROUP_GROUP_SETTING]
GO
