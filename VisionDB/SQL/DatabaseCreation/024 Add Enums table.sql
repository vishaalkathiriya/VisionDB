CREATE TABLE [dbo].[Enums](
	[EnumName] [nvarchar](50) NOT NULL,
	[EnumId] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Enums] PRIMARY KEY CLUSTERED 
(
	[EnumName] ASC,
	[EnumId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

INSERT INTO Enums (EnumName, EnumId, Name) VALUES 
('ObjectiveMethod', 0, 'Unknown'), 
('ObjectiveMethod', 1, 'Retinoscopy'), 
('ObjectiveMethod', 2, 'Autorefractor'),
('YesNo', 0, 'None'),
('YesNo', 1, 'Yes'),
('YesNo', 2, 'No'),
('Eye', 1, 'Right'),
('Eye', 2, 'Left')