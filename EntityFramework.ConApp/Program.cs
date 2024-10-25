
using Oracle.ManagedDataAccess.Client;

namespace EntityFramework.ConApp;

public static class RandomGenThrSafe
{
  #region FIELDS
  private readonly static Random _inst = new();
  #endregion

  #region METHODS
  public static int Next(int limit)
  {
    lock (_inst)
      return _inst.Next(limit);
  }
  #endregion
}

internal class Program
{
  private static readonly object _locker = new();

  static void Main()
  {
    // Anzahl der gleichzeitigen Aufrufe kann hier eingestellt werden
    const int numberOfParallelThreads = 5;

    Thread[ ] threads = new Thread[ numberOfParallelThreads ];
    for (int i = 0 ; i < threads.Length ; i++)
    {
      threads[ i ] = new(ExecuteTests!);
      threads[ i ].Start(i);
    }

    // Wait for all threads to finish
    for (int i = 0 ; i < threads.Length ; i++)
      threads[ i ].Join();

    Console.WriteLine("All threads have finished.");
  }

  static void ExecuteTests(object thrIdx)
  {
    string connectString
      = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=FREEPDB1)));User Id=system;Password=XXXXXX;";

    try
    {
      using OracleConnection oc = new(connectString);
      oc.Open();

      // Uncomment the test you would like to execute below
      // TestTransferMoneyFromKingToHerbert(oc);
      TestTransferMoneyRandom(oc);

      oc.Close();
    }
    catch (OracleException e)
    {
      Console.WriteLine(e.Message);
      Console.WriteLine(e.StackTrace);
    }
  }

  static void TestTransferMoneyRandom(OracleConnection oc)
  {
    for (int i = 0 ; i < 1000 ; i++)
    {
      int
        dest = RandomGenThrSafe.Next(4),
        source = RandomGenThrSafe.Next(4),
        difference = RandomGenThrSafe.Next(1000000);

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
    Thread.Sleep(RandomGenThrSafe.Next(25));
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

        return false;
      }
    }
  }
}
