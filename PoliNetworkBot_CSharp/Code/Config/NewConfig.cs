﻿#region

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects.InfoBot;
using PoliNetworkBot_CSharp.Code.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

#endregion

namespace PoliNetworkBot_CSharp.Code.Config
{
    public static class NewConfig
    {
        private const string RowSeparator = "| _:r:_ |";
        private const string ColumnSeparator = "| _:c:_ |";

        public static void NewConfigMethod(bool resetBot, bool resetUserBot, bool resetBotDisguisedAsUserBot,
            bool destroy_db_and_redo_it, bool alsoFillTablesFromJson)
        {
            if (resetBot) ResetBotMethod();

            if (resetUserBot) ResetUserbotMethod();

            if (resetBotDisguisedAsUserBot) ResetBotDisguisedAsUserBotMethod();

            if (destroy_db_and_redo_it)
                DestroyDB_And_Redo_it(alsoFillTablesFromJson);
        }

        private static void ResetBotDisguisedAsUserBotMethod()
        {
            var lines = File.ReadAllText(Paths.Info.ConfigBotDisguisedAsUserBotsInfo).Split(RowSeparator);
            var botInfos = new List<BotDisguisedAsUserBotInfo>();
            foreach (var t in lines)
            {
                var line = t;
                if (string.IsNullOrEmpty(line))
                    continue;

                line = line.Trim();

                if (string.IsNullOrEmpty(line))
                    continue;

                var lineInfo = line.Split(ColumnSeparator);

                var bot = new BotDisguisedAsUserBotInfo();
                bot.SetApiId(lineInfo[0].Trim());
                bot.SetApiHash(lineInfo[1].Trim());
                bot.SetUserId(lineInfo[2].Trim());
                bot.SetToken(lineInfo[3].Trim());
                bot.SetIsBot(BotTypeApi.DISGUISED_BOT);

                botInfos.Add(bot);
            }

            FileSerialization.WriteToBinaryFile(Paths.Bin.ConfigBotDisguisedAsUserbot, botInfos);
        }

        private static void ResetUserbotMethod()
        {
            var lines = File.ReadAllText(Paths.Info.ConfigUserBotsInfo).Split(RowSeparator);
            var botInfos = new List<UserBotInfo>();
            foreach (var t in lines)
            {
                var line = t;
                if (string.IsNullOrEmpty(line))
                    continue;

                line = line.Trim();

                if (string.IsNullOrEmpty(line))
                    continue;

                var lineInfo = line.Split(ColumnSeparator);

                var bot = new UserBotInfo();
                bot.SetApiId(lineInfo[0].Trim());
                bot.SetApiHash(lineInfo[1].Trim());
                bot.SetUserId(lineInfo[2].Trim());
                bot.SetNumberCountry(lineInfo[3].Trim());
                bot.SetNumberNumber(lineInfo[4].Trim());
                bot.SetPasswordToAuthenticate(lineInfo[5].Trim());
                bot.SetIsBot(BotTypeApi.USER_BOT);
                bot.SetMethod(lineInfo[6].Trim());

                botInfos.Add(bot);
            }

            FileSerialization.WriteToBinaryFile(Paths.Bin.ConfigUserbot, botInfos);
        }

        private static void ResetBotMethod()
        {
            var lines = File.ReadAllText(Paths.Info.ConfigBotsInfo).Split(RowSeparator);
            var botInfos = new List<BotInfo>();
            foreach (var t in lines)
            {
                var line = t;
                if (string.IsNullOrEmpty(line))
                    continue;

                line = line.Trim();

                if (string.IsNullOrEmpty(line))
                    continue;

                var lineInfo = line.Split(ColumnSeparator);

                var bot = new BotInfo();
                bot.SetToken(lineInfo[0].Trim());
                bot.SetWebsite(lineInfo[1].Trim());
                bot.SetIsBot(BotTypeApi.REAL_BOT);
                bot.SetAcceptMessages(true);
                bot.SetOnMessages(lineInfo[2].Trim());
                bot.SetContactString(lineInfo[3].Trim());

                botInfos.Add(bot);
            }

            FileSerialization.WriteToBinaryFile(Paths.Bin.ConfigBot, botInfos);
        }

        private static void DestroyDB_And_Redo_it(bool alsoFillTablesFromJson)
        {
            DirectoryUtils.CreateDirectory("data");

            var dbPath = Paths.Db;
            dbPath = dbPath.Split('=')[1];
            try
            {
                File.WriteAllText(dbPath, "");
            }
            catch
            {
                ;
            }

            CleanDb();

            Redo_DB(alsoFillTablesFromJson);
        }

        private static void CleanDb()
        {
            const string s = "SELECT name FROM sqlite_master WHERE type='table'";
            var r1 = SqLite.ExecuteSelect(s);
            if (r1 == null)
                return;

            foreach (DataRow dr in r1.Rows)
            {
                var name = dr.ItemArray[0].ToString();
                if (name != null && name.StartsWith("sqlite_"))
                {
                    ;
                }
                else
                {
                    var q = "DROP TABLE IF EXISTS " + name;
                    SqLite.Execute(q);
                }
            }
        }

        private static void Redo_DB(bool alsoFillTablesFromJson)
        {
            SqLite.Execute("CREATE TABLE Groups (" +
                           "id BIGINT PRIMARY KEY, " +
                           "bot_id INT(12)," +
                           "valid CHAR(1)," +
                           "link VARCHAR(250)," +
                           "last_update_link DATETIME," +
                           "type VARCHAR(250)," +
                           "title VARCHAR(250)" +
                           ") ");

            if (alsoFillTablesFromJson)
                FillGroups(0);

            SqLite.Execute("CREATE TABLE PeopleInEntities (" +
                           "id_entity INT(12)," +
                           "id_person INT(12)," +
                           "CONSTRAINT PK_Person PRIMARY KEY (id_entity,id_person)" +
                           ");");

            SqLite.Execute("CREATE TABLE Entities (" +
                           "id INT(12) PRIMARY KEY," +
                           "name VARCHAR(250)" +
                           ");");

            if (alsoFillTablesFromJson)
                FillAssoc();

            SqLite.Execute("CREATE TABLE Messages (" +
                           "id INT(12) PRIMARY KEY," +
                           "from_id_person INT(12)," +
                           "from_id_entity INT(12)," +
                           "type int INT(12)," +
                           "id_photo INT(12)," +
                           "id_video INT(12)," +
                           "id_file INT(12)," +
                           "id_voice INT(12)," +
                           "id_audio INT(12)," +
                           "message_text TEXT," +
                           "sent_date DATETIME," +
                           "has_been_sent BOOLEAN," +
                           "message_id_tg_from INT(12)," +
                           "message_id_tg_to INT(12)," +
                           "id_chat_sent_into BIGINT," +
                           "from_id_bot INT(12)," +
                           "type_chat_sent_into VARCHAR(250)" +
                           ");");

            SqLite.Execute("CREATE TABLE MessageTypes (" +
                           "id INT(12) PRIMARY KEY," +
                           "name VARCHAR(250)" +
                           ");");

            SqLite.Execute("CREATE TABLE Photos (" +
                           "id_photo INT(12) PRIMARY KEY," +
                           "file_id VARCHAR(250)," +
                           "file_size INT(12)," +
                           "height INT(12)," +
                           "width INT(12)," +
                           "unique_id VARCHAR(250)" +
                           ");");

            SqLite.Execute("CREATE TABLE Videos (" +
                           "id_video INT(12) PRIMARY KEY," +
                           "file_id VARCHAR(250)," +
                           "file_size INT(12)," +
                           "height INT(12)," +
                           "width INT(12)," +
                           "unique_id VARCHAR(250)," +
                           "duration INT," +
                           "mime VARCHAR(250)" +
                           ");");
        }

        private static void FillGroups(int botIdWhoInsertedThem)
        {
            //read groups from polinetwork python config file and fill db
            var s = File.ReadAllText("../../../Old/data/groups.json");
            var r = JsonConvert.DeserializeObject<JObject>(s);
            var r2 = r.Children();
            foreach (var r3 in r2)
            {
                if (r3 is JProperty r4)
                {
                    var name = r4.Name;
                    if (name == "Gruppi")
                    {
                        var r5 = r4.Children();
                        foreach (var r6 in r5)
                        {
                            var r7 = r6.Children();
                            foreach (var r8 in r7)
                            {
                                if (r8 is JObject r9)
                                {
                                    var r10 = r9.Children();

                                    AddGroupToDb(r10, botIdWhoInsertedThem);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static Tuple<bool, List<Exception>> AddGroupToDb(JEnumerable<JToken> r1, int botIdWhoInsertedThem)
        {
            ;

            ChatJson chat = null;
            DateTime? lastUpdateLinkTime = null;
            bool? we_are_admin = null;

            List<Exception> exceptions = new List<Exception>();
            foreach (var r2 in r1)
            {
                ;
                if (r2 is JProperty r3)
                {
                    ;
                    if (r3.Name == "Chat")
                    {
                        chat = GetChatFromJson(r3);
                    }
                    else if (r3.Name == "LastUpdateInviteLinkTime")
                    {
                        Objects.ValueWithException<DateTime?> d1 = GetLastUpdateLinkTimeFromJson(r3);
                        if (d1.HasValue())
                            lastUpdateLinkTime = d1.GetValue();
                        else
                        {
                            exceptions.AddRange(d1.GetExceptions());
                        }
                    }
                    else if (r3.Name == "we_are_admin")
                    {
                        we_are_admin = GetWeAreAdminFromJson(r3);
                    }
                }
            }

            var d2 = AddGroupToDb2(chat, lastUpdateLinkTime, we_are_admin, botIdWhoInsertedThem);
            return new Tuple<bool, List<Exception>>(d2, exceptions);
        }

        private static bool AddGroupToDb2(ChatJson chat, DateTime? lastUpdateLinkTime, bool? we_are_admin,
            int botIdWhoInsertedThem)
        {
            try
            {
                const string q1 = "INSERT INTO Groups (id, bot_id, type, title, link, last_update_link, valid) " +
                    " VALUES " +
                    " (@id, @botid, @type, @title, @link, @lul, @valid)";
                SqLite.Execute(q1, new Dictionary<string, object>
                {
                    {"@id", chat.id},
                    {"@botid", botIdWhoInsertedThem },
                    {"@type", chat.type},
                    {"@title", chat.title},
                    {"@link", chat.invite_link },
                    {"@lul", lastUpdateLinkTime},
                    {"@valid", we_are_admin }
                });
            }
            catch
            {
                return false;
            }

            return true;
        }

        private static bool? GetWeAreAdminFromJson(JProperty r3)
        {
            ;
            var r4 = r3.First;
            ;
            if (r4 is JValue r5)
            {
                ;
                if (r5.Value == null)
                    return null;

                if (r5.Value.ToString().ToLower() == "true")
                    return true;

                return false;
            }
            return null;
        }

        private static Objects.ValueWithException<DateTime?> GetLastUpdateLinkTimeFromJson(JProperty r3)
        {
            ;
            var r4 = r3.First;
            ;
            if (r4 is JValue r5)
            {
                ;
                if (r5.Value == null)
                    return null;

                return Utils.DateTimeClass.GetFromString(r5.Value.ToString());
            }
            return new Objects.ValueWithException<DateTime?>(null, new Code.Exceptions.JsonDateTimeNotFound());
        }

        private static ChatJson GetChatFromJson(JProperty r3)
        {
            ;
            var r4 = r3.Children();
            ;
            var r5 = r4.Children();

            long? id = null;
            string type = null;
            string title = null;
            string invite_link = null;

            foreach (var r6 in r5)
            {
                ;
                if (r6 is JProperty r7)
                {
                    ;
                    if (r7.Name == "id")
                    {
                        id = GetIdFromJson(r7);
                    }
                    else if (r7.Name == "type")
                    {
                        type = GetTypeFromJson(r7);
                    }
                    else if (r7.Name == "title")
                    {
                        title = GetTitleFromJson(r7);
                    }
                    else if (r7.Name == "invite_link")
                    {
                        invite_link = GetInviteLinkFromJson(r7);
                    }
                    else
                    {
                        ;
                    }
                }
            }

            return new ChatJson(id, type, title, invite_link);
        }

        private static string GetInviteLinkFromJson(JProperty r7)
        {
            var r8 = r7.First;
            ;
            if (r8 is JValue r9)
            {
                if (r9.Value == null)
                    return null;

                return r9.Value.ToString();
            }

            return null;
        }

        private static string GetTitleFromJson(JProperty r7)
        {
            var r8 = r7.First;
            ;
            if (r8 is JValue r9)
            {
                if (r9.Value == null)
                    return null;

                return r9.Value.ToString();
            }

            return null;
        }

        private static string GetTypeFromJson(JProperty r7)
        {
            var r8 = r7.First;
            ;
            if (r8 is JValue r9)
            {
                if (r9.Value == null)
                    return null;

                return r9.Value.ToString();
            }

            return null;
        }

        private static long? GetIdFromJson(JProperty r7)
        {
            var r8 = r7.First;

            if (r8 is JValue r9)
            {
                try
                {
                    return Convert.ToInt64(r9.Value);
                }
                catch
                {
                    ;
                }
            }

            return null;
        }

        private static void FillAssoc()
        {
            //read assoc from polinetwork python config file and fill db
            var s = File.ReadAllText("../../../Old/config/assoc.json");
            var r = JsonConvert.DeserializeObject<JObject>(s);
            var r2 = r.Children();
            foreach (var r3 in r2)
                if (r3 is JProperty r4)
                {
                    var name = r4.Name;
                    var r5 = r4.Value;
                    var users = GetUsersFromAssocJson(r5);
                    AddAssocToDb(name, users);
                }
        }

        private static bool AddAssocToDb(string name, IReadOnlyCollection<long> users)
        {
            const string q1 = "INSERT INTO Entities (Name) VALUES (@name)";
            _ = SqLite.Execute(q1, new Dictionary<string, object> { { "@name", name } });

            Tables.FixIdTable("Entities", "id", "name");

            const string q2 = "SELECT id FROM Entities WHERE Name = @name";
            var r2 = SqLite.ExecuteSelect(q2, new Dictionary<string, object> { { "@name", name } });

            var r3 = SqLite.GetFirstValueFromDataTable(r2);
            int? r4 = null;
            try
            {
                r4 = Convert.ToInt32(r3);
            }
            catch
            {
                ;
            }

            if (r4 == null)
                return false;

            if (users == null)
                return true;

            if (users.Count == 0)
                return true;

            foreach (var u in users)
            {
                const string q3 = "INSERT INTO PeopleInEntities (id_entity, id_person) VALUES (@ide, @idp)";
                _ = SqLite.Execute(q3, new Dictionary<string, object> { { "@ide", r4.Value }, { "@idp", u } });
            }

            return true;
        }

        private static List<long> GetUsersFromAssocJson(JToken r1)
        {
            var r2 = r1.Children();
            foreach (var r3 in r2)
                if (r3 is JProperty r4)
                    if (r4.Name == "users")
                    {
                        var r5 = r4.Value;
                        if (!(r5 is JArray r6))
                            continue;

                        var users = new List<long>();
                        foreach (var r7 in r6)
                            if (r7 is JValue r8)
                                users.Add(Convert.ToInt64(r8.Value));
                        return users;
                    }

            return null;
        }
    }
}