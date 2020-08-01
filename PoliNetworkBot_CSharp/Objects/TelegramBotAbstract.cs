﻿using PoliNetworkBot_CSharp.Bots.Enums;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace PoliNetworkBot_CSharp
{
    public class TelegramBotAbstract
    {
        private readonly TelegramBotClient botClient;
        private readonly bool isbot;
        private readonly string website;
        private readonly string contactString;

        public TelegramBotAbstract(TelegramBotClient botClient, string website, string contactString)
        {
            this.botClient = botClient;
            this.isbot = true;
            this.website = website;
            this.contactString = contactString;
        }

        internal string GetWebSite()
        {
            return this.website;
        }

        internal static TelegramBotAbstract GetFromRam(TelegramBotClient telegramBotClient_bot)
        {
            return Data.GlobalVariables.Bots[telegramBotClient_bot.BotId];
        }

        internal void DeleteMessageAsync(long id, int messageId)
        {
            if (isbot)
            {
                this.botClient.DeleteMessageAsync(id, messageId);
            }
        }

        internal int? GetIDFromUsername(string target)
        {
            if (isbot)
            {
                //bot api does not allow that
                return null;
            }
            else
            {
                //todo
                return null;
            }
        }

        internal void RestrictChatMemberAsync(long chat_id, int user_id, ChatPermissions permissions, DateTime untilDate)
        {
            if (isbot)
            {
                this.botClient.RestrictChatMemberAsync(chat_id, user_id, permissions, untilDate);
            }
        }

        internal bool SendTextMessageAsync(long chatid, string text, Telegram.Bot.Types.Enums.ParseMode v = Telegram.Bot.Types.Enums.ParseMode.Default)
        {
            if (isbot)
            {
                this.botClient.SendTextMessageAsync(chatid, text, v);
                return true;
            }
            else
            {
                return false;
            }
        }

        internal bool SendFile(object file, long chat_id, string text, TextAsCaption text_as_caption, string fileName)
        {
            if (isbot)
            {
                System.IO.Stream stream_content = Utils.StreamSerialization.SerializeToStream(file);
                Telegram.Bot.Types.InputFiles.InputOnlineFile document_input = new Telegram.Bot.Types.InputFiles.InputOnlineFile(content: stream_content, fileName);
                switch (text_as_caption)
                {
                    case TextAsCaption.AS_CAPTION:
                        {

                            this.botClient.SendDocumentAsync(chat_id, document_input, caption: text);
                            return true;
                        }

                    case TextAsCaption.BEFORE_FILE:
                        {
                            this.botClient.SendTextMessageAsync(chat_id, text);
                            this.botClient.SendDocumentAsync(chat_id, document_input);
                            return true;
                        }

                    case TextAsCaption.AFTER_FILE:
                        {
                            this.botClient.SendDocumentAsync(chat_id, document_input);
                            this.botClient.SendTextMessageAsync(chat_id, text);
                            return true;
                        }
                }

            }
            else
            {
                return false;
            }

            return false;
        }

        internal string GetContactString()
        {
            return this.contactString;
        }

        internal async Task<string> ExportChatInviteLinkAsync(long chat_id)
        {
            if (isbot)
            {
                return await this.botClient.ExportChatInviteLinkAsync(chat_id);
            }

            return null;
        }

        internal long? GetBotID()
        {
            if (isbot)
            {
                return this.botClient.BotId;
            }

            return null;
        }

        internal bool BanUserFromGroup(int target, long group_chat_id, MessageEventArgs e)
        {
            if (isbot)
            {
                try
                {
                    this.botClient.KickChatMemberAsync(group_chat_id, target);
                }
                catch
                {
                    return false;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        internal Task<Telegram.Bot.Types.ChatMember[]> GetChatAdministratorsAsync(long id)
        {
            if (isbot)
            {
                return this.botClient.GetChatAdministratorsAsync(id);
            }

            return null;
        }

        internal bool UnBanUserFromGroup(int target, long group_chat_id, MessageEventArgs e)
        {
            if (isbot)
            {
                try
                {
                    this.botClient.UnbanChatMemberAsync(group_chat_id, target);
                }
                catch
                {
                    return false;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        internal void LeaveChatAsync(long id)
        {
            if (isbot)
            {
                this.botClient.LeaveChatAsync(id);
            }
        }
    }
}