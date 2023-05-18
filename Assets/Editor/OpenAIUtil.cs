using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;

namespace AIComponent {

static class OpenAIUtil
{
    static string CreateChatRequestBody(string prompt)
    {
        var msg = new OpenAI.RequestMessage();
        msg.role = "user";
        msg.content = prompt;

        var req = new OpenAI.Request();
        req.model = "gpt-3.5-turbo";
        req.messages = new [] { msg };

        return JsonUtility.ToJson(req);
    }

    public static string InvokeChat(string prompt)
    {
        var settings = AIComponentSettings.instance;
        Debug.Log(prompt);
        // POST
        using var post = UnityWebRequest.Post
          (OpenAI.Api.GetUrl(), CreateChatRequestBody(prompt), "application/json");

        // Request timeout setting
        post.timeout = settings.timeout;

        // API key authorization
        post.SetRequestHeader("Authorization", "Bearer "  + settings.apiKey);

        // Request start
        var req = post.SendWebRequest();

        // Progress bar (Totally fake! Don't try this at home.)
        for (var progress = 0.0f; !req.isDone; progress += 0.01f)
        {
            EditorUtility.DisplayProgressBar
              ("AI Component", "Generating...", progress);
            System.Threading.Thread.Sleep(100);
            progress += 0.01f;
        }
        EditorUtility.ClearProgressBar();

        // Response extraction
        var json = post.downloadHandler.text;
        var data = JsonUtility.FromJson<OpenAI.Response>(json);
        var resText = data.choices[0].message.content;
        Debug.Log(resText);
        return resText;
    }
}

} // namespace AIComponent
