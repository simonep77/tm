@echo off

echo INSTALLAZIONE SERVIZIO TM SCHEDULER (TM-Scheduler) deve essere eseguito come amministratore

Taskmanagement.Scheduler.exe /install

net start TM-Scheduler

pause