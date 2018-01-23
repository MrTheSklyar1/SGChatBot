# SGChatBot
Telegram chat bot (StarGate Quiz)


In Release/Debug folder must be Settings.txt file with info:
Secret_Key_Of_Your_Telegram_Bot|Connection_String_To_Your_Database


Database hierarchy:

Table Users:  
User_ID - int - NOT NULL(Primary key)  
Username - varchar(MAX) - NOT NULL  
Score - int - NOT NULL  

Table Tasks:  
Task_id - int - NOT NULL(Primary key)  
Task_text - varchar(MAX) - NOT NULL  
Serial - varchar(MAX) - NOT NULL  
Season - varchar(MAX) - NOT NULL  
Series - varchar(MAX) - NOT NULL  

Table Serials:  
id - int - NOT NULL  
Serial - varchar(50) - NOT NULL  
Seasons - int - NOT NULL  
Season - int - NOT NULL  
Series - varchar(MAX) - NOT NULL  

Table UserQuestionInfo:  
User_ID - int - NOT NULL(Foreign key)  
Task_id - int - NOT NULL(Foreign key)  
done - int - NOT NULL  
Stand - int - NOT NULL  
Ask - int - NOT NULL  
LastItemsSer - varchar(MAX) - NOT NULL  
LastItemsSeas - varchar(MAX) - NOT NULL  

Table AskToWrong:  
id - int - NOT NULL(Primary key)  
user_id - int - NOT NULL  
Message - varchar(MAX) - NOT NULL  
task_id - int - NOT NULL(Foreign key)  