using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletKeeper
{
    class Constants
    {
        public const string START_MESSAGE =
        @"! Я твой персональный помощник  по учету твоих расходов. " + 
        "Ты можешь мне отправить фотографию чека из магазина, кафе, кинотеатра, я её обработую и запишу потраченную сумму для " + 
        "для того, чтобы через какое-то время ты смог посмотреть свои траты. " +
        "Набери /help для детальной инструкции.";

        public const string HELP_MESSAGE =
        @"Пришли мне фотографию чека. " + 
        "Набери /spending для получения детальной информации о тратах " +
        "или /delete если ты хочешь удалить информацию.";

        public const string IN_PROGRESS = @"Подожди немного, я начал обрабатывать...";
        public const string IT_IS_DONE = @"Спасибо! Выполнено! Набери /spending для получения детальной информации о тратах.";
        public const string DELETE_DONE = @"Выполнено!";
        public const string FAILED = @"Фотографию не удалось обработать.";
    }
}
