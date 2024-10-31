# DbiUebung-001 -- Entity Framework

> ###### Todo:
> Führen Sie die Tests mit numberOfParallelThreads=1 aus. Verhält sich der Code wie erwartet?  
> Analysieren Sie den gegeben Code auf Probleme. Wie manifestieren sich die Probleme bei den beiden Tests?  
> Beheben Sie die Nebenläufigkeitsprobleme mittels den gegeben Locking-Arten und behandeln Sie jene Fehler, die dabei auftreten können.  
> - Zeilen-Locks
> - Table-Lock
> - Optimistisches Locking

---   

- Bei nur einem Thread verhält sich der code wie erwartet, jedes query wird nacheinander abgearbeitet,  
  bei mehreren parallelen Threads können jetzt bereits Probleme auftreten.
  > Ein Zeilen-Lock scheint deadlocks nicht zu verhindern und selbst mit dem lock-object im c# code, treten ab und zu deadlocks auf (je nach cpu auslastung)
- Ich fügte, wie bereits genannt "FOR UPDATE":  
  ![forUpdate](https://github.com/user-attachments/assets/33e90627-a8b1-4cde-a929-2c4b6d920bdb)  
- sowie eine Lock-variable _locker zum code hinzu:  
  ![lockObject](https://github.com/user-attachments/assets/cc627d33-60d7-43dd-af93-13f8a4a0d2fd)  
- Als letzte Maßnahme erweiterte ich den code um einen Vollständigen Table-lock, welcher die deadlocks letztendlich verhindert:  
  > (auch bei vielen parallellen Threads, stimmen die Transaktionen und blockieren sich nun nichtmehr)  
  ![tableLock](https://github.com/user-attachments/assets/5ff19486-0f39-487e-a942-0dc993e72ede)  
 
