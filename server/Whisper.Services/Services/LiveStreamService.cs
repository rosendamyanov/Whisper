//using Whisper.Common.Response;
//using Whisper.Common.Response.Chat;
//using Whisper.Common.Response.LiveStream;
//using Whisper.Data.Repositories.Interfaces;
//using Whisper.DTOs.Request.LiveStream;
//using Whisper.Services.Factories.Interfaces;
//using Whisper.Services.Services.Interfaces;

//namespace Whisper.Services.Services
//{
//    public class LiveStreamService : ILiveStreamService
//    {
//        private readonly ILiveStreamRepository _liveStreamRepository;
//        private readonly ILiveStreamFactory _liveStreamFactory;
//        private readonly IChatRepository _chatRepository;
//        public LiveStreamService(ILiveStreamRepository liveStreamRepository, ILiveStreamFactory liveStreamFactory, IChatRepository chatRepository)
//        {
//            _liveStreamRepository = liveStreamRepository;
//            _liveStreamFactory = liveStreamFactory;
//            _chatRepository = chatRepository;
//        }
//        public async Task<ApiResponse<LiveStreamResponseDto>> GetActiveStreamAsync(Guid chatId, Guid userId)
//        {
//            var isUserInChat = await _chatRepository.IsUserInChatAsync(chatId, userId);
//            if (!isUserInChat)
//                return ApiResponse<LiveStreamResponseDto>.Failure(ChatMessages.NotChatMember, ChatMessages.NotChatMember);

//            var stream = await _liveStreamRepository.GetActiveStreamByChatIdAsync(chatId);
//            if (stream == null)
//                return ApiResponse<LiveStreamResponseDto>.Failure(LiveStreamMessages.NoActiveStream, LiveStreamCodes.NoActiveStream);

//            var streamDto = _liveStreamFactory.ToDto(stream);

//            return ApiResponse<LiveStreamResponseDto>.Success(streamDto);
//        }

//        public async Task<ApiResponse<LiveStreamResponseDto>> StartStreamAsync(Guid chatId, Guid userId)
//        {
//            var isUserInChat = await _chatRepository.IsUserInChatAsync(chatId, userId);
//            if (!isUserInChat)
//                return ApiResponse<LiveStreamResponseDto>.Failure(ChatMessages.NotChatMember, ChatMessages.NotChatMember);

//            var isAlreadyStreaming = await _liveStreamRepository.IsUserStreamingAsync(userId);
//            if (isAlreadyStreaming)
//                return ApiResponse<LiveStreamResponseDto>.Failure(LiveStreamMessages.AlreadyStreaming, LiveStreamCodes.AlreadyStreaming);

//            var existingStream = await _liveStreamRepository.GetActiveStreamByChatIdAsync(chatId);
//            if (existingStream != null)
//                return ApiResponse<LiveStreamResponseDto>.Failure(LiveStreamMessages.StreamAlreadyActive, LiveStreamCodes.StreamAlreadyActive);

//            var stream = _liveStreamFactory.Create(userId, chatId);
//            await _liveStreamRepository.StartStreamAsync(stream);

//            return ApiResponse<LiveStreamResponseDto>.Success(_liveStreamFactory.ToDto(stream));
//        }
//        public async Task<ApiResponse<string>> EndStreamAsync(Guid streamId, Guid userId)
//        {
//            var isUserStreaming = await _liveStreamRepository.IsUserStreamingAsync(userId);

//            if (!isUserStreaming)
//                return ApiResponse<string>.Failure(LiveStreamMessages.UserNotStreaming, LiveStreamCodes.UserNotStreaming);

//            var success = await _liveStreamRepository.EndStreamAsync(streamId, userId);
//            if (!success)
//                return ApiResponse<string>.Failure(LiveStreamMessages.EndStreamFailed, LiveStreamCodes.EndStreamFailed);

//            return ApiResponse<string>.Success(LiveStreamMessages.StreamEnded);
//        }
//    }
//}
