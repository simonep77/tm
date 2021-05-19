@echo off

echo INSTALLAZIONE SERVIZIO TM SCHEDULER (TM-Scheduler) deve essere eseguito come amministratore

echo abilitazione Web Api su sistema modificare url se la porta e' diversa

netsh http add urlacl url=http://+:8080/ user=Everyone listen = yes

echo Installazione SERVIZIO

Taskmanagement.Scheduler.exe /install

echo Avvio SERVIZIO

net start TM-Scheduler

pause