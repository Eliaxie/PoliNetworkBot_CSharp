﻿#region

using PoliNetworkBot_CSharp.Code.Enums;
using System;
using Telegram.Bot.Types.ReplyMarkups;
using TeleSharp.TL;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects
{
    public class ReplyMarkupObject
    {
        private readonly ReplyMarkupOptions _list;
        private readonly ReplyMarkupEnum _replyMarkupEnum;
        private InlineKeyboardMarkup inlineKeyboardMarkup;

        public ReplyMarkupObject(ReplyMarkupOptions list)
        {
            _list = list;
            _replyMarkupEnum = ReplyMarkupEnum.CHOICE;
        }

        public ReplyMarkupObject(ReplyMarkupEnum replyMarkupEnum)
        {
            _replyMarkupEnum = replyMarkupEnum;
        }

        public ReplyMarkupObject(InlineKeyboardMarkup inlineKeyboardMarkup)
        {
            this.inlineKeyboardMarkup = inlineKeyboardMarkup;
            this._replyMarkupEnum = ReplyMarkupEnum.INLINE;
        }

        public IReplyMarkup GetReplyMarkupBot()
        {
            return _replyMarkupEnum switch
            {
                ReplyMarkupEnum.FORCED => new ForceReplyMarkup(),
                ReplyMarkupEnum.REMOVE => new ReplyKeyboardRemove(),
                ReplyMarkupEnum.CHOICE => new ReplyKeyboardMarkup(_list.GetMatrixKeyboardButton(), oneTimeKeyboard: true),
                ReplyMarkupEnum.INLINE => inlineKeyboardMarkup,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public TLAbsReplyMarkup GetReplyMarkupUserBot()
        {
            return _replyMarkupEnum switch
            {
                ReplyMarkupEnum.FORCED => new TLReplyKeyboardForceReply(),
                ReplyMarkupEnum.REMOVE => new TLReplyKeyboardHide(),
                ReplyMarkupEnum.CHOICE => new TLReplyKeyboardMarkup { Rows = _list.GetMatrixTlKeyboardButton() },
                ReplyMarkupEnum.INLINE => GetRowsInline(inlineKeyboardMarkup),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private TLReplyInlineMarkup GetRowsInline(InlineKeyboardMarkup inlineKeyboardMarkup)
        {
            TLVector<TLKeyboardButtonRow> r2 = null;

            foreach (System.Collections.Generic.IEnumerable<InlineKeyboardButton> x1 in inlineKeyboardMarkup.InlineKeyboard)
            {
                TLVector<TLAbsKeyboardButton> buttons = new TLVector<TLAbsKeyboardButton>();

                foreach (var x2 in x1)
                {
                    TLKeyboardButton x3 = new TLKeyboardButton() { Text = x2.Text };
                    buttons.Add(x3);
                }

                TLKeyboardButtonRow tLKeyboardButtonRow = new TLKeyboardButtonRow() { Buttons = buttons };
                r2.Add(tLKeyboardButtonRow);
            }

            TLReplyInlineMarkup r = new TLReplyInlineMarkup() { Rows = r2 };
            return r;
        }

        internal InlineKeyboardMarkup ToInlineKeyBoard()
        {
            return inlineKeyboardMarkup;
        }
    }
}