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
using System.IO;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace Henson.Models
{
    public static class DbClient
    {
        /// <summary>
        /// The path to the database file.
        /// </summary>
        private static readonly string DbPath = Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory)!, "henson.sqlite");

        /// <summary>
        /// Creates the database file if it does not already exist with the table template.
        /// </summary>
        public static void CreateDbIfNotExists()
        {
            using var con = new SqliteConnection($"Data Source={DbPath}");
            con.Open();

            const string createTable = """
                                       CREATE TABLE IF NOT EXISTS nations
                                       (
                                           name varchar(40),
                                           pass text,
                                           flagUrl text,
                                           region text,
                                           locked integer DEFAULT 0,
                                           groupName text DEFAULT NULL,
                                           groupOrder integer DEFAULT 0,
                                           UNIQUE(name)
                                       );
                                       CREATE TABLE IF NOT EXISTS groups
                                       (
                                           name text,
                                           expanded integer DEFAULT 1,
                                           UNIQUE(name)
                                       )
                                       """;
            using SqliteCommand createTableCommand = new(createTable, con);
            createTableCommand.ExecuteNonQuery();

            //check if locked exists on upgrading and create it if it doesn't
            const string checkLockedExists = "SELECT EXISTS(SELECT 1 FROM (SELECT * FROM pragma_table_info('nations')) WHERE name='locked')";
            using SqliteCommand checkLockedExistsCommand = new(checkLockedExists, con);
            using var checkLockedExistsReader = checkLockedExistsCommand.ExecuteReader();

            if(checkLockedExistsReader.Read() && !checkLockedExistsReader.GetBoolean(0))
            {
                const string createLocked = "ALTER TABLE nations ADD COLUMN locked integer DEFAULT 0";
                using SqliteCommand createLockedCommand = new(createLocked, con);
                createLockedCommand.ExecuteNonQuery();
            }
            
            //do the same but for group name and group order
            //yeah copying code is lazy idrc
            const string checkGroupsExist = "SELECT EXISTS(SELECT 1 FROM (SELECT * FROM pragma_table_info('nations')) WHERE name='groupName')";
            using SqliteCommand checkGroupsExistCommand = new(checkGroupsExist, con);
            using var checkGroupsExistReader = checkGroupsExistCommand.ExecuteReader();

            if(checkGroupsExistReader.Read() && !checkGroupsExistReader.GetBoolean(0))
            {
                const string createGroups = "ALTER TABLE nations ADD COLUMN groupName varchar(255) DEFAULT NULL; ALTER TABLE nations ADD COLUMN groupOrder integer DEFAULT 0";
                using SqliteCommand createGroupsCommand = new(createGroups, con);
                createGroupsCommand.ExecuteNonQuery();
                
                const string setInitialGroupOrders = "UPDATE nations SET groupOrder = ROWID";
                using SqliteCommand setInitialGroupOrdersCommand = new(setInitialGroupOrders, con);
                setInitialGroupOrdersCommand.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Gets the list of existing nations and groups from the database.
        /// </summary>
        /// <returns>A dictionary describing the groups and nations, with ungrouped nations accessible with key Ungrouped.</returns>
        public static Dictionary<string, (List<(Nation nation, bool locked)> nations, bool expanded)> GetNations()
        {
            //maybe i should make a model for this
            Dictionary<string, (List<(Nation nation, bool locked)> nations, bool expanded)> retVal = new() { { "Ungrouped", ([], false) } };

            using var con = new SqliteConnection($"Data Source={DbPath}");
            con.Open();

            const string getGroups = "SELECT * FROM groups;";
            var getGroupsCommand = new SqliteCommand(getGroups, con);
            using var getGroupsReader = getGroupsCommand.ExecuteReader();
            while(getGroupsReader.Read())
            {
                retVal.Add(getGroupsReader.GetString(0), ([], getGroupsReader.GetBoolean(1)));
            }

            const string getNations = "SELECT * FROM nations ORDER BY groupOrder;";
            var getNationsCommand = new SqliteCommand(getNations, con);
            using var reader = getNationsCommand.ExecuteReader();

            while(reader.Read())
            {
                var nation = new Nation(reader.GetString(0), reader.GetString(1), reader.GetString(2),
                    reader.GetString(3));
                var foundGroup = false;
                foreach(var group in retVal.Keys)
                {
                    if(reader.IsDBNull(5) || reader.GetString(5) != group) continue;
                    foundGroup = true;
                    retVal[group].nations.Add((nation, reader.GetBoolean(4)));
                    break;
                }

                if(!foundGroup)
                {
                    retVal["Ungrouped"].nations.Add((nation, false));
                }
            }

            return retVal;
        }

        /// <summary>
        /// Inserts a new nation into the database.
        /// </summary>
        /// <param name="nation">The nation being inserted.</param>
        public static void InsertNation(Nation nation)
        {
            using var con = new SqliteConnection($"Data Source={DbPath}");
            con.Open();

            //yeah yeah i know about sanitization and all that but riddle me this: why would you want to inject into a local sqlite file
            var insertNation = $"INSERT INTO nations (name, pass, flagUrl, region) VALUES ('{nation.Name}', '{nation.Pass}', '{nation.FlagUrl}', '{nation.Region}')";
            using SqliteCommand command = new(insertNation, con);

            try
            {
                command.ExecuteNonQuery();
            }
            catch (SqliteException) { } //If something goes wrong who cares
        }

        /// <summary>
        /// Deletes a nation from the database.
        /// </summary>
        /// <param name="name">The name of the nation to be deleted.</param>
        public static void DeleteNation(string name)
        {
            using var con = new SqliteConnection($"Data Source={DbPath}");
            con.Open();

            var deleteNation = $"DELETE FROM nations WHERE name='{name}'";
            using SqliteCommand command = new(deleteNation, con);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes a miscellaneous command on the database.
        /// </summary>
        /// <param name="commandString">The command to be executed.</param>
        public static void ExecuteNonQuery(string commandString)
        {
            using var con = new SqliteConnection($"Data Source={DbPath}");
            con.Open();

            using SqliteCommand command = new(commandString, con);
            command.ExecuteNonQuery();
        }

        public static void StoreExpansionState(string group, bool expanded)
        {
            using var con = new SqliteConnection($"Data Source={DbPath}");
            con.Open();

            var intExpanded = expanded ? 1 : 0;
            var modifyGroupExpand = $"UPDATE groups SET expanded = {intExpanded} WHERE name='{group}'";
            using SqliteCommand command = new(modifyGroupExpand, con);
            command.ExecuteNonQuery();
        }

        public static void StoreGroupState(string group, List<string> nations)
        {
            using var con = new SqliteConnection($"Data Source={DbPath}");
            con.Open();

            using var transaction = con.BeginTransaction();
            var order = 0;
            foreach(var nation in nations)
            {
                var groupOrNull = group == "Ungrouped" ? "NULL" : group;
                var updateNationGroup =
                    $"UPDATE nations SET groupOrder = {order}, groupName = '{groupOrNull}' WHERE name='{nation}'";
                using SqliteCommand command = new(updateNationGroup, con, transaction);
                command.ExecuteNonQuery();
                order++;
            }
            
            transaction.Commit();
        }
    }
}
