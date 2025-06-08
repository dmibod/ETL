using System;
using Microsoft.Data.Sqlite;

public class TweetStateRepository
{
    private readonly string _connectionString;

    public TweetStateRepository(string dbPath = "state.db")
    {
        _connectionString = $"Data Source={dbPath}";
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS State (
            Account TEXT PRIMARY KEY,
            LastTweetId INTEGER
        )";
        cmd.ExecuteNonQuery();
    }

    public long GetLastTweetId(string account)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT LastTweetId FROM State WHERE Account = $account";
        cmd.Parameters.AddWithValue("$account", account);
        var result = cmd.ExecuteScalar();
        if (result == null || result == DBNull.Value)
        {
            return 0;
        }
        return Convert.ToInt64(result);
    }

    public void SetLastTweetId(string account, long id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"INSERT INTO State (Account, LastTweetId)
                             VALUES ($account, $id)
                             ON CONFLICT(Account) DO UPDATE SET LastTweetId = $id";
        cmd.Parameters.AddWithValue("$account", account);
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }
}
