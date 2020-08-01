﻿using System;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace PoliNetworkBot_CSharp.Bots.Moderation
{
    internal class Main
    {
        internal static void MainMethod(object sender, MessageEventArgs e)
        {
            TelegramBotClient telegramBotClient = null;
            if (sender is TelegramBotClient tmp)
            {
                telegramBotClient = tmp;
            }

            if (telegramBotClient == null)
                return;

            bool to_exit = ModerationCheck.CheckIfToExitAndUpdateGroupList(telegramBotClient, e);
            if (to_exit)
            {
                ModerationCheck.ExitFromChat(telegramBotClient, e);
                return;
            }

            Tuple<bool, bool> check_username = ModerationCheck.CheckUsername(e);
            if (check_username.Item1 || check_username.Item2)
            {
                ModerationCheck.SendUsernameWarning(telegramBotClient, e, check_username.Item1, check_username.Item2);
                return;
            }

            SpamType check_spam = ModerationCheck.CheckSpam(e);
            if (check_spam != SpamType.ALL_GOOD)
            {
                ModerationCheck.AntiSpamMeasure(telegramBotClient, e, check_spam);
                return;
            }

            if (!string.IsNullOrEmpty(e.Message.Text))
            {
                if (e.Message.Text.StartsWith("/"))
                {
                    CommandDispatcher.CommandDispatcherMethod(telegramBotClient, e);
                }
            }
        }
    }
}