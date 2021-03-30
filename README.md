# RemoteControl
Small service to send corporate emails controlling remote employees online (maybe they are working)

That was an urgent task to make a system of control remote employees working on their locations - built it really quick during a week on MSSQL, EF (Database-first approach), ExtJS and new for me Core 3.1 (soon will be .Net 5 I hope).

It imports departments and employees from accounting program (1C), sync it with Active Directory to get valid emails and every working day send 1-4 letters to employees with a link to proof they're sitting near computer (nevermind what they actually doing). Managers can unload 2 types of Excel report for any period. The geography of company - 5 time zones, and every employee receives a letter at actual working time.
