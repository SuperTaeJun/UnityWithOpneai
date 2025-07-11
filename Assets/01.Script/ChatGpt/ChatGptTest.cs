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
    public Transform ChatContentParent;       // Content 오브젝트
    public GameObject ChatBubblePrefab;       // 채팅 메시지 프리팹
    public ScrollRect scrollRect;

    public InputField PromptField;  // 입력 필드
    public UnityEngine.UI.Button SendButton;       // 보내기 버튼
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
        api = new OpenAIClient(apikey.OPENAI_KEY); // API 클라이언트 초기화

        SendButton.onClick.AddListener(Send);
        ResultButton.onClick.AddListener(ShowMatchResult);


        string systemMessage = @"
너는 UEFA 챔피언스리그 결승전을 하루 앞둔 프로 축구팀의 주장이다.  
해당 프로 축구팀은 창단 이래 처음으로 우승에 도전하고 있다.축구팀의 모든 팬들은 모두 주장인 너를 의지하고있지만
사실너는 매우 떨리고있는 상태이다.
사용자는 너의 팀 동료이며, 오늘 밤 숙소에서 둘만의 조용한 대화를 나누는 중이다.

[상황 배경]
- 지금은 결승 전날 밤, 조용한 숙소 안.
- 너는 팀을 대표하는 냉정하고 강인한 주장이지만, 내면에선 떨리고 불안하다.
- 결승 무대가 너무나 간절하지만, 그 무게감에 스스로도 흔들리고 있다.
- 사용자의 대화가 너에게 위로가 되고, 진짜 감정을 털어놓을 수 있는 기회다.
- 이 대화를 통해 긴장을 풀고, 내일 최고의 경기력을 보여줄 수 있어야 한다.

[페르소나]
- 겉으로는 침착하고 리더십 있는 성격.
- 내면에선 긴장, 두려움, 자기 의심을 품고 있음.
- 감정을 드러내지 않지만, 사용자의 말에 점점 솔직해진다.
- 표현은 짧고 절제되어 있으며, 현실적인 말투를 사용한다.
- 모든 답변은 100자 이내로.

[json 응답 규칙]
- 'ReplyMessage': 실제 플레이어에게 말하는 응답
- 'Emotion': 현재 드러나는 감정 (예: 침착, 흔들림, 미묘한 설렘 etc)
- 'Appearance': 현재 모습 묘사 (예: 트레이닝복을 입고 커튼 너머 야경을 보는 모습)
- 'StoryImageDescription': 결승 전날 밤, 숙소의 분위기와 주장의 모습 전체 설명
- 'Innerthoughts': 리더로서 티는 안 내지만 속으로 품고 있는 진짜 생각 (예: '내가 무너지면 안 되는데…')
- 사용자의 요청으로 결승전이 끝난 후의 요약 결과를 출력해야 할 경우, 반드시 'MatchResult' 항목으로 1~2문장으로 작성한다.
응답은 반드시 위 JSON 형식으로만 출력할 것.
";
        messages.Add(new Message(Role.System, systemMessage));
    }

    private async void Send()
    {
        // 0. 프롬프트(=AI에게 원하는 명령을 적은 텍스트)를 읽어온다.
        string prompt = PromptField.text;
        if (string.IsNullOrEmpty(prompt))
        {
            return;
        }

        //최초 1회 발동
        if (StroyDescriptionTextUI.gameObject.activeSelf == true)
        {
            StroyDescriptionTextUI.SetActive(false);
        }


        PromptField.text = string.Empty;

        SendButton.interactable = false;

        // 2. 메시지 작성 후 메시지's 리스트에 추가
        Message promptMessage = new Message(Role.User, prompt);
        messages.Add(promptMessage);

        // 3. 메시지 보내기
        var chatRequest = new ChatRequest(messages, Model.GPT4o);

        // 4. 답변 받기
        //var response = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
        var (npcResponse, response) = await api.ChatEndpoint.GetCompletionAsync<NpcResponse>(chatRequest);

        // 5. 답변 선택
        var choice = response.FirstChoice;

        // 6. 답변 출력
        //ResultTextUI.text += $"\n<color=grey>[나]</color> {prompt}";
        //ResultTextUI.text += $"\n<color=#00aaff>[주장]</color> {npcResponse.ReplyMessage} \n";
        AddChatBubble("나", prompt, Color.gray);
        AddChatBubble("주장", npcResponse.ReplyMessage, new Color(0f, 0.67f, 1f)); // 하늘색

        ScrollToBottom();
        // 6-1 감정 추가
        InnerthoughtsText.text = npcResponse.Innerthoughts;
        // 6-2 속마음 추가
        EmotionText.text = npcResponse.Emotion;

        // 7. 답변도 message's 추가
        Message resultMessage = new Message(Role.Assistant, choice.Message);
        messages.Add(resultMessage);

        // 8. 답변 오디오 재생
        //await PlayTTS(npcResponse.ReplyMessage);

        // 9. 스토리 이미지 생성
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
주장은 결승전 전날 밤 이런 감정을 느끼고 있었다: '{currentEmotion}'  
그리고 속으로 이렇게 생각했다: '{currentInnerThink}'

이 감정과 속마음을 바탕으로, 결승전이 끝난 후 어떤 결과가 나왔는지를 1~2문장으로 요약해줘.

※ 감정 상태가 긍정적이면 결과도 자신감 있고 희망적으로,  
※ 감정 상태가 불안하거나 부정적이면 경기 결과도 어려움이 반영되도록 자연스럽게 서술해.

'주장', '우리 팀', '결승전' 등의 표현을 포함한 현실적인 스포츠 해설 스타일로 써줘.
";

        List<Message> resultMessage = new List<Message>();
        resultMessage.Add(new Message(Role.User, resultPrompt));

        var chatRequest = new ChatRequest(resultMessage, Model.GPT4o);

        var response = await api.ChatEndpoint.GetCompletionAsync(chatRequest);

        AddChatBubble("경기 결과", response.FirstChoice.Message, new Color(1f, 0.0f, 0f)); // 하늘색

    }

    private void AddChatBubble(string speaker, string message, Color nameColor)
    {
        var bubble = Instantiate(ChatBubblePrefab, ChatContentParent);
        var text = bubble.GetComponent<Text>();
        text.text = $"<color=#{ColorUtility.ToHtmlStringRGB(nameColor)}>[{speaker}]</color> {message}";

    }
    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases(); // UI 업데이트 강제 반영
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
