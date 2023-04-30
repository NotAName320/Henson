/*
Functions for interacting with database
Copyright (C) 2023 NotAName320

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/


using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace Henson.Models
{
    public class DbClient
    {
        /// <summary>
        /// The path to the database file.
        /// </summary>
        private static readonly string path = Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory)!, "henson.sqlite");

        /// <summary>
        /// Creates the database file if it does not already exist with the table template.
        /// </summary>
        public static void CreateDbIfNotExists()
        {
            using var con = new SQLiteConnection($"Data Source={path}");
            con.Open();

            string createTable = "CREATE TABLE IF NOT EXISTS nations (name varchar(40), pass text, flagUrl text, region text, locked integer DEFAULT 0, UNIQUE(name))";
            using SQLiteCommand createTableCommand = new(createTable, con);
            createTableCommand.ExecuteNonQuery();

            //check if locked exists on upgrading and create it if it doesn't
            string checkLockedExists = "SELECT EXISTS(SELECT 1 FROM (SELECT * FROM pragma_table_info('nations')) WHERE name='locked')";
            using SQLiteCommand checkLockedExistsCommand = new(checkLockedExists, con);
            using SQLiteDataReader reader = checkLockedExistsCommand.ExecuteReader();

            if(reader.Read() && !reader.GetBoolean(0))
            {
                string createLocked = "ALTER TABLE nations ADD COLUMN locked integer DEFAULT 0";
                using SQLiteCommand createLockedCommand = new(createLocked, con);
                createLockedCommand.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Gets the list of existing nations from the database.
        /// </summary>
        /// <returns>A tuple consisting of a list of <c>Nation</c> objects reflecting those stored in the database as well as
        /// a list of strings of the names of locked nations.</returns>
        public static (List<Nation> nations, List<string> locked) GetNations()
        {
            List<Nation> retVal = new();
            List<string> lockedNations = new();

            using var con = new SQLiteConnection($"Data Source={path}");
            con.Open();

            string getNations = "SELECT * FROM nations";
            using SQLiteCommand command = new(getNations, con);
            using SQLiteDataReader reader = command.ExecuteReader();

            while(reader.Read())
            {
                retVal.Add(new Nation(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
                if(reader.GetBoolean(4))
                {
                    lockedNations.Add(reader.GetString(0));
                }
            }

            return (retVal, lockedNations);
        }

        /// <summary>
        /// Inserts a new nation into the database.
        /// </summary>
        /// <param name="nation">The nation being inserted.</param>
        public static void InsertNation(Nation nation)
        {
            using var con = new SQLiteConnection($"Data Source={path}");
            con.Open();

            //yeah yeah i know about sanitization and all that but riddle me this: why would you want to inject into a local sqlite file
            string insertNation = $"INSERT INTO nations (name, pass, flagUrl, region) VALUES ('{nation.Name}', '{nation.Pass}', '{nation.FlagUrl}', '{nation.Region}')";
            using SQLiteCommand command = new(insertNation, con);

            try
            {
                command.ExecuteNonQuery();
            }
            catch (SQLiteException) { } //If something goes wrong who cares
        }

        /// <summary>
        /// Deletes a nation from the database.
        /// </summary>
        /// <param name="name">The name of the nation to be deleted.</param>
        public static void DeleteNation(string name)
        {
            using var con = new SQLiteConnection($"Data Source={path}");
            con.Open();

            string deleteNation = $"DELETE FROM nations WHERE name='{name}'";
            using SQLiteCommand command = new(deleteNation, con);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes a miscellaneous command on the database.
        /// </summary>
        /// <param name="commandString">The command to be executed.</param>
        public static void ExecuteNonQuery(string commandString)
        {
            using var con = new SQLiteConnection($"Data Source={path}");
            con.Open();

            using SQLiteCommand command = new(commandString, con);
            command.ExecuteNonQuery();
        }
    }
}
