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

        private static readonly TelegramBotClient Bot = new TelegramBotClient(WalletKeeper.Config.API_KEY);

        static void Main(string[] args)
        {
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessage += BotOnPhotoReceived;

            var me = Bot.GetMeAsync().Result;
            Console.Title = me.Username;
       
            Bot.StartReceiving();
            BotOnPostReceived();
            Console.ReadLine();
            Bot.StopReceiving();
        }

        private static void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message == null || message.Type != MessageType.TextMessage)
                return;

            if (message.Text.StartsWith("/help"))
            {
                Bot.SendTextMessageAsync(message.Chat.Id, WalletKeeper.Constants.HELP_MESSAGE);
            }

            var keyboard2 = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup();
            keyboard2.InlineKeyboard = new Telegram.Bot.Types.InlineKeyboardButtons.InlineKeyboardButton[][]
            {
                    new Telegram.Bot.Types.InlineKeyboardButtons.InlineKeyboardButton[]
                    {
                        new KeyboardButton("по категориям"),
                        new KeyboardButton("по датам"),
                    },
            };

            bool flag = true;
            if (message.Text.StartsWith("/spending"))
            {
                    Bot.OnCallbackQuery += (object sc, Telegram.Bot.Args.CallbackQueryEventArgs ev) =>
                    {                        
                        if (ev.CallbackQuery.Data == "по категориям" && flag)
                        {
                            Bot.SendTextMessageAsync(message.Chat.Id, DataBaseCon.SelectRowsByCategory((int)message.Chat.Id));
                            Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id);
                            flag = !flag;
                        }
                        if (ev.CallbackQuery.Data == "по датам" && flag)
                        {
                            Bot.SendTextMessageAsync(message.Chat.Id, DataBaseCon.SelectRowsByDates((int)message.Chat.Id));
                            Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id);
                            flag = !flag;
                        }                
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

        private static string API_URL = "http://www.ispeech.org/p/generic/getaudio?text=%TEXT%&voice=rurussianfemale&speed=0&action=convert";

        public static string GetFinalUrl(string text)
        {
            return API_URL.Replace("%TEXT%", HttpUtility.UrlEncode(text));
        }

        public static byte[] GetSpeechFromText(string text)
        {
            WebClient wc = new WebClient();
            try
            {
                byte[] b = wc.DownloadData(GetFinalUrl(text));
                return b;
            }
            catch
            {
                return null;
            }
        }

        private static async void BotOnPhotoReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
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
                double result1 = Math.Abs(Convert.ToDouble(result));
           
               
                byte[] byteArray = GetSpeechFromText(result1.ToString());
                MemoryStream streamMy = new MemoryStream(byteArray);

                await Bot.SendVoiceAsync(message.Chat.Id, new FileToSend("audio.mp3", streamMy));



                DataBaseCon.InsertUser((int)message.Chat.Id, message.Chat.FirstName);
                DataBaseCon.InsertAmount((int)message.Chat.Id, result1);

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
                

                await Task.Delay(1000);

                bool flag = true;
                           
                Bot.OnCallbackQuery += (object sc, CallbackQueryEventArgs ev) =>
                {
                    Task.Delay(5000);
                    if (ev.CallbackQuery.Data == "Транспорт" && flag)
                    {
                        DataBaseCon.InsertCategory((int)message.Chat.Id, "Транспорт");
                        Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id, "Расходы на транспорт зафиксированы");
                        Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id);
                    }
                    else
                    if (ev.CallbackQuery.Data == "Мобильная связь" && flag)
                    {
                        DataBaseCon.InsertCategory((int)message.Chat.Id, "Мобильная связь");
                        Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id, "Расходы на мобильную связь зафиксированы");
                        Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id);
                    }
                    else
                    if (ev.CallbackQuery.Data == "Продукты питания" && flag)
                    {
                        DataBaseCon.InsertCategory((int)message.Chat.Id, "Продукты питания");
                        Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id, "Расходы на продукты питания зафиксированы");
                        Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id);

                    }
                    else
                    if (ev.CallbackQuery.Data == "Техника" && flag)
                    {
                        DataBaseCon.InsertCategory((int)message.Chat.Id, "Техника");
                        Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id, "Расходы на технику зафиксированы");
                        Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id);
                    }
                    else
                    if (ev.CallbackQuery.Data == "Другое" && flag)
                    {
                        DataBaseCon.InsertCategory((int)message.Chat.Id, "Другое");
                        Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id, "Расходы на другую категорию зафиксированы");
                        Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id);
                    }
                    flag = !flag;
                };
                
                if (flag)
                {  
                    await Bot.SendTextMessageAsync(message.Chat.Id, WalletKeeper.Constants.IT_IS_DONE);
                }
                               
            }
            catch (Exception e){
                await Bot.SendTextMessageAsync(message.Chat.Id, WalletKeeper.Constants.FAILED);            
            }
        }

        private static void BotOnPostReceived()
        {
            WebRequest request = WebRequest.Create("https://api.telegram.org/bot460028209:AAHcxwlE3XYAD9TjSWyphmqR_FCMl0QvDLM/sendMessage?chat_id=187064809&text=%D0%97%D0%B4%D0%B5%D1%81%D1%8C%20%D0%BC%D0%BE%D0%B3%D0%BB%D0%B0%20%D0%B1%D1%8B%D1%82%D1%8C%20%D0%B2%D0%B0%D1%88%D0%B0%20%D1%80%D0%B5%D0%BA%D0%BB%D0%B0%D0%BC%D0%B0%20))");
            request.Method = "POST";
            string sName = "sName= abracadabra";
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(sName);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;

            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }
            
        }
    }
}
