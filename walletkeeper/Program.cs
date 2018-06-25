using System;
using System.IO;
using System.Linq;
using System.Web;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using GoogleCloudSamples;
using System.Net;
using System.Collections.Generic;
using DataBaseCon = DataBase.Program;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;


namespace WalletKeeperBot
{
    class Program
    {
        private static Message message;
        private static double bill = 0;
        private static readonly TelegramBotClient Bot = new Telegram.Bot.TelegramBotClient(WalletKeeper.Config.API_KEY);

        private static String deleteNoDigit(String price)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < price.Length; i++)
                if (char.IsDigit(price[i]) || price[i] == '.' || price[i] == ',')
                    sb.Append(price[i]);
            return sb.ToString();
        }
        
        public static string ParseString(string str)
        {
            var splittedString = str.ToLower().Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var targetWords = new List<string>()
            {
                "итог", "итого", "итог:", "итого:", "итого≡", "итог≡"
            };
            if (!splittedString.Intersect(targetWords).Any())
                return "Не удалось обработать фотографию.\nПопробуй сделать фото еще раз.";
            else
            {
                float Sum = 0;
                int indexOfResult = 0;
                for (int i = 0; i < targetWords.Count; i++)
                {
                    indexOfResult = Array.LastIndexOf(splittedString, targetWords[i]);
                    if (indexOfResult > -1) break;
                }

                if (float.TryParse(deleteNoDigit(splittedString[indexOfResult - 1]).Substring(0, deleteNoDigit(splittedString[indexOfResult - 1]).Length), NumberStyles.Any, new CultureInfo("en-US"), out Sum) ||
                    float.TryParse(deleteNoDigit(splittedString[indexOfResult + 1]).Substring(0, deleteNoDigit(splittedString[indexOfResult + 1]).Length), NumberStyles.Any, new CultureInfo("en-US"), out Sum) ||
                    float.TryParse(deleteNoDigit(splittedString[indexOfResult - 1]).Substring(1, deleteNoDigit(splittedString[indexOfResult - 1]).Length), NumberStyles.Any, new CultureInfo("en-US"), out Sum) ||
                    float.TryParse(deleteNoDigit(splittedString[indexOfResult + 1]).Substring(1, deleteNoDigit(splittedString[indexOfResult + 1]).Length), NumberStyles.Any, new CultureInfo("en-US"), out Sum) ||
                    float.TryParse(deleteNoDigit(splittedString[indexOfResult + 2]).Substring(1, deleteNoDigit(splittedString[indexOfResult + 1]).Length), NumberStyles.Any, new CultureInfo("en-US"), out Sum))
                {
                    return $"{Sum}";
                }
                else
                {
                    return $"0";
                }
            }
        }

        

        static void Main(string[] args)
        {
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessage += BotOnPhotoReceived;
            Bot.OnCallbackQuery += BotOnCallbackQuery;

            var me = Bot.GetMeAsync().Result;
            Console.Title = me.Username;
       
            Bot.StartReceiving();
            Console.ReadLine();
            Bot.StopReceiving();
        }

        private static async void BotOnCallbackQuery(object sender, CallbackQueryEventArgs ev)
        {
            if (ev.CallbackQuery.Data == "по категориям")
            {
                await Bot.SendTextMessageAsync(message.Chat.Id, DataBaseCon.SelectRowsByCategory((int)message.Chat.Id));
                await Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id);
            }
            if (ev.CallbackQuery.Data == "по датам")
            {
                await Bot.SendTextMessageAsync(message.Chat.Id, DataBaseCon.SelectRowsByDates((int)message.Chat.Id));
                await Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id);
            }
            if (ev.CallbackQuery.Data == "Транспорт")
            {
                DataBaseCon.InsertAmount((int)message.Chat.Id, bill, "Транспорт");
                await Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id, "Расходы на транспорт зафиксированы");
                await Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id);
            }
            else if (ev.CallbackQuery.Data == "Мобильная связь")
            {
                DataBaseCon.InsertAmount((int)message.Chat.Id, bill, "Мобильная связь");
                await Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id, "Расходы на мобильную связь зафиксированы");
                await Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id);
            }
            else if (ev.CallbackQuery.Data == "Продукты питания")
            {
                DataBaseCon.InsertAmount((int)message.Chat.Id, bill, "Продукты питания");
                await Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id, "Расходы на продукты питания зафиксированы");
                await Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id);
            }
            else if (ev.CallbackQuery.Data == "Техника")
            {
                DataBaseCon.InsertAmount((int)message.Chat.Id, bill, "Техника");
                await Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id, "Расходы на технику зафиксированы");
                await Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id);
            }
            else if (ev.CallbackQuery.Data == "Другое")
            {
                DataBaseCon.InsertAmount((int)message.Chat.Id, bill, "Другое");
                await Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id, "Расходы на другую категорию зафиксированы");
                await Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id);
            }
        }

        private static void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            message = messageEventArgs.Message;

            if (message == null || message.Type != MessageType.TextMessage)
                return;

            if (message.Text.StartsWith("/help"))
            {
                Bot.SendTextMessageAsync(message.Chat.Id, WalletKeeper.Constants.HELP_MESSAGE);
            }

            
            if (message.Text.StartsWith("/spending"))
            {
                var keyboard2 = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup();
                keyboard2.InlineKeyboard = new Telegram.Bot.Types.InlineKeyboardButtons.InlineKeyboardButton[][]
                {
                    new Telegram.Bot.Types.InlineKeyboardButtons.InlineKeyboardButton[]
                    {
                        Telegram.Bot.Types.InlineKeyboardButtons.InlineKeyboardButton.WithCallbackData("по категориям"),
                        Telegram.Bot.Types.InlineKeyboardButtons.InlineKeyboardButton.WithCallbackData("по датам"),
                    },
                };

                Bot.SendTextMessageAsync(message.Chat.Id, "Выбери вид отчетности", replyMarkup: keyboard2);
            }

            if (message.Text.StartsWith("/start"))
            {
                DataBaseCon.InsertUser((int)message.Chat.Id, message.Chat.FirstName);
                Bot.SendTextMessageAsync(message.Chat.Id, $"Привет, {message.Chat.FirstName}" + WalletKeeper.Constants.START_MESSAGE);
            }
            if (message.Text.StartsWith("/delete"))
            {
                DataBaseCon.DeleteRows((int)message.Chat.Id);
                Bot.SendTextMessageAsync(message.Chat.Id, WalletKeeper.Constants.DELETE_DONE);
            }
        }

        private static async void BotOnPhotoReceived(object sender, MessageEventArgs messageEventArgs)
        {
            message = messageEventArgs.Message;
            try
            {
                if (message == null || message.Type != MessageType.PhotoMessage)
                    return;


                var fileId = message.Photo[message.Photo.Length - 1].FileId;

                var file = await Bot.GetFileAsync(fileId);

                var stream = file.FileStream;

                using (Stream output = new FileStream($"../../Photo/img{message.Chat.Id}{fileId}.jpg", FileMode.Append))
                {
                    byte[] buffer = new byte[32 * 1024];
                    int read;

                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, read);
                    }
                }

                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                var FileUrl = @"D:\\128.jpg";
                using (var streamm = System.IO.File.Open(FileUrl, FileMode.Open))
                {
                    FileToSend fts = new FileToSend();
                    fts.Content = streamm;
                    fts.Filename = FileUrl.Split('\\').Last();
                    var test = await Bot.SendStickerAsync(message.Chat.Id, fts);
                }

                string imagePath = $"../../Photo/img{message.Chat.Id}{fileId}.jpg";

                TextDetection newTD = new TextDetection();

                string text = newTD.photo2string(imagePath);

                string result = ParseString(text);
                Console.WriteLine(Convert.ToDouble(result));
                bill = Math.Abs(Convert.ToDouble(result));

                var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup();
                keyboard.InlineKeyboard = new Telegram.Bot.Types.InlineKeyboardButtons.InlineKeyboardButton[][]
                {
                    new Telegram.Bot.Types.InlineKeyboardButtons.InlineKeyboardButton[]
                    {
                        new KeyboardButton("Продукты питания"),
                        new KeyboardButton("Техника"),
                    },
                    new Telegram.Bot.Types.InlineKeyboardButtons.InlineKeyboardButton[]
                    {
                        new KeyboardButton("Транспорт"),
                        new KeyboardButton("Мобильная связь")
                    },
                    new Telegram.Bot.Types.InlineKeyboardButtons.InlineKeyboardButton[]
                    {
                        new KeyboardButton("Другое")
                    },
                };

                await Bot.SendTextMessageAsync(message.Chat.Id, "Выберите категорию товара, который вы приобрели", replyMarkup: keyboard);
                
                await Bot.SendTextMessageAsync(message.Chat.Id, WalletKeeper.Constants.IT_IS_DONE);

            }
            catch (Exception e)
            {
                await Bot.SendTextMessageAsync(message.Chat.Id, WalletKeeper.Constants.FAILED);
            }
        }
    }
}
