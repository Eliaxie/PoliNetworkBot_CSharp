﻿#region

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot;
using Telegram.Bot.Args;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation
{
    internal static class Main
    {
        internal static void MainMethod(object sender, MessageEventArgs e)
        {
            var t = new Thread(() => _ = MainMethod2(sender, e));
            t.Start();
        }

        private static async Task MainMethod2(object sender, MessageEventArgs e)
        {
            TelegramBotClient telegramBotClientBot = null;
            TelegramBotAbstract telegramBotClient = null;

            try
            {

                if (sender is TelegramBotClient tmp) telegramBotClientBot = tmp;

                if (telegramBotClientBot == null)
                    return;

                telegramBotClient  = TelegramBotAbstract.GetFromRam(telegramBotClientBot);

                var toExit = await ModerationCheck.CheckIfToExitAndUpdateGroupList(telegramBotClient, e);
                if (toExit == ToExit.EXIT)
                {
                    await LeaveChat.ExitFromChat(telegramBotClient, e);
                    return;
                }

                List<long> NotAuthorizedBotHasBeenAddedBool = await ModerationCheck.CheckIfNotAuthorizedBotHasBeenAdded(e, telegramBotClient);
                if (NotAuthorizedBotHasBeenAddedBool != null && NotAuthorizedBotHasBeenAddedBool.Count > 0)
                {
                    foreach (var bot in NotAuthorizedBotHasBeenAddedBool)
                    {
                        await Utils.RestrictUser.BanUserFromGroup(telegramBotClient, e, bot, e.Message.Chat.Id, null);
                    }

                    //todo: send messagge "Bots not allowed here!"
                }

                var toExitBecauseUsernameAndNameCheck = await ModerationCheck.CheckUsernameAndName(e, telegramBotClient);
                if (toExitBecauseUsernameAndNameCheck)
                    return;

                var checkSpam = ModerationCheck.CheckSpam(e);
                if (checkSpam != SpamType.ALL_GOOD)
                {
                    await ModerationCheck.AntiSpamMeasure(telegramBotClient, e, checkSpam);
                    return;
                }

                if (e.Message.Text != null && e.Message.Text.StartsWith("/"))
                    await CommandDispatcher.CommandDispatcherMethod(telegramBotClient, e);
                else
                    await TextConversation.DetectMessage(telegramBotClient, e);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);

                await Utils.NotifyUtil.NotifyOwners(exception, telegramBotClient);
            }
        }
    }
}