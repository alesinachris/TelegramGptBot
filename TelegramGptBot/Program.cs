using System;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramGptBot
{
    internal class Program
    {
        private static ITelegramBotClient _botClient;
        private static ReceiverOptions _receiverOptions;

        static async Task Main(string[] args)
        {
            // создание объекта клиента для работы с телеграм
            _botClient = new TelegramBotClient("7701907682:AAG48gxLlhxUW1rkEAZEdVqOyEkJMhZnKWs");
            // создание объекта настроек получения обновлений
            _receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] // типы получаемых обновлений
                {
                    UpdateType.Message,
                    UpdateType.CallbackQuery
                },
                ThrowPendingUpdates = true,
            };

            // токен отмены (не используется, просто как заглушка используется)
            using var cts = new CancellationTokenSource();
            // старт получения обновлений
            _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token);

            // тест запуска бота
            var me = await _botClient.GetMeAsync();
            // вывод результата теста запуска бота
            Console.WriteLine($"{me.FirstName} запущен!");

            // бесконечное ожидание, чтобы консоль не закрылась
            await Task.Delay(-1);
        }

        /// <summary>
        /// Обработчик обновлений от телеграм
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                // Если тип сообщения UpdateType.Message, то обрабатываем сообщение
                if (update.Type == UpdateType.Message)
                {
                    // вывод сообщения пользователя
                    Console.WriteLine(update.Message.Text);
                    // получение идентификатора чата, из которго пришло сообщение
                    Chat chat = update.Message.Chat;
                    // если стартовое сообщение, то выводим приветствие
                    if(update.Message.Text == "/start")
                    {
                        await Start(botClient, chat);
                    }
                    // если не стартовое сообщение, то считаем, что это вопрос
                    else
                    {
                        // если сообщение слишком длинное, более 500 символов, то выводим сообщение об ошибке
                        if (update.Message.Text.Length > 500)
                        {
                            await VeryLargeText(botClient, chat);
                        }
                        // если сообщение подходящей длины, то отправляем вопрос в VseGPT и выводим ответ
                        else
                        {
                            // получение ответа
                            string answer = await VseGpt.GetAnswer(update.Message.Text);
                            // вывод ответа ботом
                            await botClient.SendTextMessageAsync(chat.Id, answer);
                            await botClient.SendTextMessageAsync(chat.Id, "Ещё вопрос?");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // если в коде происходит исключение, то выводим в консоль исключение
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Отправка стартового сообщения
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="chat"></param>
        /// <returns></returns>
        private static async Task Start(ITelegramBotClient botClient, Chat chat)
        {
            await botClient.SendTextMessageAsync(chat.Id, "Привет, я модель GPT-4o-mini, ваш личный помощник! Задайте ваш вопрос.");
        }

        /// <summary>
        /// Отправка сообщения о слишком длинном тексте
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="chat"></param>
        /// <returns></returns>
        private static async Task VeryLargeText(ITelegramBotClient botClient, Chat chat)
        {
            await botClient.SendTextMessageAsync(chat.Id, "Слишком длинный вопрос.");
        }

        private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
        {
            var ErrorMessage = error switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => error.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
