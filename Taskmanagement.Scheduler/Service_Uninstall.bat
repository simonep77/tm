@echo off

echo DISINSTALLAZIONE SERVIZIO TM SCHEDULER (TM-Scheduler) deve essere eseguito come amministratore

net stop TM-Scheduler

Taskmanagement.Scheduler.exe /uninstall


pause