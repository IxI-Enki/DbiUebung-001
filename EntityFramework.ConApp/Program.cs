using Oracle.ManagedDataAccess.Client;
namespace EntityFramework.ConApp;

/// <summary>
///  A thread-safe random-integer generator 
/// </summary>
public static class RandomGenerator_ThreadSafe
{
  #region FIELDS
  private readonly static Random _inst = new();
  #endregion

  #region METHODS
  /// <summary>
  ///  Returns a random integer between 0 and the limit
  /// </summary>
  /// <param name="limit"></param>
  /// <returns>random int</returns>
  public static int Next(int limit)
  {
    lock (_inst)
      return _inst.Next(limit);
  }
  #endregion
}

internal class Program
{
  #region FIELDS
  private static readonly object _locker = new();
  private static int _testToRun = 1;
  #endregion

  /// <summary>
  /// Entrypoint of the Program
  /// </summary>
  static void Main()
  {
    while (_testToRun != 0)
    {
      Console.Write
        (
          "\n\n" +
          "_____Oracle Database Assignment - Locking_____\n\n" +
          "Enter the amount of Threads to use: "
        );
      Thread[ ] threads = new Thread[ GetThreadAmountFromUser() ];

      Console.Write
        (
          "Which test would you like to run?\n" +
          "1 - Transfer Money from King to Herbert\n" +
          "2 - Transfer Money randomly\n" +
          "0 - Exit\n"
        );

      _testToRun = GetTestChoiceFromUser();
      for (int i = 0 ; i < threads.Length ; i++)
      {
        threads[ i ] = new(ExecuteTests!);
        threads[ i ].Start(i);
      }

      // Wait for all threads to finish
      for (int i = 0 ; i < threads.Length ; i++)
        threads[ i ].Join();

      Console.Write("All threads have finished.");
    }
    if (_testToRun == 0)
      Console.WriteLine("Exit App");
  }

  #region PRIVATE HELPER METHODS
  private static int GetTestChoiceFromUser()
    => int.TryParse(Console.ReadLine() , out int outPut) ?
        outPut == 1 ? 1
      : outPut == 2 ? 2
      : outPut > 2 ? 2
      : outPut <= 0 ? 0
      : 1
      : 1;
  private static int GetThreadAmountFromUser()
    => int.TryParse(Console.ReadLine() , out int amountOfThreads) ? amountOfThreads : 1;
  private static void ChooseTest(OracleConnection oc)
  {
    switch (_testToRun)
    {
      case 1:
        TestTransferMoneyFromKingToHerbert(oc);
        break;
      case 2:
        TestTransferMoneyRandom(oc);
        break;
    }
  }
  private static void ThrowFailOutput(OracleException e)
  {
    Console.BackgroundColor = ConsoleColor.Red;
    Console.ForegroundColor = ConsoleColor.Black;
    Console.Write("\n_____ FAIL _____\n");
    Console.ResetColor();
    Console.WriteLine(e.Message);
    Console.WriteLine(e.StackTrace);
  }
  #endregion

  #region BUSINESS METHODS
  static void ExecuteTests(object thrIdx)
  {
    string connectString
      = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=FREEPDB1)));User Id=system;Password=dbi2425;";

    try
    {
      using OracleConnection oc = new(connectString);
      oc.Open();
      ChooseTest(oc);
      oc.Close();
    }
    catch (OracleException e)
    {
      ThrowFailOutput(e);
    }
  }

  static void TestTransferMoneyRandom(OracleConnection oc)
  {
    for (int i = 0 ; i < 1000 ; i++)
    {
      int
        dest = RandomGenerator_ThreadSafe.Next(4),
        source = RandomGenerator_ThreadSafe.Next(4),
        difference = RandomGenerator_ThreadSafe.Next(1000000);

      TransferMoney(oc , source , dest , difference);

      Console.Write("Random transfered Money, times: " + i + "\n");
    }
  }

  static void TestTransferMoneyFromKingToHerbert(OracleConnection oc)
  {
    for (int i = 0 ; i < 1000 ; i++)
    {
      TransferMoney(oc , 0 , 3 , 1);
      Console.Write("Transfered Money, times: " + i + "\n");
    }
  }

  static int CalculateNewBalance(int kontoBalance , int amount)
  {
    //Expensive computation
    Thread.Sleep(RandomGenerator_ThreadSafe.Next(25));
    return kontoBalance - amount;
  }

  static bool TransferMoney(OracleConnection oc , int source , int dest , int amount)
  {
    lock (_locker)
    {
      if (source != dest)
      {
        OracleTransaction txn = oc.BeginTransaction();

        try
        {
          OracleCommand cmd = oc.CreateCommand();
          int sourceBalance = -1;

          // Query mit einer Ergebniszeile
          cmd.CommandText = "SELECT balance FROM konto WHERE kid=" + source + " FOR UPDATE";

          OracleDataReader reader = cmd.ExecuteReader();
          if (reader.Read())
            sourceBalance = reader.GetInt32(0);
          else
            throw new Exception("invalid source konto specified: " + source);
          reader.Close();

          if (sourceBalance < 0)
            throw new Exception("this should not happen!!!");

          int newBalance = CalculateNewBalance(sourceBalance , amount);

          if (newBalance > 0)
          {
            cmd.CommandText = "LOCK TABLE konto IN EXCLUSIVE MODE";   // Table lock - to prevent deadlocks 
            cmd.ExecuteNonQuery();                                  // while running multiple threads parallel

            cmd.CommandText = "UPDATE konto SET balance = " + newBalance + " WHERE kid=" + source;

            // Anzahl der veränderten Zeilen
            int modifiedRows = cmd.ExecuteNonQuery();

            cmd.CommandText = "UPDATE konto SET balance = balance + " + amount + " WHERE kid=" + dest;
            cmd.ExecuteNonQuery();

            txn.Commit();
            return true;
          }

          txn.Rollback();
        }
        catch (Exception e)
        {
          txn.Rollback();
          Console.WriteLine(e.Message);
        }
      }
      return false;
    }
  }
  #endregion
}