﻿#region

using System;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;
using TLSharp.Core;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.TelegramMedia
{
    public class ObjectVideo : GenericMedia
    {
        private readonly long _chatId;
        private readonly ChatType _chatType;
        private readonly int _durationVideo;
        private readonly string _uniqueId;
        private int _fileSize;
        private int _idVideoDb;
        private int? _messageIdFrom;

        public ObjectVideo(int idVideoDb, string fileId, int fileSize, int height, int width,
            string uniqueId, int? messageIdFrom, long chatId, ChatType chatType, int durationVideo)
        {
            _idVideoDb = idVideoDb;
            _fileId = fileId;
            _fileSize = fileSize;
            _height = height;
            _width = width;
            _uniqueId = uniqueId;
            _messageIdFrom = messageIdFrom;
            _chatId = chatId;
            _chatType = chatType;
            _durationVideo = durationVideo;
        }

        public int GetDuration()
        {
            return _durationVideo;
        }

        public override MessageType? GetMediaBotType()
        {
            return MessageType.Video;
        }

        public int GetHeight()
        {
            return _height;
        }

        public int GetWidth()
        {
            return _width;
        }

        public async Task<TLAbsInputFile> GetTelegramUserBotInputVideo(TelegramClient userbotClient)
        {
            throw new NotImplementedException();
        }

        public TLAbsInputMedia GetTLabsInputMedia()
        {
            TLAbsInputMedia r = new TLInputMediaPhoto();

            return r;
        }
    }
}