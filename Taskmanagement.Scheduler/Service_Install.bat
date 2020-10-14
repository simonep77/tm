@echo off

echo INSTALLAZIONE SERVIZIO ERD SCHEDULER (ERD-Scheduler)

EasyReportDispatcher_SCHEDULER.exe /install

net start ERD-Scheduler

pause