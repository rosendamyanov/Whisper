namespace Whisper.Common.Response.LiveStream
{
    public static class LiveStreamMessages
    {
        public const string NoActiveStream = "No active stream found.";
        public const string AlreadyStreaming = "Already streaming elsewhere.";
        public const string StreamAlreadyActive = "Chat already has an active stream.";
        public const string UserNotStreaming = "User is not currently streaming.";
        public const string EndStreamFailed = "Failed to end the stream.";
        public const string StreamEnded = "Stream ended successfully.";
    }
}
