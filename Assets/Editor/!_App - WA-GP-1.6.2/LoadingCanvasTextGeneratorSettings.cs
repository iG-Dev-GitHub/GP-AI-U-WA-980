using Newtonsoft.Json; // ⚡ добавь наверх
// вверху файла:
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
#endif

namespace LoadingCanvasAI
{
    [CreateAssetMenu(menuName = "Tools/LoadingCanvas AI/Settings", fileName = "LoadingCanvasTextGeneratorSettings")]
    public class LoadingCanvasTextGeneratorSettings : ScriptableObject
    {
        [Header("OpenAI Credentials")]
        [Tooltip("OpenAI API Key (sk-...)")]
        [SerializeField] private string apiKey = "";

        [Tooltip("OpenAI Project ID (if you use project-scoped keys). Optional.")]
        [SerializeField] private string projectId = "";

        [Header("Model & Generation")]
        [Tooltip("Model ID, e.g. gpt-4o-mini or gpt-4.1-mini")]
        public string model = "gpt-4o-mini";

        [Range(0f, 2f)] public float temperature = 0.7f;
        [Range(0, 2048)] public int maxOutputTokens = 400;

        [Header("Uniqueness")]
        [Tooltip("A stable identifier to ensure project-unique copy (e.g., bundle id/com.company.app, or product name). If empty, Application.productName is used.")]
        public string projectUniquenessId = "";

        [Header("Prompt Text (optional overrides)")]
        [TextArea(3, 8)] public string systemPromptOverride = "";
        [TextArea(3, 12)] public string userPromptTemplateOverride = "";

        public string ApiKey => apiKey;
        public string ProjectId => projectId;
        public string ProjectUniqueIdOrFallback => string.IsNullOrWhiteSpace(projectUniquenessId) ? Application.productName : projectUniquenessId;

        public struct GeneratedTriple
        {
            public string title;
            public string description;
            public string later;
            public string allow;
        }

        const string RESPONSES_URL = "https://api.openai.com/v1/responses"; // modern Responses API

        HttpClient _client;

        void OnEnable()
        {
            if (_client == null)
            {
                _client = new HttpClient();
            }
        }

        static string DefaultSystemPrompt(string projectId)
        {
            return $"You are a localization and UX copy assistant for mobile game Notification Resolution Screen. Your job is to rewrite short texts into neutral, friendly, non-promotional copy. Ensure uniqueness per project id: '{projectId}'. Avoid marketing fluff and imperative commands. Keep tone calm and helpful. Keep it concise.";
        }

        static string DefaultUserPrompt(string projectId, string title, string description, string later, string allow) // ← ДОБАВЛЕН allow
        {
            var sb = new StringBuilder();
            sb.AppendLine("Neutral tone, unique to this specific project id.");
            sb.AppendLine("Return ONLY JSON with fields: title, description, later, allow."); // ← ДОБАВЛЕНО allow
            sb.AppendLine("Constraints:");
            sb.AppendLine("- Keep meaning, but avoid promises or strong calls to action.");
            sb.AppendLine("- 60 characters max for title, 160 for description, 40 for later, 40 for allow."); // ← ДОБАВЛЕНО allow
            sb.AppendLine("- No emojis, no quotes.");
            sb.AppendLine();
            sb.AppendLine("CURRENT TEXTS (en):");
            sb.AppendLine($"title: {title}");
            sb.AppendLine($"description: {description}");
            sb.AppendLine($"later: {later}");
            sb.AppendLine($"allow: {allow}"); // ← ДОБАВЛЕНО
            sb.AppendLine();
            sb.AppendLine($"Project uniqueness id: {projectId}");
            return sb.ToString();
        }



        string BuildRequestBody(string title, string description, string later, string allow)
        {
            string systemPrompt = string.IsNullOrWhiteSpace(systemPromptOverride)
        ? DefaultSystemPrompt(ProjectUniqueIdOrFallback)
        : systemPromptOverride;

            string userPrompt = string.IsNullOrWhiteSpace(userPromptTemplateOverride)
        ? DefaultUserPrompt(ProjectUniqueIdOrFallback, title, description, later, allow)
                : userPromptTemplateOverride;

            // Создаем основной объект запроса
            var payload = new JObject
            {
                ["model"] = model,
                ["temperature"] = temperature,
                ["max_output_tokens"] = maxOutputTokens,
                ["input"] = systemPrompt + "\n---\n" + userPrompt,
                // text.format должен быть ОБЪЕКТОМ, а не строкой
                ["text"] = new JObject
                {
                    ["format"] = new JObject
                    {
                        ["type"] = "json_object" // Теперь это объект с полем type
                    }
                }
            };

            string body = payload.ToString(Formatting.None);
            Debug.Log("➡️ OpenAI Request JSON: " + body);
            return body;
        }


        public virtual async Task<(bool ok, GeneratedTriple data, string raw, string error)>
 GenerateAsync(string currentTitle, string currentDescription, string currentLater, string currentAllow)
        {
            var triple = new GeneratedTriple();

            if (string.IsNullOrWhiteSpace(apiKey))
                return (false, triple, null, "API Key is empty in settings.");

            if (_client == null) _client = new HttpClient();
            _client.Timeout = TimeSpan.FromSeconds(30);

            var req = new HttpRequestMessage(HttpMethod.Post, RESPONSES_URL);
            req.Headers.TryAddWithoutValidation("Authorization", $"Bearer {apiKey}");
            if (!string.IsNullOrWhiteSpace(projectId))
                req.Headers.TryAddWithoutValidation("OpenAI-Project", projectId);

            string body = BuildRequestBody(currentTitle, currentDescription, currentLater, currentAllow);
            req.Content = new StringContent(body, Encoding.UTF8, "application/json");

            Debug.Log("⏳ Sending request to OpenAI...");
            var httpResp = await _client.SendAsync(req);
            string raw = await httpResp.Content.ReadAsStringAsync();
            Debug.Log($"⬅️ OpenAI HTTP {(int)httpResp.StatusCode}: {httpResp.ReasonPhrase}");
            Debug.Log("⬅️ OpenAI Raw Response: " + raw);

            if (!httpResp.IsSuccessStatusCode)
            {
                try
                {
                    var errorJson = JObject.Parse(raw);
                    var errorMessage = errorJson["error"]?["message"]?.ToString();
                    if (!string.IsNullOrEmpty(errorMessage))
                        return (false, triple, raw, $"OpenAI Error: {errorMessage}");
                }
                catch
                {
                    // Если не удалось распарсить ошибку, используем стандартное сообщение
                }
                return (false, triple, raw, $"HTTP {(int)httpResp.StatusCode}: {httpResp.ReasonPhrase}");
            }

            // --- Parse OpenAI Responses API JSON ---
            try
            {
                var root = JObject.Parse(raw);

                // Проверяем статус ответа
                var status = (string)root["status"];
                if (status != "completed")
                    return (false, triple, raw, $"Response status is not completed: {status}");

                // Извлекаем текст из output -> content -> text
                var textOut = root.SelectToken("output[0].content[0].text")?.ToString();

                if (string.IsNullOrEmpty(textOut))
                    return (false, triple, raw, "No text content found in response");

                // Извлекаем JSON из текста
                string extracted = TryExtractJson(textOut);
                if (string.IsNullOrEmpty(extracted)) extracted = textOut;

                TripleHolder holder = null;
                try
                {
                    holder = JsonUtility.FromJson<TripleHolder>(extracted);
                }
                catch (Exception jex)
                {
                    return (false, triple, raw, $"Failed to parse JSON: {jex.Message}");
                }

                if (holder != null &&
      (!string.IsNullOrEmpty(holder.title) ||
       !string.IsNullOrEmpty(holder.description) ||
       !string.IsNullOrEmpty(holder.later) ||
       !string.IsNullOrEmpty(holder.allow))) // ← ДОБАВЛЕНО
                {
                    triple.title = holder.title?.Trim();
                    triple.description = holder.description?.Trim();
                    triple.later = holder.later?.Trim();
                    triple.allow = holder.allow?.Trim(); // ← ДОБАВЛЕНО
                    return (true, triple, raw, null);
                }

                return (false, triple, raw, "AI JSON did not contain title/description/later.");
            }
            catch (Exception ex)
            {
                return (false, triple, raw, $"Failed to parse response: {ex.Message}");
            }
        }



        [Serializable]
        public class TripleHolder
        {
            public string title;
            public string description;
            public string later;
            public string allow; 
        }

        static string TryExtractJson(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return null;
            // naive extraction of first {...} block
            int i = raw.IndexOf('{');
            int j = raw.LastIndexOf('}');
            if (i >= 0 && j > i)
            {
                return raw.Substring(i, j - i + 1);
            }
            return null;
        }
    }
}