﻿#region

using PoliNetworkBot_CSharp.Code.Errors;
using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class RestrictUser
    {
        internal static async Task Mute(int time, TelegramBotAbstract telegramBotClient, long chatId, int userId, ChatType chatType)
        {
            var untilDate = DateTime.Now.AddSeconds(time);
            await Mute2Async(untilDate, telegramBotClient, chatId, userId, chatType);
        }

        private static async Task Mute2Async(DateTime? untilDate, TelegramBotAbstract telegramBotClient, long chatId, int userId, ChatType? chatType)
        {
            var permissions = new ChatPermissions
            {
                CanSendMessages = false,
                CanInviteUsers = true,
                CanSendOtherMessages = false,
                CanSendPolls = false,
                CanAddWebPagePreviews = false,
                CanChangeInfo = false,
                CanPinMessages = false,
                CanSendMediaMessages = false
            };

            if (untilDate == null)
            {
                await telegramBotClient.RestrictChatMemberAsync(chatId, userId, permissions, default, chatType);
            }
            else
            {
                await telegramBotClient.RestrictChatMemberAsync(chatId, userId, permissions, untilDate.Value, chatType);
            }
        }

        internal static async Task<Tuple<BanUnbanAllResult, List<ExceptionNumbered>, int>> BanAllAsync(TelegramBotAbstract sender, MessageEventArgs e,
            string target, Enums.RestrictAction banTarget, DateTime? until)
        {
            UserIdFound targetId = await Info.GetTargetUserIdAsync(target, sender);
            if (targetId == null || targetId.GetID() == null)
            {
                string exception2 = "";
                if (targetId != null)
                {
                    exception2 += "\n" + targetId.getError();
                }

                var text2 = new Language(new Dictionary<string, string>
                {
                    {
                        "en", "We were not able to BanAll the target '" + target + "', error code " +
                              ErrorCodes.TargetInvalidWhenBanAll + exception2
                    },
                    {
                        "it", "Non siamo riusciti a bannareAll il target '" + target + "', error code " +
                              ErrorCodes.TargetInvalidWhenBanAll + exception2
                    }
                });
                await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                    e.Message.From.LanguageCode,
                    e.Message.From.Username,
                    text2,
                    parseMode: ParseMode.Default,
                    e.Message.MessageId);
                return null;
            }

            const string q1 = "SELECT id, type FROM Groups";
            var dt = SqLite.ExecuteSelect(q1);
            if (dt == null || dt.Rows.Count == 0)
            {
                var text3 = new Language(new Dictionary<string, string>
                {
                    {
                        "en", "We were not able to BanAll the target '" + target + "', error code " +
                              ErrorCodes.DatatableEmptyWhenBanAll
                    },
                    {
                        "it", "Non siamo riusciti a bannareAll il target '" + target + "', error code " +
                              ErrorCodes.DatatableEmptyWhenBanAll
                    }
                });
                await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                    e.Message.From.LanguageCode,
                    e.Message.From.Username,
                    text3,
                    parseMode: ParseMode.Default,
                    e.Message.MessageId);
                return null;
            }

            await AlertActionStartedAsync(sender, target, e);

            var done = new List<DataRow>();
            var failed = new List<DataRow>();

            List<ExceptionNumbered> exceptions = new List<ExceptionNumbered>();

            int nExceptions = 0;

            const int TIME_SLEEP_BETWEEN_BAN_UNBAN = 10;

            switch (banTarget)
            {
                case Enums.RestrictAction.BAN:
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            Thread.Sleep(TIME_SLEEP_BETWEEN_BAN_UNBAN);
                            try
                            {
                                var groupChatId = (long)dr["id"];
                                PoliNetworkBot_CSharp.Code.Objects.SuccessWithException success = await BanUserFromGroup(sender, e, targetId.GetID().Value, groupChatId, null);
                                if (success.IsSuccess())
                                    done.Add(dr);
                                else
                                    failed.Add(dr);

                                if (success.ContainsExceptions())
                                {
                                    nExceptions += AddExceptionIfNeeded(ref exceptions, success.GetFirstException());
                                }
                            }
                            catch
                            {
                                ;
                            }
                        }
                        break;
                    }

                case Enums.RestrictAction.UNBAN:
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            Thread.Sleep(TIME_SLEEP_BETWEEN_BAN_UNBAN);
                            try
                            {
                                var groupChatId = (long)dr["id"];
                                PoliNetworkBot_CSharp.Code.Objects.SuccessWithException success = await UnBanUserFromGroup(sender, e, targetId.GetID().Value, groupChatId);
                                if (success.IsSuccess())
                                    done.Add(dr);
                                else
                                    failed.Add(dr);

                                if (success.ContainsExceptions())
                                {
                                    nExceptions += AddExceptionIfNeeded(ref exceptions, success.GetFirstException());
                                }
                            }
                            catch
                            {
                                ;
                            }
                        }
                        break;
                    }

                case Enums.RestrictAction.MUTE:
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            Thread.Sleep(TIME_SLEEP_BETWEEN_BAN_UNBAN);
                            try
                            {
                                var groupChatId = (long)dr["id"];
                                ChatType? chatType = GetChatType(dr);
                                PoliNetworkBot_CSharp.Code.Objects.SuccessWithException success = await MuteUser(sender, e, targetId.GetID().Value, groupChatId, until, chatType);
                                if (success.IsSuccess())
                                    done.Add(dr);
                                else
                                    failed.Add(dr);

                                if (success.ContainsExceptions())
                                {
                                    nExceptions += AddExceptionIfNeeded(ref exceptions, success.GetFirstException());
                                }
                            }
                            catch
                            {
                                ;
                            }
                        }
                        break;
                    }
            }

            LogBanAction(targetId.GetID().Value, banned_true_unbanned_false: banTarget, bot: sender, who_banned: e.Message.From.Id);

            int? targetId2 = targetId.GetID();
            Tuple<List<ExceptionNumbered>, int> r6 = new Tuple<List<ExceptionNumbered>, int>(exceptions, nExceptions);
            if (targetId2 == null)
            {
                await NotifyUtil.NotifyOwnersAsync(r6, sender, "Ban/Unban All of [UNKNOWN]",
                    e.Message.From.LanguageCode);
            }
            else
            {
                string link2 = "tg://user?id=" + targetId2.Value.ToString();
                await NotifyUtil.NotifyOwnersAsync(r6, sender, "Ban/Unban All of [<a href='" + link2 + "'>" + targetId2.Value.ToString() + "</a>]",
                    e.Message.From.LanguageCode);
            }

            BanUnbanAllResult r5 = new BanUnbanAllResult(done, failed);
            return new Tuple<BanUnbanAllResult, List<ExceptionNumbered>, int>(r5, exceptions, nExceptions);
        }

        private static ChatType? GetChatType(DataRow dr)
        {
            try
            {
                var o1 = dr["type"].ToString().ToLower();
                if (o1 == null)
                    return null;
                if (o1 == "supergroup")
                    return ChatType.Supergroup;
                if (o1 == "group")
                    return ChatType.Group;
                if (o1 == "channel")
                    return ChatType.Channel;
                if (o1 == "private")
                    return ChatType.Private;
            }
            catch
            {
                ;
            }

            return null;
        }

        private static async Task<PoliNetworkBot_CSharp.Code.Objects.SuccessWithException> MuteUser(TelegramBotAbstract sender, MessageEventArgs e, int value, long groupChatId, DateTime? until, ChatType? chatType)
        {
            try
            {
                await Mute2Async(until, sender, groupChatId, value, chatType);
            }
            catch (Exception ex)
            {
                return new PoliNetworkBot_CSharp.Code.Objects.SuccessWithException(false, ex);
            }

            return new PoliNetworkBot_CSharp.Code.Objects.SuccessWithException(true);
        }

        private static async Task AlertActionStartedAsync(TelegramBotAbstract sender, string target, MessageEventArgs e)
        {
            var text7 = new Language(new Dictionary<string, string>
                {
                    {
                        "en", "Action started: '" + target + "'"
                    },
                    {
                        "it", "Azione avviata: '" + target + "'"
                    }
                });
            await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                e.Message.From.LanguageCode,
                e.Message.From.Username,
                text7,
                parseMode: ParseMode.Default,
                e.Message.MessageId);
        }

        private static int AddExceptionIfNeeded(ref List<ExceptionNumbered> exceptions, ExceptionNumbered item2)
        {
            if (item2 == null)
                return 0;

            Tuple<bool, int> isPresent = FindIfPresentSimilarException(exceptions, item2);
            if (isPresent.Item1 == false)
                exceptions.Add(new ExceptionNumbered(item2));
            else
                exceptions[isPresent.Item2].Increment();

            return 1;
        }

        private static Tuple<bool, int> FindIfPresentSimilarException(List<ExceptionNumbered> exceptions, ExceptionNumbered item2)
        {
            for (int i = 0; i < exceptions.Count; i++)
            {
                ExceptionNumbered e1 = exceptions[i];
                if (AreTheySimilar(e1, item2))
                {
                    return new Tuple<bool, int>(true, i);
                }
            }

            return new Tuple<bool, int>(false, -1);
        }

        private static bool AreTheySimilar(ExceptionNumbered e1, Exception item2)
        {
            return e1.AreTheySimilar(item2);
        }

        private static bool LogBanAction(int targetId, Enums.RestrictAction banned_true_unbanned_false, TelegramBotAbstract bot, int who_banned)
        {
            if (banned_true_unbanned_false == Enums.RestrictAction.BAN || banned_true_unbanned_false == Enums.RestrictAction.UNBAN)
            {
                // ban/unban action

                try
                {
                    bool? b = null;
                    if (banned_true_unbanned_false == Enums.RestrictAction.BAN)
                    {
                        b = true;
                    }
                    else if (banned_true_unbanned_false == Enums.RestrictAction.UNBAN)
                    {
                        b = false;
                    }

                    string q = "INSERT INTO Banned (from_bot_id, who_banned, when_banned, target, banned_true_unbanned_false) " +
                        " VALUES (@fbi, @whob, @whenb, @target, @btuf)";

                    Dictionary<string, object> dict = new Dictionary<string, object>() {
                        {"@fbi", bot.GetId() },
                        {"@whob", who_banned },
                        {"@whenb", DateTime.Now },
                        {"@target", targetId },
                        {"@btuf", Utils.StringUtil.ToSN(b)}
                    };
                    int done = Utils.SqLite.Execute(q, dict);

                    if (done > 0)
                        return true;

                    return false;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                // mute/unmute action
                //TODO
                return false;
            }
        }

        private static async Task<PoliNetworkBot_CSharp.Code.Objects.SuccessWithException> UnBanUserFromGroup(TelegramBotAbstract sender, MessageEventArgs e, int target,
            long groupChatId)
        {
            return await sender.UnBanUserFromGroup(target, groupChatId, e);
        }

        public static async Task<PoliNetworkBot_CSharp.Code.Objects.SuccessWithException> BanUserFromGroup(TelegramBotAbstract sender, MessageEventArgs e, long target,
            long groupChatId, string[] time)
        {
            return await sender.BanUserFromGroup(target, groupChatId, e, time);
        }
    }
}