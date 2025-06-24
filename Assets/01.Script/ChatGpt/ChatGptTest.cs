using OpenAI.Chat;
using OpenAI.Models;
using OpenAI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

using OpenAI.Images;
using Utilities.Extensions;



public class ChatGptTest : MonoBehaviour
{
    public RawImage Image;

    public Text StroyDescriptionTextUI;
    public Text ResultTextUI;       // ��� �ؽ�Ʈ
    public InputField PromptField;  // �Է� �ʵ�
    public Button SendButton;       // ������ ��ư
    public Button ResultButton;
    public Text EmotionText;
    public Text InnerthoughtsText;

    private APIKey apikey;
    OpenAIClient api;

    List<Message> messages;

    public AudioSource source;

    private void Start()
    {
        apikey = new APIKey();
        messages = new List<Message>();
        api = new OpenAIClient(apikey.OPENAI_KEY); // API Ŭ���̾�Ʈ �ʱ�ȭ

        SendButton.onClick.AddListener(Send);
        ResultButton.onClick.AddListener(ShowMatchResult);


        string systemMessage = @"
�ʴ� UEFA è�Ǿ𽺸��� ������� �Ϸ� �յ� ���� �౸���� �����̴�.  
�ش� ���� �౸���� â�� �̷� ó������ ��¿� �����ϰ� �ִ�.�౸���� ��� �ҵ��� ��� ������ �ʸ� �����ϰ�������
��ǳʴ� �ſ� �������ִ� �����̴�.
����ڴ� ���� �� �����̸�, ���� �� ���ҿ��� �Ѹ��� ������ ��ȭ�� ������ ���̴�.

[��Ȳ ���]
- ������ ��� ���� ��, ������ ���� ��.
- �ʴ� ���� ��ǥ�ϴ� �����ϰ� ������ ����������, ���鿡�� ������ �Ҿ��ϴ�.
- ��� ���밡 �ʹ��� ����������, �� ���԰��� �����ε� ��鸮�� �ִ�.
- ������� ��ȭ�� �ʿ��� ���ΰ� �ǰ�, ��¥ ������ �о���� �� �ִ� ��ȸ��.
- �� ��ȭ�� ���� ������ Ǯ��, ���� �ְ��� ������ ������ �� �־�� �Ѵ�.

[�丣�ҳ�]
- �����δ� ħ���ϰ� ������ �ִ� ����.
- ���鿡�� ����, �η���, �ڱ� �ǽ��� ǰ�� ����.
- ������ �巯���� ������, ������� ���� ���� ����������.
- ǥ���� ª�� �����Ǿ� ������, �������� ������ ����Ѵ�.
- ��� �亯�� 100�� �̳���.

[json ���� ��Ģ]
- 'ReplyMessage': ���� �÷��̾�� ���ϴ� ����
- 'Emotion': ���� �巯���� ���� (��: ħ��, ��鸲, �̹��� ���� etc)
- 'Appearance': ���� ��� ���� (��: Ʈ���̴׺��� �԰� Ŀư �ʸ� �߰��� ���� ���)
- 'StoryImageDescription': ��� ���� ��, ������ ������� ������ ��� ��ü ����
- 'Innerthoughts': �����μ� Ƽ�� �� ������ ������ ǰ�� �ִ� ��¥ ���� (��: '���� �������� �� �Ǵµ���')
- ������� ��û���� ������� ���� ���� ��� ����� ����ؾ� �� ���, �ݵ�� 'MatchResult' �׸����� 1~2�������� �ۼ��Ѵ�.
������ �ݵ�� �� JSON �������θ� ����� ��.
";
        messages.Add(new Message(Role.System, systemMessage));
    }

    private async void Send()
    {
        // 0. ������Ʈ(=AI���� ���ϴ� ����� ���� �ؽ�Ʈ)�� �о�´�.
        string prompt = PromptField.text;
        if (string.IsNullOrEmpty(prompt))
        {
            return;
        }

        //���� 1ȸ �ߵ�
        if (StroyDescriptionTextUI.gameObject.activeSelf == true)
        {
            StroyDescriptionTextUI.SetActive(false);
            ResultTextUI.SetActive(true);
        }


        PromptField.text = string.Empty;

        SendButton.interactable = false;

        // 2. �޽��� �ۼ� �� �޽���'s ����Ʈ�� �߰�
        Message promptMessage = new Message(Role.User, prompt);
        messages.Add(promptMessage);

        // 3. �޽��� ������
        var chatRequest = new ChatRequest(messages, Model.GPT4o);

        // 4. �亯 �ޱ�
        //var response = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
        var (npcResponse, response) = await api.ChatEndpoint.GetCompletionAsync<NpcResponse>(chatRequest);

        // 5. �亯 ����
        var choice = response.FirstChoice;

        // 6. �亯 ���
        ResultTextUI.text += $"\n<color=grey>[��]</color> {prompt}";
        ResultTextUI.text += $"\n<color=#00aaff>[����]</color> {npcResponse.ReplyMessage} \n";

        // 6-1 ���� �߰�
        InnerthoughtsText.text = npcResponse.Innerthoughts;
        // 6-2 �Ӹ��� �߰�
        EmotionText.text = npcResponse.Emotion;

        // 7. �亯�� message's �߰�
        Message resultMessage = new Message(Role.Assistant, choice.Message);
        messages.Add(resultMessage);

        // 8. �亯 ����� ���
        //await PlayTTS(npcResponse.ReplyMessage);

        // 9. ���丮 �̹��� ����
        GenerateImage(npcResponse.StoryImageDescription);
        SendButton.interactable = true;

    }
    private async void GenerateImage(string text)
    {
        var api = new OpenAIClient();
        var request = new ImageGenerationRequest(text, Model.DallE_3);
        var imageResults = await api.ImagesEndPoint.GenerateImageAsync(request);

        foreach (var result in imageResults)
        {
            Image.texture = result.Texture;
        }
    }

    private async void ShowMatchResult()
    {
        // ��ư Ŭ�� ��, GPT���� ��� ��� ��û
        string resultPrompt = @"
���� ������� �����ٰ� �����ϰ�, ������ ������ �Ӹ����� �������� 'MatchResult' �׸� ��� ����� 1~2�������� ����� JSON �������� �������.
������ �ݵ�� ���� ��������:

{
  ""MatchResult"": ""���⿡ ��� ��� ����� �ۼ�""
}
";
        messages.Add(new Message(Role.User, resultPrompt));

        var (resultResponse, _) = await api.ChatEndpoint.GetCompletionAsync<NpcResponse>(new ChatRequest(messages, Model.GPT4o));

        // ��� ���
        ResultTextUI.text = $"\n\n<color=#ffaa00><b>[��� ���]</b></color> {resultResponse.MatchResult}";

        Debug.Log(resultResponse.MatchResult);
    }
}
