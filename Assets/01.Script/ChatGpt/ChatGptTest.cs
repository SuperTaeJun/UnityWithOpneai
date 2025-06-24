using OpenAI.Chat;
using OpenAI.Models;
using OpenAI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenAI.Images;
using Utilities.Extensions;



public class ChatGptTest : MonoBehaviour
{
    public RawImage Image;

    public Text StroyDescriptionTextUI;
    public Transform ChatContentParent;       // Content ������Ʈ
    public GameObject ChatBubblePrefab;       // ä�� �޽��� ������
    public ScrollRect scrollRect;

    public InputField PromptField;  // �Է� �ʵ�
    public UnityEngine.UI.Button SendButton;       // ������ ��ư
    public UnityEngine.UI.Button ResultButton;
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
        //ResultTextUI.text += $"\n<color=grey>[��]</color> {prompt}";
        //ResultTextUI.text += $"\n<color=#00aaff>[����]</color> {npcResponse.ReplyMessage} \n";
        AddChatBubble("��", prompt, Color.gray);
        AddChatBubble("����", npcResponse.ReplyMessage, new Color(0f, 0.67f, 1f)); // �ϴû�

        ScrollToBottom();
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
        string currentEmotion = EmotionText.text;
        string currentInnerThink = InnerthoughtsText.text;
        string resultPrompt = $@"
������ ����� ���� �� �̷� ������ ������ �־���: '{currentEmotion}'  
�׸��� ������ �̷��� �����ߴ�: '{currentInnerThink}'

�� ������ �Ӹ����� ��������, ������� ���� �� � ����� ���Դ����� 1~2�������� �������.

�� ���� ���°� �������̸� ����� �ڽŰ� �ְ� ���������,  
�� ���� ���°� �Ҿ��ϰų� �������̸� ��� ����� ������� �ݿ��ǵ��� �ڿ������� ������.

'����', '�츮 ��', '�����' ���� ǥ���� ������ �������� ������ �ؼ� ��Ÿ�Ϸ� ����.
";

        List<Message> resultMessage = new List<Message>();
        resultMessage.Add(new Message(Role.User, resultPrompt));

        var chatRequest = new ChatRequest(resultMessage, Model.GPT4o);

        var response = await api.ChatEndpoint.GetCompletionAsync(chatRequest);

        AddChatBubble("��� ���", response.FirstChoice.Message, new Color(1f, 0.0f, 0f)); // �ϴû�

    }

    private void AddChatBubble(string speaker, string message, Color nameColor)
    {
        var bubble = Instantiate(ChatBubblePrefab, ChatContentParent);
        var text = bubble.GetComponent<Text>();
        text.text = $"<color=#{ColorUtility.ToHtmlStringRGB(nameColor)}>[{speaker}]</color> {message}";

    }
    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases(); // UI ������Ʈ ���� �ݿ�
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
