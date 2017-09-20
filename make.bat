csc /out:bin\transformar.exe /target:winexe /win32icon:source\lung.ico source\*.cs source\exe\Openslide.cs
csc /out:bin\histo.exe /r:bin\ExcelLibrary.dll /target:winexe /win32icon:source\lung.ico source\*.cs source\exe\Histo.cs
copy bin\transformar.exe c:\histo
copy bin\histo.exe c:\histo
copy histo.pdf c:\histo
copy transformar.pdf c:\histo