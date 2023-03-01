using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace Henson.Models
{
    public class DbClient
    {
        private static readonly string path = Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory)!, "henson.sqlite");

        public static void CreateDbIfNotExists()
        {
            if (File.Exists(path)) return;

            using var con = new SQLiteConnection($"Data Source={path}");
            con.Open();

            string createTable = "CREATE TABLE nations (name varchar(40), pass text, flagUrl text, region text, UNIQUE(name))";
            using SQLiteCommand command = new(createTable, con);
            command.ExecuteNonQuery();
        }

        public static List<Nation> GetNations()
        {
            List<Nation> retVal = new();

            using var con = new SQLiteConnection($"Data Source={path}");
            con.Open();

            string getNations = "SELECT * FROM nations";
            using SQLiteCommand command = new(getNations, con);
            using SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                retVal.Add(new Nation(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
            }

            return retVal;
        }

        public static void InsertNation(Nation n)
        {
            using var con = new SQLiteConnection($"Data Source={path}");
            con.Open();

            //yeah yeah i know about sanitization and all that but riddle me this: why would you want to inject into a local sqlite file
            string insertNation = $"INSERT INTO nations (name, pass, flagUrl, region) VALUES ('{n.Name}', '{n.Pass}', '{n.FlagUrl}', '{n.Region}')";
            using SQLiteCommand command = new(insertNation, con);

            try
            {
                command.ExecuteNonQuery();
            }
            catch (SQLiteException) { }
        }

        public static void DeleteNation(string name)
        {
            using var con = new SQLiteConnection($"Data Source={path}");
            con.Open();

            string deleteNation = $"DELETE FROM nations WHERE name='{name}'";
            using SQLiteCommand command = new(deleteNation, con);
            command.ExecuteNonQuery();
        }

        public static void ExecuteNonQuery(string commandString)
        {
            using var con = new SQLiteConnection($"Data Source={path}");
            con.Open();

            using SQLiteCommand command = new(commandString, con);
            command.ExecuteNonQuery();
        }
    }
}
