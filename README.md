# DbiUebung-001 -- Oracle (not Entity) Framework

> ###### Todo:
> ##### Führen Sie die Tests mit numberOfParallelThreads=1 aus. Verhält sich der Code wie erwartet?  
> ##### Analysieren Sie den gegeben Code auf Probleme. Wie manifestieren sich die Probleme bei den beiden Tests?  
> ##### Beheben Sie die Nebenläufigkeitsprobleme mittels den gegeben Locking-Arten und behandeln Sie jene Fehler, die dabei auftreten können.  
> - Zeilen-Locks
> - Table-Lock
> - Optimistisches Locking

---    

- Bei nur einem Thread verhält sich der code wie erwartet, jedes query wird nacheinander abgearbeitet,  
  bei mehreren parallelen Threads können jetzt bereits Probleme auftreten.
  > Ein Zeilen-Lock scheint deadlocks nicht zu verhindern und selbst mit dem lock-object im c# code, treten ab und zu deadlocks auf (je nach cpu auslastung)
- Ich fügte, wie bereits genannt "FOR UPDATE":  
  ![lineLock](image-1.png)  
- sowie eine Lock-variable _locker zum code hinzu:  
  ![lockObject](image-2.png)
- Als letzte Maßnahme erweiterte ich den code um einen vollständigen Table-lock, welcher die deadlocks letztendlich verhindert:  
  ![tableLock](image.png)
    > (auch bei vielen parallellen Threads, stimmen die Transaktionen und blockieren sich nun nichtmehr)  

 ---  

- OPTIMISTIC "Locking":   
 ![optimisticLocking](image-4.png)  

- PESIMISTIC Locking:   
  ![pesimisticLocking](image-3.png)  

 --- 

### Program ran through, with 10 parallel threads:  
 ![ecexutionScreenshot](execution-2.png)

---  
> ###### Quick overview of the code
![c#CodeOverview](image-5.png)