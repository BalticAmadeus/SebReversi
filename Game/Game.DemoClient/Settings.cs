using Game.DebugClient.Utilites;
using GameLogic;

namespace Game.DemoClient
{
    public static class Settings
    {
        public static string ServerUrl { get; set; }
        public static string TeamName { get; set; }
        public static string UserName { get; set; }
        public static string Password { get; set; }

        public static string AuthCode
        {
            get
            {
                var authString = $"{TeamName}:{UserName}:{SessionId}:{SequenceNumber}{Password}";
                return AuthCodeManager.GetAuthCode(authString);
            }
        }

        public static int SequenceNumber { get; set; }
        public static int SessionId { get; set; }
        public static string Challenge { get; set; }

        public static string ChallengeResponse => AuthCodeManager.GetAuthCode($"{AuthCodeManager.GetAuthCode($"{Challenge}{Password}")}{Password}");
        public static int PlayerId { get; set; }
        public static int GameId { get; set; }
        public static int Turn { get; set; }
        public static string[] Map { get; set; }
        public static MapData MapData { get; set; }
        public static TileType PlayerType { get; set; }
    }
}
