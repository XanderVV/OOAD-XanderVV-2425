using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WpfEscapeGame.MainWindow;

namespace WpfEscapeGame
{
    public static class RandomMessageGenerator
    {
        private static Random random = new Random();

        private static string[] normalMessages = { "That doesn't seem to work.", "Hmm, that didn't do anything.", "No luck with that." };
        private static string[] lockedMessages = { "It's firmly locked.", "Looks like it won't budge.", "I can't open it." };
        private static string[] itemNotFoundMessages = { "I can't find anything like that.", "It's not here.", "Hmm, not seeing it." };

        public static string GetRandomMessage(MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.Normal:
                    return normalMessages[random.Next(normalMessages.Length)];
                case MessageType.Locked:
                    return lockedMessages[random.Next(lockedMessages.Length)];
                case MessageType.ItemNotFound:
                    return itemNotFoundMessages[random.Next(itemNotFoundMessages.Length)];
                default:
                    return "Oops, something went wrong.";
            }
        }
    }
}
