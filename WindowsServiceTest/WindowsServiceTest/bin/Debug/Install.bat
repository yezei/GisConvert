%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\installutil.exe D:\LD\Git\WindowsServiceTest\WindowsServiceTest\bin\Debug
Net Start ServiceTest
sc config ServiceTest start= auto
pause