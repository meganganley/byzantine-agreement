@ECHO OFF
csc /r:System.Threading.Tasks.Dataflow.dll;System.Runtime.dll;System.Threading.Tasks.dll /out:byz.exe *.cs 
pause