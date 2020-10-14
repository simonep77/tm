@echo off

echo DISINSTALLAZIONE SERVIZIO ERD SCHEDULER (ERD-Scheduler)

net stop ERD-Scheduler

EasyReportDispatcher_SCHEDULER.exe /uninstall

pause