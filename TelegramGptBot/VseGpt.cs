using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramGptBot
{
    internal class VseGpt
    {
        public static async Task<string> GetAnswer(string prompt)
        {
            // ключ в VseGPT после регистрации
            string apiKey = "sk-or-vv-78c4ce3f634446da64d5c0f27ffcd7bcb8af0b28f5ac55e691235c5f797c71a6";
            // базовый url API
            string baseApiUrl = "https://api.vsegpt.ru/v1/";

            try
            {
                HttpClient client = new HttpClient();
                // добавление заголовка авторизации с токеном
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                // список сообщений для отправки, также для хранения контекста беседы
                List<dynamic> messages = new List<dynamic>();
                messages.Add(new { role = "user", content = prompt });

                // формирование тела запроса к VseGPT
                var requestData = new
                {
                    model = "openai/gpt-4o-mini",
                    messages = messages,
                    temperature = 0.3,
                    n = 1,
                    max_tokens = 2000,
                    extra_headers = new { X_Title = "My App" } // опционально - передача информации об источнике API-вызова
                };

                // сериализация тела запроса в строку json
                string jsonRequest = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // запрос ответа
                HttpResponseMessage response = await client.PostAsync(baseApiUrl + "chat/completions", content);

                // если ответ сервера успешный, то получаем из тела запроса ответ и возвращаем
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    dynamic responseData = JsonConvert.DeserializeObject(jsonResponse);
                    string responseContent = responseData.choices[0].message.content;
                    Console.WriteLine("Response: " + responseContent);
                    return responseContent;
                }
                // если ответ сервера не успешный, то возвращаем сообщение об ошибке от сервера
                else
                {
                    return "Error: " + response.ReasonPhrase;
                }
            }
            catch (Exception ex)
            {
                // если произошло исключение, то возвращаем текст ошибки
                return "Exception: " + ex.Message;
            }
        }
    }
}
