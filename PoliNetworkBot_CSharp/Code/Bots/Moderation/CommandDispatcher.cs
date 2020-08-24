﻿using PoliNetworkBot_CSharp.Data;
using PoliNetworkBot_CSharp.Utils;
using System;
using System.IO;
using System.Threading;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Bots.Moderation
{
    internal class CommandDispatcher
    {
        public static void CommandDispatcherMethod(TelegramBotAbstract sender, MessageEventArgs e)
        {
            var cmd_lines = e.Message.Text.Split(' ');
            string cmd = cmd_lines[0];
            switch (cmd)
            {
                case "/start":
                    {
                        Start(sender, e);
                        return;
                    }

                case "/force_check_invite_links":
                    {
                        if (Data.GlobalVariables.Creators.Contains(e.Message.Chat.Id))
                        {
                            _ = ForceCheckInviteLinksAsync(sender, e);
                        }
                        else
                        {
                            DefaultCommand(sender, e);
                        }
                        return;
                    }

                case "/contact":
                    {
                        ContactUs(sender, e);
                        return;
                    }

                case "/help":
                    {
                        Help(sender, e);
                        return;
                    }

                case "/banAll":
                    {
                        if (GlobalVariables.Creators.Contains(e.Message.From.Id))
                        {
                            _ = BanAllAsync(sender, e, target: cmd_lines);
                        }
                        else
                        {
                            DefaultCommand(sender, e);
                        }
                        return;
                    }

                case "/ban":
                    {
                        _ = BanUserAsync(sender, e, cmd_lines);
                        return;
                    }

                case "/unbanAll":
                    {
                        if (GlobalVariables.Creators.Contains(e.Message.From.Id))
                        {
                            _ = UnbanAllAsync(sender, e, target: cmd_lines[1]);
                        }
                        else
                        {
                            DefaultCommand(sender, e);
                        }
                        return;
                    }

                case "/getGroups":
                    {
                        if (GlobalVariables.Creators.Contains(e.Message.From.Id) && e.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Private)
                        {
                            System.Data.DataTable groups = Utils.Groups.GetAllGroups();
                            Stream stream = new MemoryStream();
                            Utils.FileSerialization.SerializeFile(groups, ref stream);
                            _ = SendMessage.SendFileAsync(File: new TelegramFile(stream, "groups.bin"), chat_id: e.Message.Chat.Id,
                                text: "Here are all groups:", text_as_caption: Enums.TextAsCaption.BEFORE_FILE,
                                TelegramBot_Abstract: sender);
                        }
                        else
                        {
                            DefaultCommand(sender, e);
                        }
                        return;
                    }

                case "/time":
                    {
                        Utils.SendMessage.SendMessageInPrivate(sender, e, Utils.DateTimeClass.NowAsStringAmericanFormat());
                        return;
                    }

                case "/assoc_send":
                    {
                        _ = Assoc_SendAsync(sender, e);
                        return;
                    }

                default:
                    {
                        DefaultCommand(sender, e);
                        return;
                    }
            }
        }

        private static async System.Threading.Tasks.Task<bool> Assoc_SendAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            var reply_to = e.Message.ReplyToMessage;

            System.Collections.Generic.Dictionary<string, string> language_list = new System.Collections.Generic.Dictionary<string, string>() {
                {"it", "Scegli l'entità per il quale stai componendo il messaggio" },
                {"en", "Choose the entity you are writing this message for" }
            };
            int? message_from_id_entity = await Utils.Assoc.GetIDEntityFromPersonAsync(e.Message.From.Id, language_list, sender, e.Message.From.LanguageCode);
            DateTime? sent_date = await Utils.DateTimeClass.AskDateAsync(e.Message.From.Id, e.Message.Text, e.Message.From.LanguageCode, sender);
            long id_chat_sent_into = Data.Constants.Channels.PoliAssociazioni;

            if (reply_to.Photo != null)
            {
                Telegram.Bot.Types.PhotoSize photo_large = Utils.Photo.GetLargest(reply_to.Photo);
                int? photo_id_db = Utils.Photo.AddPhotoToDB(photo_large);
                if (photo_id_db == null)
                    return false;

                MessageDB.AddMessage(type: MessageType.Photo,
                    message_text: reply_to.Caption, message_from_id_person: e.Message.From.Id,
                    message_from_id_entity: message_from_id_entity, photo_id: photo_id_db.Value,
                    id_chat_sent_into: id_chat_sent_into, sent_date: sent_date);
            }
            else
            {
                sender.SendTextMessageAsync(e.Message.From.Id, "You have to attach something! (A photo, for example)", ChatType.Private);
                return false;
            }

            sender.SendTextMessageAsync(e.Message.From.Id, "The message has been submitted correctly", ChatType.Private);
            return true;
        }

        private static async System.Threading.Tasks.Task<bool> BanUserAsync(TelegramBotAbstract sender, MessageEventArgs e, string[] string_info)
        {
            bool r = await Groups.CheckIfAdminAsync(e.Message.From.Id, chat_id: e.Message.Chat.Id, sender);
            if (r)
            {
                if (e.Message.ReplyToMessage == null)
                {
                    var target_int = await Utils.Info.GetTargetUserIdAsync(string_info[1], sender);
                    if (target_int == null)
                        return false;

                    return Utils.RestrictUser.BanUserFromGroup(sender, e, target_int.Value, e.Message.Chat.Id, time: null);
                }
                else
                {
                    var target_int = e.Message.ReplyToMessage.From.Id;
                    return Utils.RestrictUser.BanUserFromGroup(sender, e, target_int, e.Message.Chat.Id, time: string_info);
                }
            }

            return false;
        }

        private static async System.Threading.Tasks.Task UnbanAllAsync(TelegramBotAbstract sender, MessageEventArgs e, string target)
        {
            var done = await RestrictUser.BanAllAsync(sender, e, target, false);
            Utils.SendMessage.SendMessageInPrivate(sender, e,
                "Target unbanned from " + done.Count.ToString() + " groups");
        }

        private static async System.Threading.Tasks.Task BanAllAsync(TelegramBotAbstract sender, MessageEventArgs e, string[] target)
        {
            if (e.Message.ReplyToMessage == null)
            {
                if (target.Length < 2)
                {
                    sender.SendTextMessageAsync(e.Message.From.Id, "We can't find the target.", Telegram.Bot.Types.Enums.ChatType.Private);
                }
                else
                {
                    var done = await RestrictUser.BanAllAsync(sender, e, target[1], true);
                    Utils.SendMessage.SendMessageInPrivate(sender, e,
                        "Target banned from " + done.Count.ToString() + " groups");
                }
            }
            else
            {
                var done = await RestrictUser.BanAllAsync(sender, e, target: e.Message.ReplyToMessage.From.Id.ToString(), true);
                Utils.SendMessage.SendMessageInPrivate(sender, e,
                    "Target banned from " + done.Count.ToString() + " groups");
            }
        }

        private static void DefaultCommand(TelegramBotAbstract sender, MessageEventArgs e)
        {
            Utils.SendMessage.SendMessageInPrivate(sender, e, "Mi dispiace, ma non conosco questo comando. Prova a contattare gli amministratori (/contact)");
        }

        private static void Help(TelegramBotAbstract sender, MessageEventArgs e)
        {
            if (e.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Private)
            {
                HelpPrivate(sender, e);
            }
            else
            {
                Utils.SendMessage.SendMessageInPrivateOrAGroup(sender, e, "Questo messaggio funziona solo in chat privata");
            }
        }

        private static void HelpPrivate(TelegramBotAbstract sender, MessageEventArgs e)
        {
            string text = "<i>Lista di funzioni</i>:\n" +
                                      "\n📑 Sistema di recensioni dei corsi (per maggiori info /help_review)\n" +
                                      "\n🔖 Link ai materiali nei gruppi (per maggiori info /help_material)\n" +
                                      "\n🙋 <a href='https://polinetwork.github.io/it/faq/index.html'>" +
                                      "FAQ (domande frequenti)</a>\n" +
                                      "\n🏫 Bot ricerca aule libere @AulePolimiBot\n" +
                                      "\n🕶️ Sistema di pubblicazione anonima (per maggiori info /help_anon)\n" +
                                      "\n🎙️ Registrazione delle lezioni (per maggiori info /help_record)\n" +
                                      "\n👥 Gruppo consigliati e utili /groups\n" +
                                      "\n⚠ Hai già letto le regole del network? /rules\n" +
                                      "\n✍ Per contattarci /contact";
            Utils.SendMessage.SendMessageInPrivate(sender, e, text, Telegram.Bot.Types.Enums.ParseMode.Html);
        }

        private static void ContactUs(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            Utils.DeleteMessage.DeleteIfMessageIsNotInPrivate(telegramBotClient, e);
            telegramBotClient.SendTextMessageAsync(e.Message.Chat.Id,
                    telegramBotClient.GetContactString(), e.Message.Chat.Type
                );
        }

        private static async System.Threading.Tasks.Task ForceCheckInviteLinksAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            int n = await Utils.InviteLinks.FillMissingLinksIntoDB_Async(sender);
            Utils.SendMessage.SendMessageInPrivate(sender, e, "I have updated n=" + n.ToString() + " links");
        }

        private static void Start(TelegramBotAbstract telegramBotClient, MessageEventArgs e)
        {
            Utils.DeleteMessage.DeleteIfMessageIsNotInPrivate(telegramBotClient, e);
            telegramBotClient.SendTextMessageAsync(e.Message.Chat.Id,
                    "Ciao! 👋\n" +
                    "\nScrivi /help per la lista completa delle mie funzioni 👀\n" +
                    "\nVisita anche il nostro sito " + telegramBotClient.GetWebSite(),
                     e.Message.Chat.Type
                );
        }
    }
}