namespace AssemblyCSharp
{
    public static class PoolStaticStrings
    {
        // Services configration IDS
        public static string PlayFabTitleID = "E6EA3";
        public static string PhotonAppID = "e56b28b5-418c-4670-8ca0-05f0d44d8129";
        public static string PhotonChatID = "b3d9b0ac-6a85-41c9-9e24-e42247d47cba";

        // Admob Ads IDS
        public static string adMobAndroidID = "ca-app-pub-2726096586714737~2093174742";
        //public static string adMobiOSID = "ADMOB IOS ID";
        //public static string unityAdsIOSID = "UNITY ADS IOS ID";
        public static string unityAdsAndroidID = "8f12a32e-9bb7-43c8-985d-b2bf479b736c";

        // Facebook share variables
        public static string facebookShareLinkAndroid = "https://play.google.com/store/apps/details?id=com.pool";
        public static string facebookShareLinkAppStore = "https://itunes.apple.com/us/app/apple-store/id1111111111";
        public static string facebookShareLinkTitle = "I'm playing Pool!. Available on Android and iOS.";

        // Initial coins count for new players
        public static int initCoinsCount = 5000;

        // Unity Ads - reward coins count for watching video
        public static int rewardForVideoAd = 50;

        // Facebook Invite variables
        public static string facebookInviteMessage = "Come play this great game!";
        public static int rewardCoinsForFriendInvite = 50;
        public static int rewardCoinsForShareViaFacebook = 50;

        // String to add coins for testing - To add coins start game, click "Edit" button on your avatar and put that string
        // It will add 1 000 000 coins so you can test tables, buy items etc.
        public static string addCoinsHackString = "Cheat:AddCoins";

        // Where to show admob admob
        public static bool showAdOnSelectTableScene = true;
        public static bool showAdOnGameOverScene = false;
        public static bool showAdOnStoreScene = false;
        public static bool showAdOnFriendsScene = false;
        public static bool showAdWhenLeaveGame = false;

        // Hide Coins tab in shop (In-App Purchases)
        public static bool hideCoinsTabInShop = false;

        // Fault strings
        public static string faultCueBallDidntStrike = "Cue ball didn't strike another ball";
        public static string failedToHitStriped = "You failed to hit striped ball";
        public static string failedToHitSolid = "You failed to hit solid ball";
        public static string failedToHit8Ball = "You failed to hit 8 ball";
        public static string potedCueBall = "You poted cue ball";
        public static string invalidStrike = "Invalid Strike";
        public static string invalidShotNoBandContact = "No balls hit the ray after first contact";
        public static string runOutOfTime = "ran out of time";
        public static string waitingForOpponent = "Waiting for your opponent";

        // Other strings
        public static string youAreBreaking = "You are breaking, good luck";
        public static string opponentIsBreaking = "is breaking";
        public static string IWantPlayAgain = "I want to play again!";
        public static string cantPlayRightNow = "Can't play right now";
        public static string callPocket = "Call Pocket";
        public static string opponentCalledPocket = "called pocket";

        // Players names for training mode
        public static string offlineModePlayer1Name = "Player 1";
        public static string offlineModePlayer2Name = "Player 2";

        // Photon configuration
        // Timeout in second when player will be disconnected when game in background
        public static float photonDisconnectTimeout = 45.0f;

        // Standard chat messages
        public static string[] chatMessages = new string[] {
            "Good Luck",
            "Nice shot",
            "Well played",
            "You're good!",
            "Thanks",
            "Oops",
            "Unlucky",
            "Nice try",
            "Hehe",
            "Close!",
            "Good Game",
            "Sorry gotta run",
            "Nice Cue",
            "One more game?",
            "Hello!"
        };

        // Additional chat messages
        // Prices for chat packs
        public static int[] chatPrices = new int[] { 100, 500, 1000, 5000, 10000, 25000, 50000, 100000 };

        // Chat packs names
        public static string[] chatNames = new string[] { "Rematch", "Motivate", "Emoticons", "Cheers", "Break", "Gags", "Laughing", "Talking" };

        // Chat packs strings
        public static string[][] chatMessagesExtended = new string[][] {
            new string[] {
                "Let's play again",
                "I was so lucky",
                "Best of 3?",
                "Epic game!"
            },
            new string[] {
                "Never give up",
                "You can do it",
                "I know you have it in you!",
                "You play like a pro!",
                "You can win now!",
                "You're great!"
            },
            new string[] {
                ":)",
                ":(",
                ":o",
                ";D",
                ":P",
                ":|"
            },
            new string[] {
                "Keep it going",
                "Go opponents!",
                "Fabulastic",
                "You're awesome",
                "Best shot ever",
                "That was amazing",
            },
            new string[] {
                "I'll be back in a second",
                "Give me 2 mins, brb",
                "I need 5 min break",
                "That was fun!",
                "Just need 5 min break",
                "15 min break?"
            },
            new string[] {
                "OMG",
                "LOL",
                "ROFL",
                "O'RLY?!",
                "CYA",
                "YOLO"
            },
            new string[] {
                "Hahaha!!!",
                "Ho ho ho!!!",
                "Mwhahahaa",
                "Jejeje",
                "Booooo!",
                "Muuuuuuuhhh!"
            },
            new string[] {
                "Yes",
                "No",
                "I don't know",
                "Maybe",
                "Definitely",
                "Of course"
            }
        };

    }
}

