# RemoteControl
Small service to send corporate emails controlling remote employees online (maybe they are working)

That was an urgent task to make a system of control remote employees working on their locations - built it really quick during a week on MSSQL, EF (Database-first approach), ExtJS and new for me Core 3.1 (soon will be .Net 5 I hope).

It imports departments and employees from accounting program (1C), sync it with Active Directory to get valid emails and every working day send 1-4 letters to employees with a link to proof they're sitting near computer (nevermind what they actually doing). Managers can unload 2 types of Excel report for any period. The geography of company - 5 time zones, and every employee receives a letter at actual working time.

![изображение](https://user-images.githubusercontent.com/42838528/112934992-eec24200-9133-11eb-88c5-c67a7f8dc234.png)

![изображение](https://user-images.githubusercontent.com/42838528/112935007-f550b980-9133-11eb-8684-ae78a5a66729.png)

![изображение](https://user-images.githubusercontent.com/42838528/112935022-faae0400-9133-11eb-8421-1168456256a5.png)

